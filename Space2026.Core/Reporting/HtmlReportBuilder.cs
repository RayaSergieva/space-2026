using System.Text;
using Space2026.Core.Models;
using Space2026.Core.Navigation;

namespace Space2026.Core.Reporting;

/// <summary>
/// Builds the HTML mission report for email: a mission-control transmission
/// with an ASCII astronaut, a rescued/lost/algorithm stats strip, one card per
/// journey and a colour legend. All styles are inlined because email clients
/// strip stylesheets; a monospace font keeps the map columns aligned. Cells
/// are padded to a fixed two-character width so wide labels (S1, S2, S3)
/// cannot push their row out of alignment. The plain-text MissionReportBuilder
/// remains the canonical wording for clients that cannot display HTML.
/// </summary>
public static class HtmlReportBuilder
{
    private const string Background = "#0b0e14";
    private const string Panel = "#11151f";
    private const string Border = "#1f2430";
    private const string TextMuted = "#64748b";
    private const string TextFaint = "#475569";
    private const string Cyan = "#7dd3fc";
    private const string Green = "#4ade80";
    private const string Red = "#f87171";
    private const string Gold = "#facc15";
    private const string OpenSpaceGray = "#c4c9d4";
    private const string AsteroidRed = "#ef4444";
    private const string DebrisAmber = "#f59e0b";

    private const string AstronautArt =
        "   _..._\n" +
        "  /_____\\\n" +
        " |  o o  |\n" +
        "  \\_---_/\n" +
        " /|     |\\\n" +
        "| |     | |\n" +
        "  |_____|\n" +
        "  /__|__\\";

    public static string Build(Grid grid, IReadOnlyList<MissionResult> results, string algorithmName)
    {
        var rescued = results.Count(r => r.Succeeded);
        var lost = results.Count - rescued;

        var builder = new StringBuilder();
        builder.Append($"<div style=\"max-width:560px;background:{Background};border-radius:12px;overflow:hidden;");
        builder.Append("font-family:Consolas,Menlo,monospace\">");

        AppendHeader(builder);
        AppendStatsStrip(builder, rescued, lost, algorithmName);

        builder.Append("<div style=\"padding:20px 24px\">");

        foreach (var failure in results.Where(r => !r.Succeeded))
        {
            builder.Append("<div style=\"background:#2a1215;border:1px solid #7f1d1d;border-radius:8px;");
            builder.Append("padding:10px 14px;margin-bottom:16px\">");
            builder.Append($"<p style=\"margin:0;font-size:15px;color:#fca5a5;font-weight:bold\">");
            builder.Append($"&#9888;&nbsp; Mission failed \u2014 Astronaut {failure.Astronaut.Name} lost in space!</p></div>");
        }

        foreach (var result in results.Where(r => r.Succeeded))
            AppendMissionCard(builder, grid, result);

        AppendLegend(builder);
        builder.Append("</div>");

        AppendFooter(builder);
        builder.Append("</div>");
        return builder.ToString();
    }

    private static void AppendHeader(StringBuilder builder)
    {
        builder.Append($"<div style=\"background:{Panel};padding:20px 24px;border-bottom:1px solid {Border}\">");
        builder.Append("<table role=\"presentation\" style=\"border-collapse:collapse\"><tr>");
        builder.Append("<td style=\"vertical-align:middle;padding-right:18px\">");
        builder.Append($"<pre style=\"margin:0;font-size:12px;line-height:1.35;color:#93c5fd\">{AstronautArt}</pre></td>");
        builder.Append("<td style=\"vertical-align:middle\">");
        builder.Append($"<p style=\"margin:0;font-size:12px;letter-spacing:2px;color:{TextMuted};font-weight:bold\">MISSION CONTROL TRANSMISSION</p>");
        builder.Append($"<p style=\"margin:6px 0 0;font-size:24px;font-weight:bold;color:#e2e8f0\">SPACE 2026 <span style=\"color:{TextFaint}\">/</span> Mission report</p>");
        builder.Append($"<p style=\"margin:6px 0 0;font-size:13px;color:{TextMuted}\">{DateTime.Now:dd MMM yyyy} &middot; {DateTime.Now:HH:mm} local</p>");
        builder.Append("</td></tr></table></div>");
    }

    private static void AppendStatsStrip(StringBuilder builder, int rescued, int lost, string algorithmName)
    {
        builder.Append($"<table role=\"presentation\" style=\"width:100%;border-collapse:collapse;border-bottom:1px solid {Border}\"><tr>");
        AppendStat(builder, rescued.ToString(), "RESCUED", Green, withRightBorder: true);
        AppendStat(builder, lost.ToString(), "LOST", Red, withRightBorder: true);
        AppendStat(builder, ShortAlgorithmName(algorithmName), "ALGORITHM", Cyan, withRightBorder: false);
        builder.Append("</tr></table>");
    }

    private static void AppendStat(StringBuilder builder, string value, string label, string colour, bool withRightBorder)
    {
        var borderStyle = withRightBorder ? $"border-right:1px solid {Border};" : "";
        builder.Append($"<td style=\"width:33%;padding:12px 16px;text-align:center;{borderStyle}\">");
        builder.Append($"<p style=\"margin:0;font-size:22px;font-weight:bold;color:{colour}\">{value}</p>");
        builder.Append($"<p style=\"margin:2px 0 0;font-size:11px;letter-spacing:1px;color:{TextMuted};font-weight:bold\">{label}</p></td>");
    }

    private static void AppendMissionCard(StringBuilder builder, Grid grid, MissionResult result)
    {
        builder.Append($"<div style=\"background:{Panel};border:1px solid {Border};border-radius:8px;margin-bottom:16px;overflow:hidden\">");

        builder.Append($"<div style=\"padding:12px 16px;border-bottom:1px solid {Border}\">");
        builder.Append("<table role=\"presentation\" style=\"width:100%;border-collapse:collapse\"><tr>");
        builder.Append($"<td style=\"font-size:16px;color:{Cyan};font-weight:bold\">&#9656; Astronaut {result.Astronaut.Name}</td>");
        builder.Append($"<td style=\"text-align:right\"><span style=\"font-size:14px;color:{Background};font-weight:bold;");
        builder.Append($"background:{Gold};border-radius:10px;padding:2px 10px\">{result.Result.Cost} steps</span></td>");
        builder.Append("</tr></table></div>");

        AppendGrid(builder, grid, result);
        builder.Append("</div>");
    }

    private static void AppendGrid(StringBuilder builder, Grid grid, MissionResult result)
    {
        var path = result.Result;
        var overlay = path.Path.Count > 2
            ? new HashSet<Position>(path.Path.Skip(1).Take(path.Path.Count - 2))
            : new HashSet<Position>();

        builder.Append($"<pre style=\"margin:0;padding:14px 16px;font-size:19px;font-weight:bold;line-height:1.8;color:{OpenSpaceGray}\">");

        for (var r = 0; r < grid.Rows; r++)
        {
            for (var c = 0; c < grid.Columns; c++)
            {
                if (c > 0) builder.Append(' ');

                var position = new Position(r, c);
                if (overlay.Contains(position))
                    builder.Append($"<span style=\"color:{Gold}\">*</span> ");
                else
                    builder.Append($"<span style=\"color:{ColourFor(grid, position)}\">{grid.SymbolAt(position),-2}</span>");
            }
            builder.Append('\n');
        }

        builder.Append("</pre>");
    }

    private static void AppendLegend(StringBuilder builder)
    {
        builder.Append($"<p style=\"margin:16px 0 0;font-size:14px;color:{TextMuted}\">");
        builder.Append($"<span style=\"color:{Cyan}\">S#</span> astronaut &nbsp; ");
        builder.Append($"<span style=\"color:{Green}\">F</span> station &nbsp; ");
        builder.Append($"<span style=\"color:{Gold}\">*</span> route &nbsp; ");
        builder.Append($"<span style=\"color:{AsteroidRed}\">X</span> asteroid &nbsp; ");
        builder.Append($"<span style=\"color:{DebrisAmber}\">D</span> debris</p>");
    }

    private static void AppendFooter(StringBuilder builder)
    {
        builder.Append($"<div style=\"background:{Panel};padding:12px 24px;border-top:1px solid {Border}\">");
        builder.Append("<table role=\"presentation\" style=\"width:100%;border-collapse:collapse\"><tr>");
        builder.Append($"<td style=\"font-size:12px;color:{Gold};font-weight:bold\">Generated by SPACE 2026</td>");
        builder.Append($"<td style=\"text-align:right;font-size:12px;color:{Gold};font-weight:bold\">&#9889; transmission complete</td>");
        builder.Append("</tr></table></div>");
    }

    private static string ShortAlgorithmName(string fullName)
    {
        var bracket = fullName.IndexOf(" (", StringComparison.Ordinal);
        return bracket > 0 ? fullName[..bracket] : fullName;
    }

    private static string ColourFor(Grid grid, Position position)
    {
        var symbol = grid.SymbolAt(position);
        if (symbol == "F") return Green;
        if (symbol.StartsWith('S')) return Cyan;

        return grid.TerrainAt(position) switch
        {
            CellType.Asteroid => AsteroidRed,
            CellType.Debris => DebrisAmber,
            _ => OpenSpaceGray
        };
    }
}