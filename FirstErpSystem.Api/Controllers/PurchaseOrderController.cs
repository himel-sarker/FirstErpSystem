using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FirstErpSystem.Api.Data;
using FirstErpSystem.Api.Models;
using FirstErpSystem.Api.Services;

namespace FirstErpSystem.Api.Controllers;

/*
================================================================
PurchaseOrderController — Added By Himel Sarkar 09-06-2026
LEARNING FLOW:
- Purchase Order workflow:
  1. Staff/Manager creates Draft order
  2. Admin/Manager approves → Status: Approved
     → Email goes to Supplier
     → SMS goes to Creator
  3. Admin marks as Received → Status: Received
     → Stock IN automatically for each line item
- Endpoints:
  GET  /api/purchaseorder          → সব orders
  GET  /api/purchaseorder/{id}     → একটা order details
  POST /api/purchaseorder          → নতুন order তৈরি
  PUT  /api/purchaseorder/{id}/approve  → approve
  PUT  /api/purchaseorder/{id}/receive  → receive + stock update
================================================================
*/
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PurchaseOrderController : ControllerBase
{
    //Added By Himel Sarkar 09-06-2026
    private readonly AppDbContext  _context;
    private readonly IEmailService _emailService;
    private readonly ISmsService   _smsService;

    public PurchaseOrderController(
        AppDbContext context,
        IEmailService emailService,
        ISmsService smsService)
    {
        _context      = context;
        _emailService = emailService;
        _smsService   = smsService;
    }

    // GET /api/purchaseorder
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _context.PurchaseOrders
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new
            {
                o.Id, o.OrderNo, o.SupplierName,
                o.Status, o.TotalAmount, o.OrderDate
            })
            .ToListAsync();

        return Ok(orders);
    }

    // GET /api/purchaseorder/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        /*
        LEARNING: Include + ThenInclude = eager loading
        Order → Lines → Product (3 level deep)
        একটাই SQL query-তে সব data আসে
        */
        var order = await _context.PurchaseOrders
            .Include(o => o.Lines)
                .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound(new { message = "Purchase order not found" });

        return Ok(new
        {
            order.Id, order.OrderNo,
            order.SupplierName, order.SupplierEmail,
            order.SupplierPhone, order.Status,
            order.TotalAmount, order.OrderDate,
            order.ApprovedAt, order.ReceivedAt,
            order.Notes,
            Lines = order.Lines.Select(l => new
            {
                l.Id, l.ProductId,
                ProductName = l.Product.Name,
                ProductCode = l.Product.Code,
                l.Quantity, l.UnitPrice,
                LineTotal   = l.UnitPrice * l.Quantity
            })
        });
    }

    // POST /api/purchaseorder
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderDto dto)
    {
        /*
        LEARNING: Order Number generation
        PO-2026-001 format
        Count + 1 = sequential number
        PadLeft(3, '0') = 1 → 001, 10 → 010
        */
        var orderCount = await _context.PurchaseOrders.CountAsync();
        var orderNo    = $"PO-{DateTime.UtcNow.Year}-{(orderCount + 1).ToString().PadLeft(3, '0')}";

        var order = new PurchaseOrder
        {
            OrderNo              = orderNo,
            SupplierName         = dto.SupplierName,
            SupplierEmail        = dto.SupplierEmail,
            SupplierPhone        = dto.SupplierPhone,
            Notes                = dto.Notes,
            Status               = "Draft",
            OrderDate            = DateTime.UtcNow,
            CreatedByEmployeeId  = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value)
        };

        // Add order lines
        decimal total = 0;
        foreach (var line in dto.Lines)
        {
            var product = await _context.Products.FindAsync(line.ProductId);
            if (product == null)
                return BadRequest(new { message = $"Product {line.ProductId} not found" });

            order.Lines.Add(new PurchaseOrderLine
            {
                ProductId = line.ProductId,
                Quantity  = line.Quantity,
                UnitPrice = line.UnitPrice
            });
            total += line.Quantity * line.UnitPrice;
        }

        order.TotalAmount = total;
        _context.PurchaseOrders.Add(order);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Purchase order created",
            id      = order.Id,
            orderNo = order.OrderNo
        });
    }

    // PUT /api/purchaseorder/{id}/approve
    [HttpPut("{id}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        /*
        LEARNING: Approve workflow
        1. Only Admin/Manager can approve
        2. Only Draft orders can be approved
        3. After approve → Email to supplier + SMS to creator
        */
        var role = User.FindFirst(ClaimTypes.Role)!.Value;
        if (role != "Admin" && role != "Manager")
            return Forbid();

        var order = await _context.PurchaseOrders.FindAsync(id);
        if (order == null)
            return NotFound(new { message = "Order not found" });

        if (order.Status != "Draft")
            return BadRequest(new { message = $"Cannot approve. Current status: {order.Status}" });

        order.Status     = "Approved";
        order.ApprovedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Send Email to supplier
        try
        {
            await _emailService.SendEmailAsync(
                order.SupplierEmail,
                order.SupplierName,
                $"Purchase Order {order.OrderNo} Approved",
                $@"<h2>Purchase Order Approved</h2>
                   <p>Dear {order.SupplierName},</p>
                   <p>Your Purchase Order <strong>{order.OrderNo}</strong>
                   has been approved.</p>
                   <p>Total Amount: ৳{order.TotalAmount:N2}</p>
                   <p>Please proceed with delivery.</p>
                   <br><p>First ERP System</p>"
            );
        }
        catch
        {
            // Email fail হলেও order approve থাকবে
            // Production-এ এখানে logging করতে হবে
        }

        // Send SMS to creator
        try
        {
            var creator = await _context.Employees
                .FindAsync(order.CreatedByEmployeeId);
            if (creator != null)
            {
                await _smsService.SendSmsAsync(
                    creator.Email, // production-এ phone number হবে
                    $"PO {order.OrderNo} approved. Amount: {order.TotalAmount:N0} BDT"
                );
            }
        }
        catch
        {
            // SMS fail হলেও approve থাকবে
        }

        return Ok(new { message = "Purchase order approved", orderNo = order.OrderNo });
    }

    // PUT /api/purchaseorder/{id}/receive
    [HttpPut("{id}/receive")]
    public async Task<IActionResult> Receive(int id)
    {
        /*
        LEARNING: Receive workflow
        1. Only Approved orders can be received
        2. For each line item → Stock IN automatically
        3. StockTransaction record করা হয় (audit trail)
        4. Product.StockQuantity update হয়
        */
        var role = User.FindFirst(ClaimTypes.Role)!.Value;
        if (role != "Admin")
            return Forbid();

        var order = await _context.PurchaseOrders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound(new { message = "Order not found" });

        if (order.Status != "Approved")
            return BadRequest(new { message = $"Cannot receive. Current status: {order.Status}" });

        // Update stock for each line
        foreach (var line in order.Lines)
        {
            var product = await _context.Products.FindAsync(line.ProductId);
            if (product != null)
            {
                // Increase stock
                product.StockQuantity += line.Quantity;

                // Record transaction
                _context.StockTransactions.Add(new StockTransaction
                {
                    ProductId           = line.ProductId,
                    TransactionType     = "IN",
                    Quantity            = line.Quantity,
                    ReferenceNo         = order.OrderNo,
                    Remarks             = $"Received from PO: {order.OrderNo}",
                    TransactionDate     = DateTime.UtcNow,
                    CreatedByEmployeeId = int.Parse(
                        User.FindFirst(ClaimTypes.NameIdentifier)!.Value)
                });
            }
        }

        order.Status     = "Received";
        order.ReceivedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Purchase order received. Stock updated.", orderNo = order.OrderNo });
    }
    //End By Himel Sarkar 09-06-2026
}

/*
================================================================
DTOs — Added By Himel Sarkar 09-06-2026
================================================================
*/
public class CreatePurchaseOrderDto
{
    public string SupplierName  { get; set; } = string.Empty;
    public string SupplierEmail { get; set; } = string.Empty;
    public string SupplierPhone { get; set; } = string.Empty;
    public string Notes         { get; set; } = string.Empty;
    public List<PurchaseOrderLineDto> Lines { get; set; } = new();
}

public class PurchaseOrderLineDto
{
    public int     ProductId { get; set; }
    public int     Quantity  { get; set; }
    public decimal UnitPrice { get; set; }
}
