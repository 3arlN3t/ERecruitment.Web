namespace ERecruitment.Web.Models;

public class LanguageProficiency
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string LanguageName { get; set; } = string.Empty;
    public string SpeakProficiency { get; set; } = string.Empty;
    public string ReadWriteProficiency { get; set; } = string.Empty;
}
