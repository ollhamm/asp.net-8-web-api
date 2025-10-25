using Microsoft.AspNetCore.Mvc;
using aspnetcoreapi.Services;
using aspnetcoreapi.Models;
using aspnetcoreapi.DTOs;

namespace aspnetcoreapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly RoleService _roleService;
        public RoleController(RoleService roleService) => _roleService = roleService;

        // POST: api/role/create
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] RoleCreateRequest req, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            try
            {
               var created = await _roleService.AddAsync(req, ct);
                return Created($"/api/users/{created.Id}", created);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}