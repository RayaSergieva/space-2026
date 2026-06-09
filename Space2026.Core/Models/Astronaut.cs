namespace Space2026.Core.Models;

/// <summary>
/// A stranded astronaut: a label ("S1", "S2", "S3") and a launch-pod position.
/// Modelled as a class because an astronaut is an entity with identity —
/// unlike Position, two astronauts are never interchangeable by value.
/// </summary>
public sealed class Astronaut
{
    public string Name { get; }
    public Position Start { get; }

    public Astronaut(string name, Position start)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Astronaut name must be provided.", nameof(name));

        Name = name;
        Start = start;
    }

    public override string ToString() => $"{Name} @ {Start}";
}