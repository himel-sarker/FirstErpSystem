using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using FirstErpSystem.Api.Data;
using FirstErpSystem.Api.Models;

namespace FirstErpSystem.Api.Controllers;

/*
================================================================
ProductController — Added By Himel Sarkar 09-06-2026
LEARNING FLOW:
- Product = ERP Inventory এর core
- Endpoints:
  GET    /api/product        → সব active products
  GET    /api/product/{id}   → একটা product
  POST   /api/product        → নতুন product add
  PUT    /api/product/{id}   → product update
  DELETE /api/product/{id}   → soft delete
  POST   /api/product/{id}/stock-in  → stock বাড়ানো
  POST   /api/product/{id}/stock-out → stock কমানো
- [Authorize] = JWT token ছাড়া access নেই
================================================================
*/
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductController : ControllerBase
{
    //Added By Himel Sarkar 09-06-2026
    private readonly AppDbContext _context;

    public ProductController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/product
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        /*
        LEARNING: Select = projection
        সব column না নিয়ে শুধু দরকারি columns নাও
        এটা performance এর জন্য ভালো practice
        */
        var products = await _context.Products
            .Where(p => p.IsActive)
            .Select(p => new
            {
                p.Id, p.Name, p.Code,
                p.Category, p.UnitPrice,
                p.StockQuantity, p.ReorderLevel,
                p.Unit,
                IsLowStock = p.StockQuantity <= p.ReorderLevel
            })
            .OrderBy(p => p.Name)
            .ToListAsync();

        return Ok(products);
    }

    // GET /api/product/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null || !product.IsActive)
            return NotFound(new { message = "Product not found" });
        return Ok(product);
    }

    // POST /api/product
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Product dto)
    {
        /*
        LEARNING: AnyAsync = exists check
        Code unique হতে হবে — duplicate check করি
        */
        if (await _context.Products.AnyAsync(p => p.Code == dto.Code))
            return BadRequest(new { message = "Product code already exists" });

        var product = new Product
        {
            Name          = dto.Name,
            Code          = dto.Code.ToUpper(),
            Description   = dto.Description,
            Category      = dto.Category,
            UnitPrice     = dto.UnitPrice,
            StockQuantity = dto.StockQuantity,
            ReorderLevel  = dto.ReorderLevel,
            Unit          = dto.Unit,
            CreatedAt     = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Product created successfully", id = product.Id });
    }

    // PUT /api/product/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Product dto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound(new { message = "Product not found" });

        product.Name         = dto.Name;
        product.Description  = dto.Description;
        product.Category     = dto.Category;
        product.UnitPrice    = dto.UnitPrice;
        product.ReorderLevel = dto.ReorderLevel;
        product.Unit         = dto.Unit;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Product updated successfully" });
    }

    // DELETE /api/product/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound(new { message = "Product not found" });

        product.IsActive = false;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Product deactivated successfully" });
    }

    // POST /api/product/{id}/stock-in
    [HttpPost("{id}/stock-in")]
    public async Task<IActionResult> StockIn(int id, [FromBody] StockAdjustDto dto)
    {
        /*
        LEARNING: Stock IN flow:
        1. Product খুঁজে বের করো
        2. StockQuantity বাড়াও
        3. StockTransaction record করো (audit trail)
        4. SaveChanges = দুটো update একসাথে save
        */
        var product = await _context.Products.FindAsync(id);
        if (product == null || !product.IsActive)
            return NotFound(new { message = "Product not found" });

        product.StockQuantity += dto.Quantity;

        var transaction = new StockTransaction
        {
            ProductId             = id,
            TransactionType       = "IN",
            Quantity              = dto.Quantity,
            ReferenceNo           = dto.ReferenceNo,
            Remarks               = dto.Remarks,
            TransactionDate       = DateTime.UtcNow,
            CreatedByEmployeeId   = 1
        };

        _context.StockTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message          = "Stock in recorded",
            newStockQuantity = product.StockQuantity
        });
    }

    // POST /api/product/{id}/stock-out
    [HttpPost("{id}/stock-out")]
    public async Task<IActionResult> StockOut(int id, [FromBody] StockAdjustDto dto)
    {
        /*
        LEARNING: Stock OUT flow:
        1. Product খুঁজে বের করো
        2. Sufficient stock আছে কিনা check করো
        3. StockQuantity কমাও
        4. StockTransaction record করো
        */
        var product = await _context.Products.FindAsync(id);
        if (product == null || !product.IsActive)
            return NotFound(new { message = "Product not found" });

        if (product.StockQuantity < dto.Quantity)
            return BadRequest(new
            {
                message       = "Insufficient stock",
                available     = product.StockQuantity,
                requested     = dto.Quantity
            });

        product.StockQuantity -= dto.Quantity;

        var transaction = new StockTransaction
        {
            ProductId           = id,
            TransactionType     = "OUT",
            Quantity            = dto.Quantity,
            ReferenceNo         = dto.ReferenceNo,
            Remarks             = dto.Remarks,
            TransactionDate     = DateTime.UtcNow,
            CreatedByEmployeeId = 1
        };

        _context.StockTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message          = "Stock out recorded",
            newStockQuantity = product.StockQuantity
        });
    }

    // GET /api/product/{id}/transactions
    [HttpGet("{id}/transactions")]
    public async Task<IActionResult> GetTransactions(int id)
    {
        /*
        LEARNING: Include = eager loading
        StockTransaction এর সাথে Product data ও load করে
        N+1 problem avoid করে — একটাই query চলে
        */
        var transactions = await _context.StockTransactions
            .Where(t => t.ProductId == id)
            .OrderByDescending(t => t.TransactionDate)
            .Select(t => new
            {
                t.Id,
                t.TransactionType,
                t.Quantity,
                t.ReferenceNo,
                t.Remarks,
                t.TransactionDate
            })
            .ToListAsync();

        return Ok(transactions);
    }
    //End By Himel Sarkar 09-06-2026
}

/*
================================================================
StockAdjustDto — Added By Himel Sarkar 09-06-2026
LEARNING: DTO = Data Transfer Object
Controller-এর নিচে রাখলাম কারণ শুধু এই controller-এ use হয়
বড় project-এ আলাদা DTOs folder-এ রাখতে হবে
================================================================
*/
public class StockAdjustDto
{
    public int Quantity { get; set; }
    public string ReferenceNo { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;
}
