using Space2026.Core.Models;
using Space2026.Core.Navigation;
using Space2026.Core.Pathfinding;

namespace Space2026.Console;

/// <summary>
/// Console-only presentation: prints missions with colour so the map reads at
/// a glance — asteroids dark red, debris dark yellow, astronauts cyan, the
/// station green and the route a bright yellow '*'. Core's MapRenderer stays
/// plain-text; colour is strictly an edge concern of this project.
/// </summary>
internal static class ConsoleRenderer
{
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
        var overlay = path.Path.Count > 2
            ? new HashSet<Position>(path.Path.Skip(1).Take(path.Path.Count - 2))
            : new HashSet<Position>();

        for (var r = 0; r < grid.Rows; r++)
        {
            for (var c = 0; c < grid.Columns; c++)
            {
                if (c > 0) System.Console.Write(' ');

                var position = new Position(r, c);
                if (overlay.Contains(position))
                    Write("*", ConsoleColor.Yellow);
                else
                    Write(grid.SymbolAt(position), ColourFor(grid, position));
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
            CellType.Asteroid => ConsoleColor.DarkRed,
            CellType.Debris => ConsoleColor.DarkYellow,
            _ => ConsoleColor.DarkGray
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