using Space2026.Core.Navigation;
using Space2026.Core.Parsing;
using Space2026.Core.Pathfinding;
using Space2026.Core.Reporting;

Console.WriteLine("SPACE 2026 — Astronaut Navigation");
Console.WriteLine("Guiding astronauts home, one shortest path at a time.");
Console.WriteLine();

// The example mission from the assessment brief.
const string missionMap =
    "S1 0 X 0 0 0 S2\n" +
    "X 0 0 0 0 X 0\n" +
    "X X 0 X 0 X 0\n" +
    "0 X X 0 0 X 0\n" +
    "0 X X 0 0 0 F";

var parser = new MapParser();
var grid = parser.Parse(missionMap);
IPathfindingStrategy strategy = new BreadthFirstSearchStrategy();

Console.WriteLine($"Algorithm: {strategy.Name}");
Console.WriteLine();

var navigation = new NavigationService(strategy);
var results = navigation.PlanMissions(grid);

Console.WriteLine(MissionReportBuilder.Build(grid, results));