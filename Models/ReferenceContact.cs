namespace ERecruitment.Web.Models;

public class ReferenceContact
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
}
