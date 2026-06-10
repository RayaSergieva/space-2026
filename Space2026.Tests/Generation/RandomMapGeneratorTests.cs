using Space2026.Core.Generation;
using Space2026.Core.Pathfinding;

namespace Space2026.Tests.Generation;

public class RandomMapGeneratorTests
{
    [Fact]
    public void Generates_the_requested_size_and_astronaut_count()
    {
        var grid = new RandomMapGenerator(seed: 42).Generate(rows: 8, columns: 10, astronautCount: 3);

        Assert.Equal(8, grid.Rows);
        Assert.Equal(10, grid.Columns);
        Assert.Equal(3, grid.Astronauts.Count);
    }

    [Fact]
    public void Ensures_S1_can_always_reach_the_station_when_asked()
    {
        var generator = new RandomMapGenerator(seed: 7);
        var dijkstra = new DijkstraStrategy();

        for (var i = 0; i < 25; i++)
        {
            var grid = generator.Generate(rows: 10, columns: 10, asteroidDensity: 0.30, ensureSolvable: true);

            var result = dijkstra.FindShortestPath(grid, grid.Astronauts[0].Start, grid.Station);
            Assert.True(result.Found, $"Map {i} should be solvable for S1.");
        }
    }

    [Fact]
    public void Same_seed_produces_the_same_map()
    {
        var a = new RandomMapGenerator(seed: 123).Generate(6, 6, astronautCount: 2, asteroidDensity: 0.2);
        var b = new RandomMapGenerator(seed: 123).Generate(6, 6, astronautCount: 2, asteroidDensity: 0.2);

        Assert.Equal(a.Station, b.Station);
        Assert.Equal(
            a.Astronauts.Select(x => (x.Name, x.Start)),
            b.Astronauts.Select(x => (x.Name, x.Start)));
    }

    [Fact]
    public void Rejects_densities_that_leave_no_room_for_a_path()
    {
        Assert.Throws<ArgumentException>(() =>
            new RandomMapGenerator(seed: 1).Generate(5, 5, asteroidDensity: 0.95));
    }

    [Fact]
    public void Rejects_out_of_range_dimensions()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new RandomMapGenerator(seed: 1).Generate(rows: 1, columns: 5));
    }
}