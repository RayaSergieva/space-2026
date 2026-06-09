using Space2026.Core.Models;

namespace Space2026.Tests.Models;

public class PositionTests
{
    [Fact]
    public void Positions_with_same_coordinates_are_equal()
    {
        var a = new Position(2, 3);
        var b = new Position(2, 3);

        Assert.Equal(a, b);
        Assert.True(a == b);
    }

    [Fact]
    public void Equal_positions_produce_the_same_hash_code()
    {
        // HashSet/Dictionary correctness depends on this, not just on Equals.
        var a = new Position(7, 1);
        var b = new Position(7, 1);

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Translate_offsets_row_and_column_correctly()
    {
        var start = new Position(5, 5);

        Assert.Equal(new Position(4, 5), start.Translate(-1, 0)); // up
        Assert.Equal(new Position(5, 6), start.Translate(0, 1));  // right
    }
}