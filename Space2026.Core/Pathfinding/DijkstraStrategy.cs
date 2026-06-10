using Space2026.Core.Models;

namespace Space2026.Core.Pathfinding;

/// <summary>
/// Dijkstra's algorithm: always expands the cheapest-so-far cell via a
/// priority queue, so the first arrival at the goal is provably the minimum
/// total cost — correctly accounting for debris costing more to cross than
/// open space. The optimal default once terrain costs are unequal.
/// </summary>
public sealed class DijkstraStrategy : IPathfindingStrategy
{
    public string Name => "Dijkstra (weighted)";

    public PathResult FindShortestPath(Grid grid, Position start, Position goal)
    {
        var bestCost = new Dictionary<Position, int> { [start] = 0 };
        var cameFrom = new Dictionary<Position, Position>();
        var settled = new HashSet<Position>();
        var frontier = new PriorityQueue<Position, int>();

        frontier.Enqueue(start, 0);

        while (frontier.TryDequeue(out var current, out var currentCost))
        {
            // A cell can sit in the queue more than once (no decrease-key in
            // .NET's PriorityQueue); the first dequeue is the cheapest, so any
            // later occurrence is stale and must be skipped.
            if (!settled.Add(current))
                continue;

            if (current == goal)
                return PathResult.Success(currentCost, BuildPath(cameFrom, start, goal));

            foreach (var next in grid.Neighbours(current))
            {
                if (settled.Contains(next))
                    continue;

                var tentative = currentCost + grid.EntryCost(next);
                if (tentative < bestCost.GetValueOrDefault(next, int.MaxValue))
                {
                    bestCost[next] = tentative;
                    cameFrom[next] = current;
                    frontier.Enqueue(next, tentative);
                }
            }
        }

        return PathResult.Failure();
    }

    private static IReadOnlyList<Position> BuildPath(
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