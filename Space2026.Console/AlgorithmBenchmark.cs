using System.Diagnostics;
using Space2026.Core.Generation;
using Space2026.Core.Pathfinding;

namespace Space2026.Console;

/// <summary>
/// Menu option 6: times every strategy on the same seeded 100x100 map and
/// prints a comparison table. A diagnostics feature rather than menu
/// plumbing, so it lives in its own class.
/// </summary>
internal static class AlgorithmBenchmark
{
    public static void Run()
    {
        const int iterations = 200;

        System.Console.WriteLine();
        System.Console.WriteLine("Benchmark: seeded 100x100 map, 25% asteroids, 10% debris, 200 runs per algorithm.");
        System.Console.WriteLine("Generating the map and warming up the JIT...");

        var grid = new RandomMapGenerator(seed: 2026).Generate(
            rows: 100, columns: 100, astronautCount: 1,
            asteroidDensity: 0.25, debrisDensity: 0.10, ensureSolvable: true);
        var start = grid.Astronauts[0].Start;

        var strategies = new IPathfindingStrategy[]
        {
            new BreadthFirstSearchStrategy(),
            new DijkstraStrategy(),
            new AStarStrategy(),
            new OptimizedDijkstraStrategy(),
            new OptimizedAStarStrategy()
        };

        // Warm-up so the JIT compiles every code path before timing starts.
        foreach (var strategy in strategies)
            strategy.FindShortestPath(grid, start, grid.Station);

        var measurements = new List<(string Name, double Microseconds, int Cost)>();
        foreach (var strategy in strategies)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            var stopwatch = Stopwatch.StartNew();
            var cost = 0;
            for (var i = 0; i < iterations; i++)
                cost = strategy.FindShortestPath(grid, start, grid.Station).Cost;
            stopwatch.Stop();

            measurements.Add((strategy.Name, stopwatch.Elapsed.TotalMicroseconds / iterations, cost));
        }

        var baseline = measurements[1].Microseconds; // standard Dijkstra

        System.Console.WriteLine();
        System.Console.WriteLine($"{"Algorithm",-36}{"Avg / run",12}{"Speed vs Dijkstra",20}{"Path cost",12}");
        foreach (var (name, microseconds, cost) in measurements)
            System.Console.WriteLine($"{name,-36}{microseconds,9:F1} us{baseline / microseconds,18:F2}x{cost,12}");

        System.Console.WriteLine();
        System.Console.WriteLine("Higher x = faster. BFS may show a higher cost: it minimises moves, not cost.");
        System.Console.WriteLine("Indicative Stopwatch figures; BenchmarkDotNet is the rigorous tool for production work.");
    }
}