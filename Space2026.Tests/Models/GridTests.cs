using Space2026.Core.Models;

namespace Space2026.Tests.Models;

public class GridTests
{
    // A tiny 2x3 world we reuse:   S1 X F
    //                              O  D O
    private static Grid CreateGrid()
    {
        var terrain = new CellType[2, 3]
        {
            { CellType.OpenSpace, CellType.Asteroid, CellType.OpenSpace },
            { CellType.OpenSpace, CellType.Debris,   CellType.OpenSpace }
        };

        var symbols = new string[2, 3]
        {
            { "S1", "X", "F" },
            { "O",  "D", "O" }
        };

        var astronauts = new List<Astronaut> { new("S1", new Position(0, 0)) };
        return new Grid(terrain, symbols, astronauts, station: new Position(0, 2));
    }

    [Fact]
    public void Reports_dimensions_from_the_terrain_array()
    {
        var grid = CreateGrid();

        Assert.Equal(2, grid.Rows);
        Assert.Equal(3, grid.Columns);
    }

    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(1, 2, true)]
    [InlineData(-1, 0, false)]
    [InlineData(0, 3, false)]
    [InlineData(2, 0, false)]
    public void InBounds_accepts_inside_and_rejects_outside(int row, int col, bool expected)
    {
        Assert.Equal(expected, CreateGrid().InBounds(new Position(row, col)));
    }

    [Fact]
    public void Asteroids_are_not_passable_but_debris_is()
    {
        var grid = CreateGrid();

        Assert.False(grid.IsPassable(new Position(0, 1))); // X
        Assert.True(grid.IsPassable(new Position(1, 1)));  // D
    }

    [Fact]
    public void Entering_debris_costs_double()
    {
        var grid = CreateGrid();

        Assert.Equal(1, grid.EntryCost(new Position(1, 0))); // open
        Assert.Equal(2, grid.EntryCost(new Position(1, 1))); // debris
    }

    [Fact]
    public void Neighbours_of_a_corner_exclude_off_map_and_asteroid_cells()
    {
        var grid = CreateGrid();

        // From S1 at (0,0): up/left are off-map, right (0,1) is an asteroid.
        // Only down (1,0) remains.
        var neighbours = grid.Neighbours(new Position(0, 0)).ToList();

        Assert.Single(neighbours);
        Assert.Equal(new Position(1, 0), neighbours[0]);
    }
}