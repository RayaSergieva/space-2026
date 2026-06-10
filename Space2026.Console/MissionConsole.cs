using System.Diagnostics;
using Space2026.Core.Generation;
using Space2026.Core.Models;
using Space2026.Core.Navigation;
using Space2026.Core.Parsing;
using Space2026.Core.Pathfinding;
using Space2026.Core.Reporting;

namespace Space2026.Console;

/// <summary>
/// The interactive console application: shows the menu, gathers maps from the
/// user, runs missions through the Core services and prints the report.
/// Pure presentation - all mission logic lives in Space2026.Core.
/// </summary>
internal sealed class MissionConsole
{
    private readonly MapParser _parser = new();

    // Dijkstra is the default - optimal on weighted terrain (debris costs 2).
    // Held as the interface so the menu picker can swap algorithms freely.
    private IPathfindingStrategy _strategy = new DijkstraStrategy();

    public void Run()
    {
        System.Console.WriteLine("SPACE 2026 — Astronaut Navigation");
        System.Console.WriteLine("Guiding astronauts home, one shortest path at a time.");

        while (true)
        {
            ShowMenu();
            var choice = (System.Console.ReadLine() ?? "").Trim();

            try
            {
                switch (choice)
                {
                    case "1": RunMission(LoadSampleMap()); break;
                    case "2": RunMission(LoadMapFromFile()); break;
                    case "3": RunMission(EnterMapManually()); break;
                    case "4": RunMission(GenerateRandomMap()); break;
                    case "5": ChooseAlgorithm(); break;
                    case "6": RunBenchmark(); break;
                    case "7":
                        System.Console.WriteLine();
                        System.Console.WriteLine("Safe travels, commander.");
                        return;
                    default:
                        System.Console.WriteLine("Unknown option — please choose 1-7.");
                        break;
                }
            }
            catch (MapValidationException ex)
            {
                // User-fixable input problems: one friendly line, back to the menu.
                System.Console.WriteLine();
                System.Console.WriteLine($"Invalid map: {ex.Message}");
            }
        }
    }

    private void ShowMenu()
    {
        System.Console.WriteLine();
        System.Console.WriteLine("Choose an option:");
        System.Console.WriteLine("  1) Run the sample mission (from the brief)");
        System.Console.WriteLine("  2) Load a map from a file");
        System.Console.WriteLine("  3) Enter a map manually");
        System.Console.WriteLine("  4) Generate a random map");
        System.Console.WriteLine($"  5) Choose algorithm  (current: {_strategy.Name})");
        System.Console.WriteLine("  6) Benchmark the algorithms");
        System.Console.WriteLine("  7) Exit");
        System.Console.Write("> ");
    }

    private Grid EnterMapManually()
    {
        System.Console.Write("Number of rows (M): ");
        var rows = ReadInt(MapParser.MinDimension, MapParser.MaxDimension);

        System.Console.Write("Number of columns (N): ");
        var columns = ReadInt(MapParser.MinDimension, MapParser.MaxDimension);

        System.Console.WriteLine($"Enter {rows} rows of {columns} space-separated symbols (S1-S3, F, O or 0, X, D):");

        var lines = new List<string>(rows);
        for (var i = 0; i < rows; i++)
        {
            System.Console.Write($"  row {i + 1}: ");
            lines.Add(System.Console.ReadLine() ?? "");
        }

        var grid = _parser.Parse(lines);

        // The parser derives the width from the rows; cross-check it against
        // the N the user declared, per the brief's input contract.
        if (grid.Columns != columns)
            throw new MapValidationException(
                $"You declared {columns} columns but the map has {grid.Columns}. Please re-enter the map.");

        return grid;
    }

    private Grid GenerateRandomMap()
    {
        System.Console.Write("Rows (M): ");
        var rows = ReadInt(MapParser.MinDimension, MapParser.MaxDimension);
        System.Console.Write("Columns (N): ");
        var cols = ReadInt(MapParser.MinDimension, MapParser.MaxDimension);
        System.Console.Write("Astronauts (1-3): ");
        var count = ReadInt(1, MapParser.MaxAstronauts);
        System.Console.Write("Asteroid density % (0-60): ");
        var asteroidPct = ReadInt(0, 60);
        System.Console.Write("Debris density % (0-30): ");
        var debrisPct = ReadInt(0, 30);

        try
        {
            return new RandomMapGenerator().Generate(
                rows, cols, count, asteroidPct / 100.0, debrisPct / 100.0, ensureSolvable: true);
        }
        catch (InvalidOperationException ex)
        {
            // Core reports "couldn't generate a solvable map" as a program
            // condition; at the console edge it's a user-fixable input issue,
            // so translate it into the friendly error channel.
            throw new MapValidationException(ex.Message);
        }
    }

    private void ChooseAlgorithm()
    {
        var options = new IPathfindingStrategy[]
        {
            new DijkstraStrategy(),
            new OptimizedDijkstraStrategy(),
            new AStarStrategy(),
            new OptimizedAStarStrategy(),
            new BreadthFirstSearchStrategy()
        };

        System.Console.WriteLine();
        for (var i = 0; i < options.Length; i++)
            System.Console.WriteLine($"  {i + 1}) {options[i].Name}");
        System.Console.Write("Select algorithm: ");

        var choice = ReadInt(1, options.Length);
        _strategy = options[choice - 1];
        System.Console.WriteLine($"Algorithm set to {_strategy.Name}.");
    }

    private static void RunBenchmark()
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

    private Grid LoadSampleMap()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Maps", "sample.txt");
        if (!File.Exists(path))
            throw new MapValidationException(
                "The bundled sample map is missing. Expected it at: " + path);

        return _parser.Parse(File.ReadAllText(path));
    }

    private Grid LoadMapFromFile()
    {
        System.Console.Write("Path to map file: ");
        var path = (System.Console.ReadLine() ?? "").Trim().Trim('"');

        if (string.IsNullOrWhiteSpace(path))
            throw new MapValidationException("No file path was provided.");

        if (!File.Exists(path))
            throw new MapValidationException($"File not found: {path}");

        return _parser.Parse(File.ReadAllText(path));
    }

    private void RunMission(Grid grid)
    {
        System.Console.WriteLine();
        System.Console.WriteLine($"Algorithm: {_strategy.Name}");
        System.Console.WriteLine();

        var navigation = new NavigationService(_strategy);
        var results = navigation.PlanMissions(grid);

        ConsoleRenderer.PrintMissions(grid, results);
        OfferEmail(grid, results);
    }

    private void OfferEmail(Grid grid, IReadOnlyList<MissionResult> results)
    {
        System.Console.Write("Email this report to mission control? (y/N): ");
        var answer = (System.Console.ReadLine() ?? "").Trim();
        if (!answer.StartsWith("y", StringComparison.OrdinalIgnoreCase))
            return;

        try
        {
            System.Console.Write("  Sender email: ");
            var sender = (System.Console.ReadLine() ?? "").Trim();

            System.Console.Write("  Sender password (Gmail requires an app password, not your real one): ");
            var password = ReadPassword();

            System.Console.Write("  Receiver email: ");
            var receiver = (System.Console.ReadLine() ?? "").Trim();

            using var reporter = new EmailMissionReporter(sender, password, receiver);
            reporter.Send("SPACE 2026 - Mission Report",
                HtmlReportBuilder.Build(grid, results, _strategy.Name), isHtml: true);

            System.Console.WriteLine("  Report transmitted to mission control.");
        }
        catch (Exception ex)
        {
            // Network/auth failures shouldn't crash the app - report and return.
            System.Console.WriteLine($"  Could not send email: {ex.Message}");
        }
    }

    private static string ReadPassword()
    {
        var password = new System.Text.StringBuilder();
        while (true)
        {
            var key = System.Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter) { System.Console.WriteLine(); break; }
            if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0) password.Remove(password.Length - 1, 1);
                continue;
            }
            if (!char.IsControl(key.KeyChar)) password.Append(key.KeyChar);
        }
        return password.ToString();
    }

    private static int ReadInt(int min, int max)
    {
        while (true)
        {
            if (int.TryParse(System.Console.ReadLine(), out var value) && value >= min && value <= max)
                return value;

            System.Console.Write($"Please enter a whole number between {min} and {max}: ");
        }
    }
}