using Space2026.Core.Parsing;
using Space2026.Core.Pathfinding;

namespace Space2026.Tests.Pathfinding;

public class BreadthFirstSearchStrategyTests
{
    private readonly MapParser _parser = new();
    private readonly BreadthFirstSearchStrategy _bfs = new();

    private const string BriefExample =
        "S1 0 X 0 0 0 S2\n" +
        "X 0 0 0 0 X 0\n" +
        "X X 0 X 0 X 0\n" +
        "0 X X 0 0 X 0\n" +
        "0 X X 0 0 0 F";

    [Theory]
    [InlineData("S1", 10)]
    [InlineData("S2", 4)]
    public void Finds_the_optimal_cost_on_the_brief_example(string astronaut, int expectedCost)
    {
        var grid = _parser.Parse(BriefExample);
        var start = grid.Astronauts.Single(a => a.Name == astronaut).Start;

        var result = _bfs.FindShortestPath(grid, start, grid.Station);

        Assert.True(result.Found);
        Assert.Equal(expectedCost, result.Cost);
    }

    [Fact]
    public void Path_starts_at_the_astronaut_and_ends_at_the_station()
    {
        var grid = _parser.Parse(BriefExample);
        var start = grid.Astronauts.Single(a => a.Name == "S2").Start;

        var result = _bfs.FindShortestPath(grid, start, grid.Station);

        Assert.Equal(start, result.Path.First());
        Assert.Equal(grid.Station, result.Path.Last());
    }

    [Fact]
    public void Every_step_in_the_path_is_a_legal_neighbour_of_the_previous()
    {
        var grid = _parser.Parse(BriefExample);
        var start = grid.Astronauts.Single(a => a.Name == "S1").Start;

        var result = _bfs.FindShortestPath(grid, start, grid.Station);

        for (var i = 1; i < result.Path.Count; i++)
            Assert.Contains(result.Path[i], grid.Neighbours(result.Path[i - 1]));
    }

    [Fact]
    public void Reports_failure_when_the_astronaut_is_boxed_in()
    {
        // S1 is walled off by asteroids; S2 can reach F.
        var grid = _parser.Parse("S1 X 0 S2\nX X 0 0\n0 0 0 F");
        var start = grid.Astronauts.Single(a => a.Name == "S1").Start;

        var result = _bfs.FindShortestPath(grid, start, grid.Station);

        Assert.False(result.Found);
        Assert.Empty(result.Path);
        Assert.Equal(0, result.Cost);
    }
}