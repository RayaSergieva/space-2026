using System.Net;
using System.Net.Mail;

namespace Space2026.Core.Reporting;

/// <summary>
/// Emails the mission report to mission control over SMTP, taking the sender
/// address, password and receiver address as inputs per the brief. Uses the
/// built-in System.Net.Mail so the solution keeps zero external dependencies;
/// for production use, MailKit is the actively maintained successor.
/// </summary>
public sealed class EmailMissionReporter : IMissionReporter, IDisposable
{
    private readonly string _senderEmail;
    private readonly string _receiverEmail;
    private readonly SmtpClient _client;

    public EmailMissionReporter(
        string senderEmail,
        string senderPassword,
        string receiverEmail,
        string smtpHost = "smtp.gmail.com",
        int smtpPort = 587)
    {
        if (string.IsNullOrWhiteSpace(senderEmail))
            throw new ArgumentException("Sender email is required.", nameof(senderEmail));
        if (string.IsNullOrWhiteSpace(receiverEmail))
            throw new ArgumentException("Receiver email is required.", nameof(receiverEmail));

        _senderEmail = senderEmail;
        _receiverEmail = receiverEmail;
        _client = new SmtpClient(smtpHost, smtpPort)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(senderEmail, senderPassword)
        };
    }

    public void Send(string subject, string body, bool isHtml = false)
    {
        using var message = new MailMessage(_senderEmail, _receiverEmail, subject, body)
        {
            IsBodyHtml = isHtml
        };
        _client.Send(message);
    }

    public void Dispose() => _client.Dispose();
}