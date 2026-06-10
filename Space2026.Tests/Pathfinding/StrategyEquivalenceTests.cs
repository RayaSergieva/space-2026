using Space2026.Core.Parsing;
using Space2026.Core.Pathfinding;

namespace Space2026.Tests.Pathfinding;

public class StrategyEquivalenceTests
{
    private readonly MapParser _parser = new();

    public static TheoryData<IPathfindingStrategy> WeightedStrategies => new()
    {
        new DijkstraStrategy(),
        new AStarStrategy(),
        new OptimizedDijkstraStrategy(),
        new OptimizedAStarStrategy()
    };

    [Theory]
    [MemberData(nameof(WeightedStrategies))]
    public void Finds_the_briefs_optimal_costs(IPathfindingStrategy strategy)
    {
        var grid = _parser.Parse(TestMaps.BriefExample);

        foreach (var (name, expected) in new[] { ("S1", 10), ("S2", 4) })
        {
            var start = grid.Astronauts.Single(a => a.Name == name).Start;
            Assert.Equal(expected, strategy.FindShortestPath(grid, start, grid.Station).Cost);
        }
    }

    [Theory]
    [MemberData(nameof(WeightedStrategies))]
    public void Detours_around_debris_when_cheaper(IPathfindingStrategy strategy)
    {
        var grid = _parser.Parse(TestMaps.DebrisCorridor);
        var start = grid.Astronauts.Single(a => a.Name == "S1").Start;

        Assert.Equal(6, strategy.FindShortestPath(grid, start, grid.Station).Cost);
    }

    [Theory]
    [MemberData(nameof(WeightedStrategies))]
    public void Reports_failure_when_boxed_in(IPathfindingStrategy strategy)
    {
        var grid = _parser.Parse(TestMaps.TrappedAstronaut);
        var start = grid.Astronauts.Single(a => a.Name == "S1").Start;

        Assert.False(strategy.FindShortestPath(grid, start, grid.Station).Found);
    }
}