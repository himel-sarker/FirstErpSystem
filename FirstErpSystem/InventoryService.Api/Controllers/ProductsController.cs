using InventoryService.Application.Products.Commands;
using InventoryService.Application.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Api.Controllers;

/*
================================================================
ProductsController — Added By Himel Sarkar 11-06-2026
LEARNING FLOW:
- In a traditional Monolith, the Controller does ALL the work 
  (DB logic, business rules, etc.).
- In Clean Architecture with CQRS, the Controller is "Thin".
  It just receives the HTTP request, passes it to MediatR (the Waiter),
  and returns the result. It has ZERO database logic here!
================================================================
*/
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // GET api/products
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // We send a Query to MediatR. We don't care HOW it gets the data.
        var products = await _mediator.Send(new GetProductsQuery());
        return Ok(products);
    }

    // POST api/products
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
    {
        // We send a Command to MediatR. We don't care HOW it saves to DB.
        var id = await _mediator.Send(command);
        return Ok(new { id = id, message = "Product created via Microservice!" });
    }
}
