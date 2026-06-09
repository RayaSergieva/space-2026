namespace Space2026.Core.Parsing;

/// <summary>
/// Raised when a supplied map violates the mission rules (bad dimensions,
/// unknown symbols, missing station, wrong astronaut count, ...).
/// A dedicated type lets the UI distinguish "the user's map is wrong"
/// (show a friendly message) from genuine program bugs (fail loudly).
/// </summary>
public sealed class MapValidationException : Exception
{
    public MapValidationException(string message) : base(message) { }
}