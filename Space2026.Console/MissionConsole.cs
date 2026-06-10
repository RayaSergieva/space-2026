using Space2026.Core.Models;
using Space2026.Core.Navigation;
using Space2026.Core.Parsing;
using Space2026.Core.Pathfinding;
using Space2026.Core.Reporting;

namespace Space2026.Console;

/// <summary>
/// The interactive console application: shows the menu, gathers maps from the
/// user, runs missions through the Core services and prints the report.
/// Pure presentation — all mission logic lives in Space2026.Core.
/// </summary>
internal sealed class MissionConsole
{
    private readonly MapParser _parser = new();

    // Dijkstra will replace this default in Phase 6; the menu will let the
    // user swap strategies in Phase 7. Held as the interface on purpose.
    private IPathfindingStrategy _strategy = new BreadthFirstSearchStrategy();

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
                    case "4":
                        System.Console.WriteLine();
                        System.Console.WriteLine("Safe travels, commander.");
                        return;
                    default:
                        System.Console.WriteLine("Unknown option — please choose 1-4.");
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
        System.Console.WriteLine("  4) Exit");
        System.Console.Write("> ");
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

    private Grid EnterMapManually()
    {
        System.Console.Write("Number of rows (M): ");
        var rows = ReadInt(MapParser.MinDimension, MapParser.MaxDimension);

        System.Console.WriteLine($"Enter {rows} rows of space-separated symbols (S1-S3, F, O or 0, X, D):");

        var lines = new List<string>(rows);
        for (var i = 0; i < rows; i++)
        {
            System.Console.Write($"  row {i + 1}: ");
            lines.Add(System.Console.ReadLine() ?? "");
        }

        return _parser.Parse(lines);
    }

    private void RunMission(Grid grid)
    {
        System.Console.WriteLine();
        System.Console.WriteLine($"Algorithm: {_strategy.Name}");
        System.Console.WriteLine();

        var navigation = new NavigationService(_strategy);
        var results = navigation.PlanMissions(grid);

        ConsoleRenderer.PrintMissions(grid, results);
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