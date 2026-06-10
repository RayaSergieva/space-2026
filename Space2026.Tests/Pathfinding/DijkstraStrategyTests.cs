using Space2026.Core.Parsing;
using Space2026.Core.Pathfinding;

namespace Space2026.Tests.Pathfinding;

public class DijkstraStrategyTests
{
    private readonly MapParser _parser = new();
    private readonly DijkstraStrategy _dijkstra = new();

    [Theory]
    [InlineData("S1", 10)]
    [InlineData("S2", 4)]
    public void Matches_bfs_optimal_costs_on_the_unweighted_brief_example(string astronaut, int expectedCost)
    {
        var grid = _parser.Parse(TestMaps.BriefExample);
        var start = grid.Astronauts.Single(a => a.Name == astronaut).Start;

        var result = _dijkstra.FindShortestPath(grid, start, grid.Station);

        Assert.Equal(expectedCost, result.Cost);
    }

    [Fact]
    public void Detours_around_debris_when_the_longer_route_is_cheaper()
    {
        var grid = _parser.Parse(TestMaps.DebrisCorridor);
        var start = grid.Astronauts.Single(a => a.Name == "S1").Start;

        var result = _dijkstra.FindShortestPath(grid, start, grid.Station);

        Assert.Equal(6, result.Cost); // down, along the open row, back up
    }

    [Fact]
    public void Bfs_takes_the_fewest_moves_route_and_pays_more_through_debris()
    {
        // Not a bug in BFS — it answers "fewest moves", not "lowest cost".
        // This contrast is exactly why Dijkstra is the default once debris exists.
        var grid = _parser.Parse(TestMaps.DebrisCorridor);
        var start = grid.Astronauts.Single(a => a.Name == "S1").Start;

        var result = new BreadthFirstSearchStrategy().FindShortestPath(grid, start, grid.Station);

        Assert.Equal(7, result.Cost);
    }

    [Fact]
    public void Reports_failure_when_the_astronaut_is_boxed_in()
    {
        var grid = _parser.Parse(TestMaps.TrappedAstronaut);
        var start = grid.Astronauts.Single(a => a.Name == "S1").Start;

        var result = _dijkstra.FindShortestPath(grid, start, grid.Station);

        Assert.False(result.Found);
        Assert.Empty(result.Path);
    }
}