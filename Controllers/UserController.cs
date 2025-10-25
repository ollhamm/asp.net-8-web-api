using Microsoft.AspNetCore.Mvc;
using aspnetcoreapi.Services;
using aspnetcoreapi.DTOs;

namespace aspnetcoreapi.Controllers;
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _service;
    public UserController(UserService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var u = await _service.GetByIdAsync(id);
        return u is null ? NotFound() : Ok(u);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UserDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UserDto dto)
    {
        var ok = await _service.UpdateAsync(id, dto);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _service.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }
}