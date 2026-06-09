namespace Space2026.Core.Models;

/// <summary>
/// The terrain of a single cell, which determines whether it can be entered
/// and at what cost. Astronauts and the Space Station are not terrain — they
/// are locations tracked separately by the grid, standing on open space.
/// </summary>
public enum CellType
{
    /// <summary>Safe to travel through. Entry cost: 1.</summary>
    OpenSpace,

    /// <summary>Dangerous — cannot be entered.</summary>
    Asteroid,

    /// <summary>Passable space debris. Entry cost: 2 (bonus objective).</summary>
    Debris
}