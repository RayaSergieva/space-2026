using Space2026.Core.Navigation;
using Space2026.Core.Parsing;
using Space2026.Core.Pathfinding;

namespace Space2026.Tests.Navigation;

public class NavigationServiceTests
{
    private readonly MapParser _parser = new();

    [Fact]
    public void Orders_successful_astronauts_by_ascending_cost()
    {
        var grid = _parser.Parse(TestMaps.BriefExample);
        var service = new NavigationService(new BreadthFirstSearchStrategy());

        var results = service.PlanMissions(grid);

        Assert.Equal("S2", results[0].Astronaut.Name); // 4 steps
        Assert.Equal("S1", results[1].Astronaut.Name); // 10 steps
    }

    [Fact]
    public void Reports_failures_before_successes()
    {
        var grid = _parser.Parse(TestMaps.TrappedAstronaut);
        var service = new NavigationService(new BreadthFirstSearchStrategy());

        var results = service.PlanMissions(grid);

        Assert.False(results[0].Succeeded);
        Assert.Equal("S1", results[0].Astronaut.Name);
        Assert.True(results[1].Succeeded);
    }

    [Fact]
    public void Rejects_a_null_strategy()
    {
        Assert.Throws<ArgumentNullException>(() => new NavigationService(null!));
    }
}