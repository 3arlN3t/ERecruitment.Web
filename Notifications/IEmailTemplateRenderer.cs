namespace ERecruitment.Web.Notifications;

public interface IEmailTemplateRenderer
{
    Task<string> RenderAsync(string templateName, object model);
}


