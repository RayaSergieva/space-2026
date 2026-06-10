using Space2026.Core.Models;

namespace Space2026.Core.Pathfinding;

/// <summary>
/// A* search — Dijkstra guided by a heuristic. Each frontier cell is
/// prioritised by (cost so far + estimated cost to goal), steering the search
/// toward the station instead of flooding outward. The Manhattan-distance
/// heuristic never overestimates (every step costs at least 1), so it is
/// admissible and A* returns the same optimal cost as Dijkstra — usually
/// after exploring far fewer cells.
/// </summary>
public sealed class AStarStrategy : IPathfindingStrategy
{
    public string Name => "A* (weighted, heuristic-guided)";

    public PathResult FindShortestPath(Grid grid, Position start, Position goal)
    {
        var gScore = new Dictionary<Position, int> { [start] = 0 };
        var cameFrom = new Dictionary<Position, Position>();
        var settled = new HashSet<Position>();
        var frontier = new PriorityQueue<Position, int>();

        frontier.Enqueue(start, Heuristic(start, goal));

        while (frontier.TryDequeue(out var current, out _))
        {
            // Same stale-entry guard as Dijkstra: the first dequeue of a cell
            // is its best; any later occurrence in the queue is skipped.
            if (!settled.Add(current))
                continue;

            if (current == goal)
                return PathResult.Success(gScore[goal], PathReconstructor.Build(cameFrom, start, goal));

            foreach (var next in grid.Neighbours(current))
            {
                if (settled.Contains(next))
                    continue;

                var tentative = gScore[current] + grid.EntryCost(next);
                if (tentative < gScore.GetValueOrDefault(next, int.MaxValue))
                {
                    gScore[next] = tentative;
                    cameFrom[next] = current;
                    frontier.Enqueue(next, tentative + Heuristic(next, goal));
                }
            }
        }

        return PathResult.Failure();
    }

    private static int Heuristic(Position from, Position to) =>
        Math.Abs(from.Row - to.Row) + Math.Abs(from.Col - to.Col);
}