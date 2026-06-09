using Space2026.Core.Models;

namespace Space2026.Core.Pathfinding;

/// <summary>
/// Classic breadth-first search. BFS explores the grid in expanding waves, so
/// the first time it reaches the goal it has found a route with the fewest
/// moves — optimal while every step costs the same, as in the base mission.
/// </summary>
public sealed class BreadthFirstSearchStrategy : IPathfindingStrategy
{
    public string Name => "Breadth-First Search";

    public PathResult FindShortestPath(Grid grid, Position start, Position goal)
    {
        var frontier = new Queue<Position>();
        var cameFrom = new Dictionary<Position, Position>();
        var visited = new HashSet<Position> { start };

        frontier.Enqueue(start);

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();

            if (current == goal)
                return BuildResult(grid, cameFrom, start, goal);

            foreach (var next in grid.Neighbours(current))
            {
                if (visited.Add(next))
                {
                    cameFrom[next] = current;
                    frontier.Enqueue(next);
                }
            }
        }

        return PathResult.Failure();
    }

    private static PathResult BuildResult(
        Grid grid, IReadOnlyDictionary<Position, Position> cameFrom, Position start, Position goal)
    {
        var path = new List<Position> { goal };
        var current = goal;

        while (current != start)
        {
            current = cameFrom[current];
            path.Add(current);
        }

        path.Reverse();

        // Cost = sum of entry costs of every cell stepped into (start excluded).
        var cost = 0;
        for (var i = 1; i < path.Count; i++)
            cost += grid.EntryCost(path[i]);

        return PathResult.Success(cost, path);
    }
}