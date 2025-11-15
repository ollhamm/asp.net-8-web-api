using Microsoft.AspNetCore.Mvc;
using aspnetcoreapi.Services;
using aspnetcoreapi.Models;
using aspnetcoreapi.DTOs;
using aspnetcoreapi.Common;
using Microsoft.AspNetCore.Authorization;

namespace aspnetcoreapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
   
    
    public class RoleController : ControllerBase
    {
        private readonly RoleService _roleService;
        public RoleController(RoleService roleService) => _roleService = roleService;

        // POST: api/role/create
        [Authorize(Roles = "Admin")]
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

        // GET: api/role
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var roles = await _roleService.GetAllAsync(ct);
            var response = new ApiResponse
            {
                Title = "Roles Retrieved",
                Status = 200,
                Message = "Roles retrieved successfully.",
                Data = roles
            };
            return StatusCode(200, response);
        }
        

        // GET: api/role/{id}
        [Authorize(Roles = "Admin")]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id,  CancellationToken ct)
        {
            var roleId = await _roleService.GetByIdAsync(id, ct);
            var response = new ApiResponse
            {
                Title = "Role Retrieved",
                Status = 200,
                Message = "Role retrieved successfully.",
                Data = roleId
            };
            return StatusCode(200, response);
        }
    }
}