using Space2026.Core.Parsing;
using Space2026.Core.Pathfinding;
using Space2026.Core.Rendering;

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

// Route every astronaut, then report: failures first, then ascending cost.
var results = grid.Astronauts
    .Select(astronaut => (Astronaut: astronaut,
                          Result: strategy.FindShortestPath(grid, astronaut.Start, grid.Station)))
    .OrderBy(r => r.Result.Found)        // false sorts before true → failures on top
    .ThenBy(r => r.Result.Cost)
    .ToList();

foreach (var (astronaut, result) in results)
{
    if (!result.Found)
    {
        Console.WriteLine($"Mission failed \u2014 Astronaut {astronaut.Name} lost in space!");
        Console.WriteLine();
        continue;
    }

    Console.WriteLine($"Astronaut {astronaut.Name} - Shortest path: {result.Cost} steps");
    Console.WriteLine(MapRenderer.Render(grid, result));
    Console.WriteLine();
}