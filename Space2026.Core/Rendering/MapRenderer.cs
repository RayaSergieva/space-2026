using System.Text;
using Space2026.Core.Models;
using Space2026.Core.Pathfinding;

namespace Space2026.Core.Rendering;

/// <summary>
/// Produces the text view of a map, optionally overlaying an astronaut's
/// journey with '*' on the cells between the launch pod and the station —
/// both endpoints keep their original labels, exactly as the brief shows.
/// Returns a plain string so the same rendering feeds the console, an email
/// report, a file, or any future UI.
/// </summary>
public static class MapRenderer
{
    public const char PathMarker = '*';

    public static string Render(Grid grid, PathResult? path = null)
    {
        var overlay = BuildOverlay(path);
        var builder = new StringBuilder();

        for (var r = 0; r < grid.Rows; r++)
        {
            for (var c = 0; c < grid.Columns; c++)
            {
                if (c > 0) builder.Append(' ');

                var position = new Position(r, c);
                builder.Append(overlay.Contains(position)
                    ? PathMarker.ToString()
                    : grid.SymbolAt(position));
            }

            if (r < grid.Rows - 1) builder.Append('\n');
        }

        return builder.ToString();
    }

    private static HashSet<Position> BuildOverlay(PathResult? path)
    {
        if (path is not { Found: true } || path.Path.Count <= 2)
            return new HashSet<Position>();

        // Endpoints keep their own labels; only the interior gets stars.
        return new HashSet<Position>(path.Path.Skip(1).Take(path.Path.Count - 2));
    }
}