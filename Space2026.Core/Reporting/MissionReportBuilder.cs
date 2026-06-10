using System.Text;
using Space2026.Core.Models;
using Space2026.Core.Navigation;
using Space2026.Core.Rendering;

namespace Space2026.Core.Reporting;

/// <summary>
/// Builds the canonical plain-text mission report: failure lines first, then
/// each successful journey with its cost and annotated map. Centralised so the
/// console, an email, or any future channel produce identical wording.
/// </summary>
public static class MissionReportBuilder
{
    public static string Build(Grid grid, IReadOnlyList<MissionResult> results)
    {
        var builder = new StringBuilder();

        foreach (var failure in results.Where(r => !r.Succeeded))
            builder.AppendLine($"Mission failed \u2014 Astronaut {failure.Astronaut.Name} lost in space!");

        if (results.Any(r => !r.Succeeded) && results.Any(r => r.Succeeded))
            builder.AppendLine();

        var successes = results.Where(r => r.Succeeded).ToList();
        for (var i = 0; i < successes.Count; i++)
        {
            var result = successes[i];
            builder.AppendLine($"Astronaut {result.Astronaut.Name} - Shortest path: {result.Result.Cost} steps");
            builder.AppendLine(MapRenderer.Render(grid, result.Result));
            if (i < successes.Count - 1) builder.AppendLine();
        }

        return builder.ToString().TrimEnd('\r', '\n');
    }
}