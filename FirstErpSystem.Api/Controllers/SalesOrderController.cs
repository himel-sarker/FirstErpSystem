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
SalesOrderController — Added By Himel Sarkar 09-06-2026
LEARNING FLOW:
- Sales Order workflow:
  1. Create Draft order
  2. Confirm order → Status: Confirmed
  3. Generate Invoice → Status: Invoiced
     → Email to customer with invoice details
  4. Payment (SSLCommerz) → Status: Paid
     → Stock OUT automatically for each line
     → SMS to customer
- Endpoints:
  GET  /api/salesorder              → সব orders
  GET  /api/salesorder/{id}         → একটা order
  POST /api/salesorder              → নতুন order
  PUT  /api/salesorder/{id}/confirm → confirm
  PUT  /api/salesorder/{id}/invoice → invoice generate
  PUT  /api/salesorder/{id}/pay     → payment record
================================================================
*/
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalesOrderController : ControllerBase
{
    //Added By Himel Sarkar 09-06-2026
    private readonly AppDbContext  _context;
    private readonly IEmailService _emailService;
    private readonly ISmsService   _smsService;

    public SalesOrderController(
        AppDbContext context,
        IEmailService emailService,
        ISmsService smsService)
    {
        _context      = context;
        _emailService = emailService;
        _smsService   = smsService;
    }

    // GET /api/salesorder
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _context.SalesOrders
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new
            {
                o.Id, o.OrderNo, o.CustomerName,
                o.Status, o.TotalAmount,
                o.PaidAmount, o.OrderDate
            })
            .ToListAsync();

        return Ok(orders);
    }

    // GET /api/salesorder/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _context.SalesOrders
            .Include(o => o.Lines)
                .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound(new { message = "Sales order not found" });

        return Ok(new
        {
            order.Id, order.OrderNo,
            order.CustomerName, order.CustomerEmail,
            order.CustomerPhone, order.Status,
            order.TotalAmount, order.PaidAmount,
            order.PaymentMethod, order.OrderDate,
            order.InvoicedAt, order.PaidAt,
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

    // POST /api/salesorder
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSalesOrderDto dto)
    {
        /*
        LEARNING: Stock check before creating order
        Customer order নেওয়ার আগে stock আছে কিনা দেখতে হবে
        নাহলে order নিয়ে deliver করতে পারব না
        */
        foreach (var line in dto.Lines)
        {
            var product = await _context.Products.FindAsync(line.ProductId);
            if (product == null)
                return BadRequest(new { message = $"Product {line.ProductId} not found" });

            if (product.StockQuantity < line.Quantity)
                return BadRequest(new
                {
                    message   = $"Insufficient stock for {product.Name}",
                    available = product.StockQuantity,
                    requested = line.Quantity
                });
        }

        var orderCount = await _context.SalesOrders.CountAsync();
        var orderNo    = $"SO-{DateTime.UtcNow.Year}-{(orderCount + 1).ToString().PadLeft(3, '0')}";

        var order = new SalesOrder
        {
            OrderNo             = orderNo,
            CustomerName        = dto.CustomerName,
            CustomerEmail       = dto.CustomerEmail,
            CustomerPhone       = dto.CustomerPhone,
            Notes               = dto.Notes,
            Status              = "Draft",
            OrderDate           = DateTime.UtcNow,
            CreatedByEmployeeId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value)
        };

        decimal total = 0;
        foreach (var line in dto.Lines)
        {
            order.Lines.Add(new SalesOrderLine
            {
                ProductId = line.ProductId,
                Quantity  = line.Quantity,
                UnitPrice = line.UnitPrice
            });
            total += line.Quantity * line.UnitPrice;
        }

        order.TotalAmount = total;
        _context.SalesOrders.Add(order);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Sales order created",
            id      = order.Id,
            orderNo = order.OrderNo
        });
    }

    // PUT /api/salesorder/{id}/confirm
    [HttpPut("{id}/confirm")]
    public async Task<IActionResult> Confirm(int id)
    {
        var order = await _context.SalesOrders.FindAsync(id);
        if (order == null)
            return NotFound(new { message = "Order not found" });

        if (order.Status != "Draft")
            return BadRequest(new { message = $"Cannot confirm. Status: {order.Status}" });

        order.Status = "Confirmed";
        await _context.SaveChangesAsync();

        return Ok(new { message = "Sales order confirmed", orderNo = order.OrderNo });
    }

    // PUT /api/salesorder/{id}/invoice
    [HttpPut("{id}/invoice")]
    public async Task<IActionResult> GenerateInvoice(int id)
    {
        /*
        LEARNING: Invoice generation
        1. Status → Invoiced
        2. Email to customer with invoice details
        3. Customer এখন payment করতে পারবে
        */
        var order = await _context.SalesOrders
            .Include(o => o.Lines)
                .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound(new { message = "Order not found" });

        if (order.Status != "Confirmed")
            return BadRequest(new { message = $"Cannot invoice. Status: {order.Status}" });

        order.Status     = "Invoiced";
        order.InvoicedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Build invoice email
        var lineItems = string.Join("", order.Lines.Select(l =>
            $"<tr><td>{l.Product.Name}</td><td>{l.Quantity}</td>" +
            $"<td>৳{l.UnitPrice:N2}</td>" +
            $"<td>৳{l.UnitPrice * l.Quantity:N2}</td></tr>"
        ));

        try
        {
            await _emailService.SendEmailAsync(
                order.CustomerEmail,
                order.CustomerName,
                $"Invoice {order.OrderNo} - First ERP System",
                $@"<h2>Invoice: {order.OrderNo}</h2>
                   <p>Dear {order.CustomerName},</p>
                   <table border='1' cellpadding='8' style='border-collapse:collapse'>
                     <tr>
                       <th>Product</th><th>Qty</th>
                       <th>Unit Price</th><th>Total</th>
                     </tr>
                     {lineItems}
                     <tr>
                       <td colspan='3'><strong>Total</strong></td>
                       <td><strong>৳{order.TotalAmount:N2}</strong></td>
                     </tr>
                   </table>
                   <p>Please complete payment to proceed.</p>
                   <br><p>First ERP System</p>"
            );
        }
        catch { }

        return Ok(new
        {
            message    = "Invoice generated. Email sent to customer.",
            orderNo    = order.OrderNo,
            totalAmount = order.TotalAmount
        });
    }

    // PUT /api/salesorder/{id}/pay
    [HttpPut("{id}/pay")]
    public async Task<IActionResult> RecordPayment(int id, [FromBody] PaymentDto dto)
    {
        /*
        LEARNING: Payment recording flow
        1. Status → Paid
        2. Stock OUT for each line item
        3. StockTransaction record (audit trail)
        4. SMS confirmation to customer
        Note: SSLCommerz integration Phase 4 Part F-এ করব
        এখন manual payment record করছি
        */
        var order = await _context.SalesOrders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound(new { message = "Order not found" });

        if (order.Status != "Invoiced")
            return BadRequest(new { message = $"Cannot pay. Status: {order.Status}" });

        // Stock OUT for each line
        foreach (var line in order.Lines)
        {
            var product = await _context.Products.FindAsync(line.ProductId);
            if (product != null)
            {
                if (product.StockQuantity < line.Quantity)
                    return BadRequest(new
                    {
                        message   = $"Insufficient stock for product {line.ProductId}",
                        available = product.StockQuantity
                    });

                product.StockQuantity -= line.Quantity;

                _context.StockTransactions.Add(new StockTransaction
                {
                    ProductId           = line.ProductId,
                    TransactionType     = "OUT",
                    Quantity            = line.Quantity,
                    ReferenceNo         = order.OrderNo,
                    Remarks             = $"Sold via SO: {order.OrderNo}",
                    TransactionDate     = DateTime.UtcNow,
                    CreatedByEmployeeId = int.Parse(
                        User.FindFirst(ClaimTypes.NameIdentifier)!.Value)
                });
            }
        }

        order.Status               = "Paid";
        order.PaidAmount           = order.TotalAmount;
        order.PaymentMethod        = dto.PaymentMethod;
        order.PaymentTransactionId = dto.TransactionId;
        order.PaidAt               = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // SMS to customer
        try
        {
            await _smsService.SendSmsAsync(
                order.CustomerPhone,
                $"Payment confirmed for {order.OrderNo}. " +
                $"Amount: {order.TotalAmount:N0} BDT. Thank you!"
            );
        }
        catch { }

        return Ok(new
        {
            message   = "Payment recorded. Stock updated.",
            orderNo   = order.OrderNo,
            paidAmount = order.PaidAmount
        });
    }
    //End By Himel Sarkar 09-06-2026
}

/*
================================================================
DTOs — Added By Himel Sarkar 09-06-2026
================================================================
*/
public class CreateSalesOrderDto
{
    public string CustomerName  { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string Notes         { get; set; } = string.Empty;
    public List<SalesOrderLineDto> Lines { get; set; } = new();
}

public class SalesOrderLineDto
{
    public int     ProductId { get; set; }
    public int     Quantity  { get; set; }
    public decimal UnitPrice { get; set; }
}

public class PaymentDto
{
    public string PaymentMethod { get; set; } = "Cash";
    public string TransactionId { get; set; } = string.Empty;
}
