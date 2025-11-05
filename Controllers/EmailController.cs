using ERecruitment.Web.Services;
using ERecruitment.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERecruitment.Web.Controllers;

[Authorize(Roles = "Admin")]
public class EmailController : Controller
{
    private readonly IRecruitmentRepository _repo;

    public EmailController(IRecruitmentRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? to = null, string? subject = null, Models.EmailDeliveryStatus? status = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        // Use paginated method - filtering is done in-memory for now
        var paged = await _repo.GetEmailDeliveriesPagedAsync(1, int.MaxValue);
        var all = paged.Items.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(to))
        {
            all = all.Where(e => e.ToEmail.Contains(to, StringComparison.OrdinalIgnoreCase));
        }
        if (!string.IsNullOrWhiteSpace(subject))
        {
            all = all.Where(e => e.Subject.Contains(subject, StringComparison.OrdinalIgnoreCase));
        }
        if (status is not null)
        {
            all = all.Where(e => e.Status == status.Value);
        }

        var allList = all.ToList();
        var items = allList.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var model = new EmailListViewModel
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = allList.Count,
            ToFilter = to,
            SubjectFilter = subject,
            StatusFilter = status
        };
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var item = await _repo.GetEmailDeliveryAsync(id);
        if (item is null)
        {
            return NotFound();
        }
        return View(item);
    }
}


