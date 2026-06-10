using Space2026.Core.Models;
using Space2026.Core.Parsing;
using Space2026.Core.Pathfinding;

namespace Space2026.Core.Generation;

/// <summary>
/// Generates random cosmic maps. Obstacles are scattered by density, then the
/// station and astronauts are placed on distinct cells. With ensureSolvable
/// the generator re-rolls until astronaut S1 can reach the station, so demos
/// never open on an impossible board. An optional seed makes generation fully
/// deterministic — that is what makes this class testable.
/// Generated maps are emitted as text and run through MapParser, so machine-
/// made maps pass exactly the same validation gate as human ones.
/// </summary>
public sealed class RandomMapGenerator
{
    private const int MaxAttempts = 200;

    private readonly Random _random;
    private readonly MapParser _parser = new();

    public RandomMapGenerator(int? seed = null) =>
        _random = seed.HasValue ? new Random(seed.Value) : new Random();

    public Grid Generate(
        int rows,
        int columns,
        int astronautCount = 1,
        double asteroidDensity = 0.25,
        double debrisDensity = 0.0,
        bool ensureSolvable = true)
    {
        ValidateArguments(rows, columns, astronautCount, asteroidDensity, debrisDensity);

        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            var grid = _parser.Parse(BuildCandidate(rows, columns, astronautCount, asteroidDensity, debrisDensity));

            if (!ensureSolvable || IsSolvable(grid))
                return grid;
        }

        throw new InvalidOperationException(
            "Could not generate a solvable map with the requested density. Try fewer asteroids or a larger map.");
    }

    private string BuildCandidate(
        int rows, int columns, int astronautCount, double asteroidDensity, double debrisDensity)
    {
        var symbols = new string[rows, columns];

        for (var r = 0; r < rows; r++)
            for (var c = 0; c < columns; c++)
            {
                var roll = _random.NextDouble();
                if (roll < asteroidDensity) symbols[r, c] = "X";
                else if (roll < asteroidDensity + debrisDensity) symbols[r, c] = "D";
                else symbols[r, c] = "0";
            }

        // Reserve distinct cells for the station and each astronaut, overwriting
        // whatever obstacle the density roll put there.
        var slots = PickDistinctCells(rows, columns, astronautCount + 1);
        symbols[slots[0].Row, slots[0].Col] = "F";
        for (var i = 0; i < astronautCount; i++)
            symbols[slots[i + 1].Row, slots[i + 1].Col] = $"S{i + 1}";

        return ToText(symbols, rows, columns);
    }

    private List<Position> PickDistinctCells(int rows, int columns, int count)
    {
        var all = new List<Position>(rows * columns);
        for (var r = 0; r < rows; r++)
            for (var c = 0; c < columns; c++)
                all.Add(new Position(r, c));

        // Fisher–Yates shuffle, then take the first N — uniform and simple.
        for (var i = all.Count - 1; i > 0; i--)
        {
            var j = _random.Next(i + 1);
            (all[i], all[j]) = (all[j], all[i]);
        }

        return all.Take(count).ToList();
    }

    private static bool IsSolvable(Grid grid)
    {
        var bfs = new BreadthFirstSearchStrategy();
        return bfs.FindShortestPath(grid, grid.Astronauts[0].Start, grid.Station).Found;
    }

    private static string ToText(string[,] symbols, int rows, int columns)
    {
        var lines = new string[rows];
        for (var r = 0; r < rows; r++)
        {
            var row = new string[columns];
            for (var c = 0; c < columns; c++) row[c] = symbols[r, c];
            lines[r] = string.Join(' ', row);
        }
        return string.Join('\n', lines);
    }

    private static void ValidateArguments(
        int rows, int columns, int astronautCount, double asteroidDensity, double debrisDensity)
    {
        if (rows < MapParser.MinDimension || rows > MapParser.MaxDimension)
            throw new ArgumentOutOfRangeException(nameof(rows),
                $"Rows must be between {MapParser.MinDimension} and {MapParser.MaxDimension}.");
        if (columns < MapParser.MinDimension || columns > MapParser.MaxDimension)
            throw new ArgumentOutOfRangeException(nameof(columns),
                $"Columns must be between {MapParser.MinDimension} and {MapParser.MaxDimension}.");
        if (astronautCount is < 1 or > MapParser.MaxAstronauts)
            throw new ArgumentOutOfRangeException(nameof(astronautCount),
                $"Astronaut count must be between 1 and {MapParser.MaxAstronauts}.");
        if (asteroidDensity < 0 || debrisDensity < 0 || asteroidDensity + debrisDensity > 0.9)
            throw new ArgumentException(
                "Obstacle densities must be non-negative and leave room for a path (sum <= 0.9).");
        if (rows * columns < astronautCount + 1)
            throw new ArgumentException("The map is too small to place the station and all astronauts.");
    }
}