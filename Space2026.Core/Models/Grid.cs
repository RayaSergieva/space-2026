namespace Space2026.Core.Models;

/// <summary>
/// An immutable cosmic navigation map. Terrain (what pathfinding reasons about)
/// and the original symbols (what rendering echoes back) are stored separately,
/// so each consumer has exactly one source of truth. The Space Station and the
/// astronauts are locations on the grid, not terrain types.
/// </summary>
public sealed class Grid
{
    private static readonly (int dRow, int dCol)[] Directions =
    {
        (-1, 0), // up
        (1, 0),  // down
        (0, -1), // left
        (0, 1)   // right
    };

    private const int OpenCost = 1;
    private const int DebrisCost = 2;

    private readonly CellType[,] _terrain;
    private readonly string[,] _symbols;

    public int Rows { get; }
    public int Columns { get; }
    public Position Station { get; }
    public IReadOnlyList<Astronaut> Astronauts { get; }

    public Grid(CellType[,] terrain, string[,] symbols, IReadOnlyList<Astronaut> astronauts, Position station)
    {
        _terrain = terrain ?? throw new ArgumentNullException(nameof(terrain));
        _symbols = symbols ?? throw new ArgumentNullException(nameof(symbols));
        Astronauts = astronauts ?? throw new ArgumentNullException(nameof(astronauts));

        Rows = terrain.GetLength(0);
        Columns = terrain.GetLength(1);
        Station = station;
    }

    public bool InBounds(Position p) =>
        p.Row >= 0 && p.Row < Rows && p.Col >= 0 && p.Col < Columns;

    public CellType TerrainAt(Position p) => _terrain[p.Row, p.Col];

    public string SymbolAt(Position p) => _symbols[p.Row, p.Col];

    /// <summary>True when the cell exists and is not an asteroid.</summary>
    public bool IsPassable(Position p) => InBounds(p) && _terrain[p.Row, p.Col] != CellType.Asteroid;

    /// <summary>The cost of entering the given cell. Debris is dearer than open space.</summary>
    public int EntryCost(Position p) =>
        _terrain[p.Row, p.Col] == CellType.Debris ? DebrisCost : OpenCost;

    /// <summary>Yields the in-bounds, passable orthogonal neighbours of a cell.</summary>
    public IEnumerable<Position> Neighbours(Position p)
    {
        foreach (var (dRow, dCol) in Directions)
        {
            var next = p.Translate(dRow, dCol);
            if (IsPassable(next))
                yield return next;
        }
    }
}