using System.Net;
using System.Net.Mail;

namespace ERecruitment.Web.Notifications;

public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpClient _client;
    private readonly string _from;

    public SmtpEmailSender(IConfiguration config)
    {
        var host = config["Smtp:Host"];
        var port = int.TryParse(config["Smtp:Port"], out var p) ? p : 25;
        var user = config["Smtp:User"];
        var pass = config["Smtp:Pass"];
        _from = config["Smtp:From"] ?? "no-reply@localhost";

        _client = new SmtpClient(host ?? "localhost", port)
        {
            EnableSsl = bool.TryParse(config["Smtp:Ssl"], out var ssl) && ssl
        };
        if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(pass))
        {
            _client.Credentials = new NetworkCredential(user, pass);
        }
    }

    public Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        var mail = new MailMessage(_from, toEmail, subject, body)
        {
            IsBodyHtml = true
        };
        return _client.SendMailAsync(mail, cancellationToken);
    }
}


