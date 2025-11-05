using ERecruitment.Web.Data;
using ERecruitment.Web.Models;

namespace ERecruitment.Web.Notifications;

public class LoggingEmailSenderDecorator : IEmailSender
{
    private readonly IEmailSender _inner;
    private readonly ApplicationDbContext _db;

    public LoggingEmailSenderDecorator(IEmailSender inner, ApplicationDbContext db)
    {
        _inner = inner;
        _db = db;
    }

    public async Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        try
        {
            await _inner.SendAsync(toEmail, subject, body, cancellationToken);
            _db.EmailDeliveries.Add(new EmailDelivery
            {
                ToEmail = toEmail,
                Subject = subject,
                BodyPreview = body.Length > 200 ? body[..200] : body,
                FullBody = body,
                Status = EmailDeliveryStatus.Sent
            });
        }
        catch (Exception ex)
        {
            _db.EmailDeliveries.Add(new EmailDelivery
            {
                ToEmail = toEmail,
                Subject = subject,
                BodyPreview = body.Length > 200 ? body[..200] : body,
                FullBody = body,
                Status = EmailDeliveryStatus.Failed,
                Error = ex.Message
            });
        }
        finally
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}


