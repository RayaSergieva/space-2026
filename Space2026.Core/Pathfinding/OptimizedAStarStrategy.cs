using System.Runtime.CompilerServices;
using Space2026.Core.Models;

namespace Space2026.Core.Pathfinding;

/// <summary>
/// A* on flat arrays: combines the two orthogonal speed-ups this project
/// demonstrates separately. AStarStrategy explores fewer cells (guided by an
/// admissible Manhattan heuristic) and OptimizedDijkstraStrategy makes every
/// visit cheaper (int-indexed arrays instead of hashed collections). The
/// Manhattan heuristic is consistent on a 4-connected grid whose minimum
/// entry cost is 1, so a cell's cost is final the first time it leaves the
/// queue; stale entries are skipped by lazy deletion, comparing the g-cost an
/// entry carried at enqueue time (its priority minus the heuristic) against
/// the best known g-cost. Identical results to DijkstraStrategy, verified by
/// the cross-strategy equivalence tests.
/// </summary>
public sealed class OptimizedAStarStrategy : IPathfindingStrategy
{
    public string Name => "A* (optimized, flat arrays)";

    public PathResult FindShortestPath(Grid grid, Position start, Position goal)
    {
        var rows = grid.Rows;
        var columns = grid.Columns;
        var cellCount = rows * columns;

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
        var goalRow = goal.Row;
        var goalCol = goal.Col;

        bestCost[startIndex] = 0;
        var frontier = new PriorityQueue<int, int>();
        frontier.Enqueue(startIndex, Heuristic(startIndex, columns, goalRow, goalCol));

        while (frontier.TryDequeue(out var current, out var priority))
        {
            // Lazy deletion: the g-cost this entry carried is its priority
            // minus the (deterministic) heuristic; if a cheaper route has
            // been recorded since it was enqueued, the entry is stale.
            if (priority - Heuristic(current, columns, goalRow, goalCol) > bestCost[current])
                continue;

            if (current == goalIndex)
                return BuildResult(cameFrom, columns, startIndex, goalIndex, bestCost[goalIndex]);

            var row = current / columns;
            var col = current - row * columns;
            var currentCost = bestCost[current];

            if (row > 0) Relax(current, current - columns, currentCost, entryCost, bestCost, cameFrom, frontier, columns, goalRow, goalCol);
            if (row < rows - 1) Relax(current, current + columns, currentCost, entryCost, bestCost, cameFrom, frontier, columns, goalRow, goalCol);
            if (col > 0) Relax(current, current - 1, currentCost, entryCost, bestCost, cameFrom, frontier, columns, goalRow, goalCol);
            if (col < columns - 1) Relax(current, current + 1, currentCost, entryCost, bestCost, cameFrom, frontier, columns, goalRow, goalCol);
        }

        return PathResult.Failure();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Heuristic(int index, int columns, int goalRow, int goalCol)
    {
        var row = index / columns;
        var col = index - row * columns;
        return Math.Abs(row - goalRow) + Math.Abs(col - goalCol);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Relax(
        int current, int next, int currentCost,
        int[] entryCost, int[] bestCost, int[] cameFrom,
        PriorityQueue<int, int> frontier, int columns, int goalRow, int goalCol)
    {
        var cost = entryCost[next];
        if (cost < 0)
            return; // asteroid

        var tentative = currentCost + cost;
        if (tentative < bestCost[next])
        {
            bestCost[next] = tentative;
            cameFrom[next] = current;
            frontier.Enqueue(next, tentative + Heuristic(next, columns, goalRow, goalCol));
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