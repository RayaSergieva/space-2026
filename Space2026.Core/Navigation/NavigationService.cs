using Space2026.Core.Models;
using Space2026.Core.Pathfinding;

namespace Space2026.Core.Navigation;

/// <summary>
/// Coordinates the mission: routes every astronaut to the station using the
/// injected pathfinding strategy, then orders results per the brief —
/// failures first, then successes by ascending cost. The strategy arrives via
/// the constructor (dependency injection), so the algorithm can be swapped
/// without this class changing at all.
/// </summary>
public sealed class NavigationService
{
    private readonly IPathfindingStrategy _strategy;

    public NavigationService(IPathfindingStrategy strategy) =>
        _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));

    public string StrategyName => _strategy.Name;

    public IReadOnlyList<MissionResult> PlanMissions(Grid grid)
    {
        ArgumentNullException.ThrowIfNull(grid);

        return grid.Astronauts
            .Select(astronaut => new MissionResult(
                astronaut,
                _strategy.FindShortestPath(grid, astronaut.Start, grid.Station)))
            .OrderBy(result => result.Succeeded)            // failures (false) on top
            .ThenBy(result => result.Result.Cost)           // then cheapest first
            .ThenBy(result => result.Astronaut.Name, StringComparer.Ordinal) // stable tie-break
            .ToList();
    }
}