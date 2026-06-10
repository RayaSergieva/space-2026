namespace Space2026.Core.Reporting;

/// <summary>
/// Abstraction over "deliver this report somewhere". The email reporter sends
/// it to mission control over SMTP; future channels (a file, a web hook, a
/// chat message) just implement this interface — the same open-for-extension
/// idea as IPathfindingStrategy, applied to report delivery.
/// </summary>
public interface IMissionReporter
{
    void Send(string subject, string body, bool isHtml = false);
}