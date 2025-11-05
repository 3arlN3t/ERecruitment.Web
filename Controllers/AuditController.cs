using ERecruitment.Web.Services;
using ERecruitment.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERecruitment.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AuditController : Controller
{
    private readonly IRecruitmentRepository _repo;

    public AuditController(IRecruitmentRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 50, string? search = null)
    {
        var paged = await _repo.GetAuditLogPagedAsync(page, pageSize, search);
        var vm = new AuditListViewModel
        {
            Items = paged.Items,
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount,
            Search = search
        };
        return View(vm);
    }
}


