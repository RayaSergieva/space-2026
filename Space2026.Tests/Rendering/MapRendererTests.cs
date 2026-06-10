using Space2026.Core.Parsing;
using Space2026.Core.Pathfinding;
using Space2026.Core.Rendering;

namespace Space2026.Tests.Rendering;

public class MapRendererTests
{
    private readonly MapParser _parser = new();
    private readonly BreadthFirstSearchStrategy _bfs = new();

    private const string BriefExample =
        "S1 0 X 0 0 0 S2\n" +
        "X 0 0 0 0 X 0\n" +
        "X X 0 X 0 X 0\n" +
        "0 X X 0 0 X 0\n" +
        "0 X X 0 0 0 F";

    [Fact]
    public void Renders_a_map_without_a_path_exactly_as_supplied()
    {
        var grid = _parser.Parse(BriefExample);

        Assert.Equal(BriefExample, MapRenderer.Render(grid));
    }

    [Fact]
    public void Renders_S2_journey_exactly_as_the_brief_shows()
    {
        var grid = _parser.Parse(BriefExample);
        var s2 = grid.Astronauts.Single(a => a.Name == "S2").Start;
        var path = _bfs.FindShortestPath(grid, s2, grid.Station);

        const string expected =
            "S1 0 X 0 0 0 S2\n" +
            "X 0 0 0 0 X *\n" +
            "X X 0 X 0 X *\n" +
            "0 X X 0 0 X *\n" +
            "0 X X 0 0 0 F";

        Assert.Equal(expected, MapRenderer.Render(grid, path));
    }

    [Fact]
    public void Keeps_endpoint_labels_when_path_has_no_interior()
    {
        var grid = _parser.Parse("S1 F\n0 0");
        var start = grid.Astronauts[0].Start;
        var path = _bfs.FindShortestPath(grid, start, grid.Station);

        Assert.Equal("S1 F\n0 0", MapRenderer.Render(grid, path));
    }
}