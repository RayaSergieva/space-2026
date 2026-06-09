using Space2026.Core.Models;

namespace Space2026.Tests.Models;

public class AstronautTests
{
    [Fact]
    public void Stores_name_and_start_position()
    {
        var astronaut = new Astronaut("S1", new Position(0, 0));

        Assert.Equal("S1", astronaut.Name);
        Assert.Equal(new Position(0, 0), astronaut.Start);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Rejects_missing_or_blank_name(string? name)
    {
        Assert.Throws<ArgumentException>(() => new Astronaut(name!, new Position(0, 0)));
    }
}