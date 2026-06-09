using System.Text.RegularExpressions;
using Space2026.Core.Models;

namespace Space2026.Core.Parsing;

/// <summary>
/// Turns raw text into a validated Grid. Every mission rule is enforced here,
/// in one place, so any Grid that exists is guaranteed valid — downstream code
/// never re-checks. All failures throw MapValidationException with a message
/// written for the person who typed the map.
/// </summary>
public sealed class MapParser
{
    public const int MinDimension = 2;
    public const int MaxDimension = 100;
    public const int MaxAstronauts = 3;

    private static readonly Regex AstronautPattern = new("^S[1-3]$", RegexOptions.Compiled);

    /// <summary>Parses a map from a block of text (one row per line).</summary>
    public Grid Parse(string mapText)
    {
        if (string.IsNullOrWhiteSpace(mapText))
            throw new MapValidationException("The map is empty. Please provide a cosmic map.");

        var lines = mapText
            .Replace("\r\n", "\n")
            .Split('\n')
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();

        return Parse(lines);
    }

    /// <summary>Parses a map from individual rows.</summary>
    public Grid Parse(IReadOnlyList<string> lines)
    {
        if (lines is null || lines.Count == 0)
            throw new MapValidationException("The map has no rows.");

        ValidateDimension(lines.Count, "rows");

        var columns = Tokenize(lines[0]).Length;
        ValidateDimension(columns, "columns");

        var terrain = new CellType[lines.Count, columns];
        var symbols = new string[lines.Count, columns];
        var astronauts = new List<Astronaut>();
        var seenNames = new HashSet<string>();
        Position? station = null;

        for (var r = 0; r < lines.Count; r++)
        {
            var tokens = Tokenize(lines[r]);
            if (tokens.Length != columns)
                throw new MapValidationException(
                    $"Row {r + 1} has {tokens.Length} columns but the map width is {columns}. All rows must be the same length.");

            for (var c = 0; c < columns; c++)
            {
                ParseCell(tokens[c], new Position(r, c), terrain, symbols, astronauts, seenNames, ref station);
            }
        }

        if (astronauts.Count == 0)
            throw new MapValidationException("The map contains no astronauts. At least 'S1' is required.");

        if (station is null)
            throw new MapValidationException("The map has no Space Station ('F'). The astronauts have nowhere to go!");

        // Deterministic order regardless of where astronauts sit on the map.
        var ordered = astronauts.OrderBy(a => a.Name, StringComparer.Ordinal).ToList();
        return new Grid(terrain, symbols, ordered, station.Value);
    }

    private static void ParseCell(
        string token,
        Position position,
        CellType[,] terrain,
        string[,] symbols,
        List<Astronaut> astronauts,
        HashSet<string> seenNames,
        ref Position? station)
    {
        var upper = token.ToUpperInvariant();
        var (r, c) = (position.Row, position.Col);

        if (upper is "O" or "0")
        {
            terrain[r, c] = CellType.OpenSpace;
            symbols[r, c] = token; // preserve the author's glyph for faithful rendering
        }
        else if (upper == "X")
        {
            terrain[r, c] = CellType.Asteroid;
            symbols[r, c] = "X";
        }
        else if (upper == "D")
        {
            terrain[r, c] = CellType.Debris;
            symbols[r, c] = "D";
        }
        else if (upper == "F")
        {
            if (station is not null)
                throw new MapValidationException("The map contains more than one Space Station ('F'). Exactly one is allowed.");
            terrain[r, c] = CellType.OpenSpace;
            symbols[r, c] = "F";
            station = position;
        }
        else if (AstronautPattern.IsMatch(upper))
        {
            if (!seenNames.Add(upper))
                throw new MapValidationException($"Astronaut '{upper}' appears more than once on the map.");
            if (seenNames.Count > MaxAstronauts)
                throw new MapValidationException($"The map contains more than {MaxAstronauts} astronauts.");
            terrain[r, c] = CellType.OpenSpace;
            symbols[r, c] = upper;
            astronauts.Add(new Astronaut(upper, position));
        }
        else
        {
            throw new MapValidationException(
                $"Unknown symbol '{token}' at row {r + 1}, column {c + 1}. Valid symbols: S1, S2, S3, F, O (or 0), X, D.");
        }
    }

    private static string[] Tokenize(string line) =>
        line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

    private static void ValidateDimension(int value, string label)
    {
        if (value < MinDimension || value > MaxDimension)
            throw new MapValidationException(
                $"Number of {label} must be between {MinDimension} and {MaxDimension}, but was {value}.");
    }
}