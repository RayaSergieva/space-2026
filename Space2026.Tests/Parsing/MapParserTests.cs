using Space2026.Core.Models;
using Space2026.Core.Parsing;

namespace Space2026.Tests.Parsing;

public class MapParserTests
{
    private readonly MapParser _parser = new();

    [Fact]
    public void Parses_the_brief_example_into_the_expected_grid()
    {
        var grid = _parser.Parse(TestMaps.BriefExample);

        Assert.Equal(5, grid.Rows);
        Assert.Equal(7, grid.Columns);
        Assert.Equal(new Position(4, 6), grid.Station);
        Assert.Equal(2, grid.Astronauts.Count);
        Assert.Equal(new Position(0, 0), grid.Astronauts.Single(a => a.Name == "S1").Start);
        Assert.Equal(new Position(0, 6), grid.Astronauts.Single(a => a.Name == "S2").Start);
    }

    [Fact]
    public void Accepts_letter_O_and_digit_zero_as_open_space_preserving_the_glyph()
    {
        var grid = _parser.Parse("S1 O 0\n0 X F");

        Assert.Equal(CellType.OpenSpace, grid.TerrainAt(new Position(0, 1)));
        Assert.Equal(CellType.OpenSpace, grid.TerrainAt(new Position(0, 2)));
        Assert.Equal("O", grid.SymbolAt(new Position(0, 1)));
        Assert.Equal("0", grid.SymbolAt(new Position(0, 2)));
    }

    [Fact]
    public void Recognises_debris_terrain()
    {
        var grid = _parser.Parse("S1 D F\n0 0 0");

        Assert.Equal(CellType.Debris, grid.TerrainAt(new Position(0, 1)));
    }

    [Theory]
    [InlineData("S1 0 Z\n0 0 F", "Unknown symbol")]
    [InlineData("S1 0 F\n0 0", "columns")]
    [InlineData("S1 0 0\n0 0 0", "no Space Station")]
    [InlineData("0 0 0\n0 0 F", "no astronauts")]
    [InlineData("S1 0 S1\n0 0 F", "more than once")]
    [InlineData("S1 F F\n0 0 0", "more than one Space Station")]
    public void Rejects_invalid_maps_with_a_message_naming_the_problem(string map, string expectedFragment)
    {
        var ex = Assert.Throws<MapValidationException>(() => _parser.Parse(map));

        Assert.Contains(expectedFragment, ex.Message);
    }

    [Fact]
    public void Rejects_maps_below_the_minimum_dimension()
    {
        Assert.Throws<MapValidationException>(() => _parser.Parse("S1 F"));
    }

    [Fact]
    public void Rejects_empty_input()
    {
        Assert.Throws<MapValidationException>(() => _parser.Parse("   "));
    }
}