namespace FirstErpSystem.Api.Models;

/*
================================================================
SalesOrder Model — Added By Himel Sarkar 09-06-2026
LEARNING FLOW:
- SalesOrder = customer-কে product বিক্রির order
- Status workflow: Draft → Confirmed → Invoiced → Paid
  Draft     = তৈরি হয়েছে
  Confirmed = confirm করা হয়েছে
  Invoiced  = invoice generate হয়েছে
  Paid      = SSLCommerz দিয়ে payment হয়েছে
- Payment হলে stock automatically OUT হবে
- Email + SMS notification যাবে customer-কে
================================================================
*/
public class SalesOrder
{
    public int Id { get; set; }
    public string OrderNo { get; set; } = string.Empty; // e.g. SO-2026-001
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft"; // Draft, Confirmed, Invoiced, Paid
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty; // SSLCommerz, Cash
    public string PaymentTransactionId { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? InvoicedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public int CreatedByEmployeeId { get; set; }
    public string Notes { get; set; } = string.Empty;

    // Navigation property
    public List<SalesOrderLine> Lines { get; set; } = new();
}
