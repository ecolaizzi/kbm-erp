using KBM.Api.Authorization;
using KBM.Application.Auth;
using KBM.Application.Companies;
using KBM.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KBM.Api.Controllers;

[ApiController]
[Route("api/companies")]
[Authorize]
public class CompaniesController(ICompanyService companies) : ControllerBase
{
    [HttpGet]
    [RequiresPermission(PermissionCodes.CompaniesRead)]
    public Task<IReadOnlyList<CompanyListItem>> List(CancellationToken ct) =>
        companies.ListAsync(ct);

    [HttpGet("{id:long}")]
    [RequiresPermission(PermissionCodes.CompaniesRead)]
    public async Task<ActionResult<CompanyDetail>> Get(long id, CancellationToken ct)
    {
        var company = await companies.GetAsync(id, ct);
        return company is null ? NotFound() : Ok(company);
    }

    [HttpPost]
    [RequiresPermission(PermissionCodes.CompaniesCreate)]
    public async Task<ActionResult<CompanyDetail>> Create([FromBody] CreateCompanyRequest request, CancellationToken ct)
    {
        try
        {
            var company = await companies.CreateAsync(request, ct);
            return CreatedAtAction(nameof(Get), new { id = company.Id }, company);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiError("CONFLICT", ex.Message));
        }
    }

    [HttpPut("{id:long}")]
    [RequiresPermission(PermissionCodes.CompaniesEdit)]
    public async Task<ActionResult<CompanyDetail>> Update(long id, [FromBody] UpdateCompanyRequest request, CancellationToken ct)
    {
        var company = await companies.UpdateAsync(id, request, ct);
        return company is null ? NotFound() : Ok(company);
    }
}
