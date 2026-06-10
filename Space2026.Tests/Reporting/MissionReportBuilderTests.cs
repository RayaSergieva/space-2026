using Space2026.Core.Navigation;
using Space2026.Core.Parsing;
using Space2026.Core.Pathfinding;
using Space2026.Core.Reporting;

namespace Space2026.Tests.Reporting;

public class MissionReportBuilderTests
{
    private readonly MapParser _parser = new();

    [Fact]
    public void Lists_failures_on_top_then_successes_with_maps()
    {
        var grid = _parser.Parse(TestMaps.TrappedAstronaut);
        var results = new NavigationService(new BreadthFirstSearchStrategy()).PlanMissions(grid);

        var report = MissionReportBuilder.Build(grid, results);

        Assert.StartsWith("Mission failed \u2014 Astronaut S1 lost in space!", report);
        Assert.Contains("Astronaut S2 - Shortest path: 2 steps", report);
    }

    [Fact]
    public void Report_for_the_brief_example_contains_both_missions_in_order()
    {
        var grid = _parser.Parse(TestMaps.BriefExample);
        var results = new NavigationService(new BreadthFirstSearchStrategy()).PlanMissions(grid);

        var report = MissionReportBuilder.Build(grid, results);

        var s2Index = report.IndexOf("Astronaut S2 - Shortest path: 4 steps", StringComparison.Ordinal);
        var s1Index = report.IndexOf("Astronaut S1 - Shortest path: 10 steps", StringComparison.Ordinal);

        Assert.True(s2Index >= 0 && s1Index >= 0, "Both mission headers must be present.");
        Assert.True(s2Index < s1Index, "S2 (cheaper) must be reported before S1.");
    }
}