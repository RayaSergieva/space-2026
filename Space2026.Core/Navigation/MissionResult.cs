using Space2026.Core.Models;
using Space2026.Core.Pathfinding;

namespace Space2026.Core.Navigation;

/// <summary>Pairs an astronaut with the outcome of their journey to the station.</summary>
public sealed record MissionResult(Astronaut Astronaut, PathResult Result)
{
    public bool Succeeded => Result.Found;
}