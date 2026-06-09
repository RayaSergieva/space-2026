using Space2026.Core.Models;

namespace Space2026.Core.Pathfinding;

/// <summary>
/// The outcome of a single pathfinding run. When Found is false the astronaut
/// is lost in space; Path is then empty and Cost is zero.
/// </summary>
public sealed record PathResult(bool Found, int Cost, IReadOnlyList<Position> Path)
{
    public static PathResult Failure() => new(false, 0, Array.Empty<Position>());

    public static PathResult Success(int cost, IReadOnlyList<Position> path) => new(true, cost, path);
}