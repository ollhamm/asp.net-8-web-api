// ...existing code...
using Microsoft.AspNetCore.Mvc;
using aspnetcoreapi.Services;
using aspnetcoreapi.DTOs;
using Microsoft.AspNetCore.Authorization;
using aspnetcoreapi.Common;

namespace aspnetcoreapi.Controllers;
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _service;
    public ProductsController(ProductService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
       
        var products = await _service.GetAllAsync();
        var response = new ApiResponse
        {
            Title = "Products Retrieved",
            Status = 200,
            Message = "Products retrieved successfully.",
            Data = products
        };
        return StatusCode(200, response);
        
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var product = await _service.GetByIdAsync(id);
        var response = new ApiResponse
        {
            Title = "Product Retrieved",
            Status = 200,
            Message = "Product retrieved successfully.",
            Data = product
        };
        return StatusCode(200, response);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] ProductCreateRequest dto)
    {
        var created = await _service.AddAsync(dto);
        var response = new ApiResponse
        {
            Title = "Product Created",
            Status = 201,
            Message = $"Product created successfully",
            Data = created
        };
        return StatusCode(201, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProductDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        var response = new ApiResponse
        {
            Title = "Product Updated",
            Status = 200,
            Message = "Product updated successfully.",
            Data = updated
        };
        return StatusCode(200, response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _service.DeleteAsync(id);
        var response = new ApiResponse
        {
            Title = "Product Deleted",
            Status = 200,
            Message = "Product deleted successfully.",
            Data = deleted
        };
        return StatusCode(200, response);
    }
}