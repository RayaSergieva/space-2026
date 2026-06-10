using System.Runtime.CompilerServices;
using Space2026.Core.Models;

namespace Space2026.Core.Pathfinding;

/// <summary>
/// Dijkstra on flat arrays: every cell is encoded as a single int index
/// (row * columns + col), so the hash-based collections of the standard
/// implementation become plain array reads - no hashing, no iterator
/// allocations, cache-friendly memory access. The "settled" set becomes a
/// lazy-deletion check (a dequeued entry costlier than the best known route
/// is stale and skipped). Identical results to DijkstraStrategy, verified by
/// the cross-strategy equivalence tests; only the constant factor changes.
/// With edge weights limited to {1, 2}, a Dial's bucket queue could replace
/// the heap for O(E) total - left as the documented next rung, since at this
/// grid size the heap is no longer the bottleneck.
/// </summary>
public sealed class OptimizedDijkstraStrategy : IPathfindingStrategy
{
    public string Name => "Dijkstra (optimized, flat arrays)";

    public PathResult FindShortestPath(Grid grid, Position start, Position goal)
    {
        var rows = grid.Rows;
        var columns = grid.Columns;
        var cellCount = rows * columns;

        // One linear pass turns terrain into entry costs (-1 = asteroid).
        // After this, the search loop never touches the Grid object.
        var entryCost = new int[cellCount];
        for (var r = 0; r < rows; r++)
            for (var c = 0; c < columns; c++)
            {
                var position = new Position(r, c);
                entryCost[r * columns + c] = grid.IsPassable(position) ? grid.EntryCost(position) : -1;
            }

        var bestCost = new int[cellCount];
        Array.Fill(bestCost, int.MaxValue);
        var cameFrom = new int[cellCount];
        Array.Fill(cameFrom, -1);

        var startIndex = start.Row * columns + start.Col;
        var goalIndex = goal.Row * columns + goal.Col;

        bestCost[startIndex] = 0;
        var frontier = new PriorityQueue<int, int>();
        frontier.Enqueue(startIndex, 0);

        while (frontier.TryDequeue(out var current, out var currentCost))
        {
            // Lazy deletion: a cheaper route was recorded after this entry
            // was enqueued, so this copy is stale.
            if (currentCost > bestCost[current])
                continue;

            if (current == goalIndex)
                return BuildResult(cameFrom, columns, startIndex, goalIndex, currentCost);

            var row = current / columns;
            var col = current - row * columns;

            // Inline 4-neighbour expansion; bounds checks on row/col prevent
            // index wrap-around at the grid edges.
            if (row > 0) Relax(current, current - columns, currentCost, entryCost, bestCost, cameFrom, frontier);
            if (row < rows - 1) Relax(current, current + columns, currentCost, entryCost, bestCost, cameFrom, frontier);
            if (col > 0) Relax(current, current - 1, currentCost, entryCost, bestCost, cameFrom, frontier);
            if (col < columns - 1) Relax(current, current + 1, currentCost, entryCost, bestCost, cameFrom, frontier);
        }

        return PathResult.Failure();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Relax(
        int current, int next, int currentCost,
        int[] entryCost, int[] bestCost, int[] cameFrom,
        PriorityQueue<int, int> frontier)
    {
        var cost = entryCost[next];
        if (cost < 0)
            return; // asteroid

        var tentative = currentCost + cost;
        if (tentative < bestCost[next])
        {
            bestCost[next] = tentative;
            cameFrom[next] = current;
            frontier.Enqueue(next, tentative);
        }
    }

    private static PathResult BuildResult(int[] cameFrom, int columns, int startIndex, int goalIndex, int cost)
    {
        var indices = new List<int> { goalIndex };
        var current = goalIndex;

        while (current != startIndex)
        {
            current = cameFrom[current];
            indices.Add(current);
        }

        indices.Reverse();

        var path = new Position[indices.Count];
        for (var i = 0; i < indices.Count; i++)
        {
            var index = indices[i];
            var row = index / columns;
            path[i] = new Position(row, index - row * columns);
        }

        return PathResult.Success(cost, path);
    }
}