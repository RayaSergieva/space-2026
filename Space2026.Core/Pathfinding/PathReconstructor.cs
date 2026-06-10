using Space2026.Core.Models;

namespace Space2026.Core.Pathfinding;

/// <summary>
/// Shared helper that walks a came-from map backwards from the goal to rebuild
/// the route. Kept in one place so the strategies don't each duplicate it.
/// </summary>
internal static class PathReconstructor
{
    public static IReadOnlyList<Position> Build(
        IReadOnlyDictionary<Position, Position> cameFrom, Position start, Position goal)
    {
        var path = new List<Position> { goal };
        var current = goal;

        while (current != start)
        {
            current = cameFrom[current];
            path.Add(current);
        }

        path.Reverse();
        return path;
    }
}