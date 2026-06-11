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
/// Pure presentation - all mission logic lives in Space2026.Core; input
/// primitives live in ConsoleInput and the benchmark in AlgorithmBenchmark.
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
                    case "6": AlgorithmBenchmark.Run(); break;
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
            catch (Exception ex)
            {
                // Last resort: an unexpected failure should never kill the menu.
                // Known cases are translated into friendly errors at their
                // source; this names whatever slips through and keeps
                // mission control open.
                System.Console.WriteLine();
                System.Console.WriteLine($"Unexpected error ({ex.GetType().Name}): {ex.Message}");
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
        var rows = ConsoleInput.ReadInt(MapParser.MinDimension, MapParser.MaxDimension);

        System.Console.Write("Number of columns (N): ");
        var columns = ConsoleInput.ReadInt(MapParser.MinDimension, MapParser.MaxDimension);

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
        System.Console.WriteLine("Press Enter at any prompt to accept the default.");
        System.Console.Write("Rows (M) [10]: ");
        var rows = ConsoleInput.ReadInt(MapParser.MinDimension, MapParser.MaxDimension, defaultValue: 10);
        System.Console.Write("Columns (N) [10]: ");
        var cols = ConsoleInput.ReadInt(MapParser.MinDimension, MapParser.MaxDimension, defaultValue: 10);
        System.Console.Write("Astronauts (1-3) [2]: ");
        var count = ConsoleInput.ReadInt(1, MapParser.MaxAstronauts, defaultValue: 2);
        System.Console.Write("Asteroid density % (0-60) [25]: ");
        var asteroidPct = ConsoleInput.ReadInt(0, 60, defaultValue: 25);
        System.Console.Write("Debris density % (0-30) [10]: ");
        var debrisPct = ConsoleInput.ReadInt(0, 30, defaultValue: 10);

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

        var choice = ConsoleInput.ReadInt(1, options.Length);
        _strategy = options[choice - 1];
        System.Console.WriteLine($"Algorithm set to {_strategy.Name}.");
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

        try
        {
            return _parser.Parse(File.ReadAllText(path));
        }
        catch (UnauthorizedAccessException)
        {
            // Known failure, translated at the source into the friendly
            // channel - the loop's catch-all stays for the truly unforeseen.
            throw new MapValidationException(
                $"Permission denied reading: {path}. Check the file's access rights and try again.");
        }
        catch (IOException ex)
        {
            throw new MapValidationException(
                $"The file could not be read - it may be locked by another program. Details: {ex.Message}");
        }
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
            var password = ConsoleInput.ReadPassword();

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
}