namespace Space2026.Tests;

/// <summary>Shared map fixtures used across the test suite.</summary>
internal static class TestMaps
{
    /// <summary>The exact example from the assessment brief — our canonical map.</summary>
    public const string BriefExample =
        "S1 0 X 0 0 0 S2\n" +
        "X 0 0 0 0 X 0\n" +
        "X X 0 X 0 X 0\n" +
        "0 X X 0 0 X 0\n" +
        "0 X X 0 0 0 F";

    /// <summary>S1 is boxed in by asteroids and can never reach F; S2 can (cost 2).</summary>
    public const string TrappedAstronaut =
        "S1 X 0 S2\n" +
        "X X 0 0\n" +
        "0 0 0 F";

    /// <summary>
    /// A straight line of debris between S1 and F with an open detour below.
    /// Fewest-moves route goes through the debris (total cost 7); the cheapest
    /// route takes the longer open detour (total cost 6).
    /// </summary>
    public const string DebrisCorridor =
        "S1 D D D F\n" +
        "0 0 0 0 0";
}