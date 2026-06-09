using Space2026.Core.Models;

namespace Space2026.Core.Pathfinding;

/// <summary>
/// Strategy contract for "find the best route from start to goal on this grid".
/// Every algorithm implements this one interface, so algorithms can be swapped
/// without touching any other part of the application.
/// </summary>
public interface IPathfindingStrategy
{
    /// <summary>Human-friendly algorithm name, shown when choosing a strategy.</summary>
    string Name { get; }

    PathResult FindShortestPath(Grid grid, Position start, Position goal);
}