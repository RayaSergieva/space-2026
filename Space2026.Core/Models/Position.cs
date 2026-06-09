namespace Space2026.Core.Models;

/// <summary>
/// An immutable coordinate on the cosmic map (row, column).
/// A readonly record struct gives us fast value-based equality and zero
/// heap allocations, so positions can be used correctly and cheaply as
/// keys in hash-based collections like HashSet and Dictionary.
/// </summary>
public readonly record struct Position(int Row, int Col)
{
    /// <summary>Returns a new position offset by the given row/column deltas.</summary>
    public Position Translate(int deltaRow, int deltaCol) => new(Row + deltaRow, Col + deltaCol);

    public override string ToString() => $"({Row}, {Col})";
}