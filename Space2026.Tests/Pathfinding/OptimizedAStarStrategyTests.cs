using Space2026.Core.Generation;
using Space2026.Core.Parsing;
using Space2026.Core.Pathfinding;

namespace Space2026.Tests.Pathfinding;

public class OptimizedAStarStrategyTests
{
    [Fact]
    public void Agrees_with_standard_dijkstra_on_twenty_seeded_random_maps()
    {
        var generator = new RandomMapGenerator(seed: 77);
        var standard = new DijkstraStrategy();
        var optimized = new OptimizedAStarStrategy();

        for (var i = 0; i < 20; i++)
        {
            // ensureSolvable: false on purpose - failure cases must agree too.
            var grid = generator.Generate(20, 20, astronautCount: 1,
                asteroidDensity: 0.25, debrisDensity: 0.15, ensureSolvable: false);
            var start = grid.Astronauts[0].Start;

            var expected = standard.FindShortestPath(grid, start, grid.Station);
            var actual = optimized.FindShortestPath(grid, start, grid.Station);

            Assert.Equal(expected.Found, actual.Found);
            if (expected.Found)
                Assert.Equal(expected.Cost, actual.Cost);
        }
    }

    [Fact]
    public void Path_endpoints_and_contiguity_hold_on_the_brief_example()
    {
        var grid = new MapParser().Parse(TestMaps.BriefExample);
        var start = grid.Astronauts.Single(a => a.Name == "S1").Start;

        var result = new OptimizedAStarStrategy().FindShortestPath(grid, start, grid.Station);

        Assert.Equal(start, result.Path.First());
        Assert.Equal(grid.Station, result.Path.Last());
        for (var i = 1; i < result.Path.Count; i++)
            Assert.Contains(result.Path[i], grid.Neighbours(result.Path[i - 1]));
    }
}