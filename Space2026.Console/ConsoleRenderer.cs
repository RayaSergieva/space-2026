using Space2026.Core.Models;
using Space2026.Core.Navigation;
using Space2026.Core.Pathfinding;

namespace Space2026.Console;

/// <summary>
/// Console-only presentation: prints missions with colour, and traces each
/// route star by star like a live plot from mission control. The animation
/// redraws the whole map per frame instead of repositioning the cursor into
/// individual cells — redrawing is immune to console scrolling, which made
/// the previous cell-poking approach crash once output filled the window.
/// Animation is skipped when output is redirected (pipes, files, CI).
/// </summary>
internal static class ConsoleRenderer
{
    private const int TraceDelayMilliseconds = 350;

    public static void PrintMissions(Grid grid, IReadOnlyList<MissionResult> results)
    {
        foreach (var failure in results.Where(r => !r.Succeeded))
            WriteLine($"Mission failed \u2014 Astronaut {failure.Astronaut.Name} lost in space!", ConsoleColor.Red);

        if (results.Any(r => !r.Succeeded))
            System.Console.WriteLine();

        foreach (var result in results.Where(r => r.Succeeded))
        {
            WriteLine($"Astronaut {result.Astronaut.Name} - Shortest path: {result.Result.Cost} steps", ConsoleColor.White);
            PrintGrid(grid, result.Result);
            System.Console.WriteLine();
        }
    }

    private static void PrintGrid(Grid grid, PathResult path)
    {
        var interior = path.Path.Count > 2
            ? path.Path.Skip(1).Take(path.Path.Count - 2).ToList()
            : new List<Position>();

        if (System.Console.IsOutputRedirected || interior.Count == 0)
        {
            DrawFrame(grid, new HashSet<Position>(interior));
            return;
        }

        try
        {
            AnimateTrace(grid, interior);
        }
        catch (Exception)
        {
            // Terminals that disallow cursor control must never crash the app;
            // fall back to printing the finished map.
            DrawFrame(grid, new HashSet<Position>(interior));
        }
    }

    private static void AnimateTrace(Grid grid, List<Position> interior)
    {
        var revealed = new HashSet<Position>();

        // Frame 0: the map with no route yet.
        var frameTop = System.Console.CursorTop;
        DrawFrame(grid, revealed);

        foreach (var position in interior)
        {
            Thread.Sleep(TraceDelayMilliseconds);
            revealed.Add(position);

            // If output scrolled, the map's first row moved up; re-measure it
            // from where the cursor now is (one map-height above).
            frameTop = Math.Max(0, System.Console.CursorTop - grid.Rows);
            System.Console.SetCursorPosition(0, frameTop);
            DrawFrame(grid, revealed);
        }
    }

    private static void DrawFrame(Grid grid, HashSet<Position> overlay)
    {
        for (var r = 0; r < grid.Rows; r++)
        {
            for (var c = 0; c < grid.Columns; c++)
            {
                if (c > 0) System.Console.Write(' ');

                var position = new Position(r, c);
                if (overlay.Contains(position))
                    Write("*".PadRight(2), ConsoleColor.Yellow);
                else
                    Write(grid.SymbolAt(position).PadRight(2), ColourFor(grid, position));
            }
            System.Console.WriteLine();
        }
    }

    private static ConsoleColor ColourFor(Grid grid, Position position)
    {
        var symbol = grid.SymbolAt(position);
        if (symbol == "F") return ConsoleColor.Green;
        if (symbol.StartsWith('S')) return ConsoleColor.Cyan;

        return grid.TerrainAt(position) switch
        {
            CellType.Asteroid => ConsoleColor.Red,
            CellType.Debris => ConsoleColor.Yellow,
            _ => ConsoleColor.Gray
        };
    }

    private static void Write(string text, ConsoleColor colour)
    {
        var previous = System.Console.ForegroundColor;
        System.Console.ForegroundColor = colour;
        System.Console.Write(text);
        System.Console.ForegroundColor = previous;
    }

    private static void WriteLine(string text, ConsoleColor colour)
    {
        Write(text, colour);
        System.Console.WriteLine();
    }
}