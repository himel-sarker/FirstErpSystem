namespace FirstErpSystem.Api.Models;

/*
================================================================
PurchaseOrder Model — Added By Himel Sarkar 09-06-2026
LEARNING FLOW:
- PurchaseOrder = supplier থেকে product কেনার order
- Status workflow: Draft → Approved → Received
  Draft    = তৈরি হয়েছে কিন্তু approve হয়নি
  Approved = Manager/Admin approve করেছে
  Received = product physically পাওয়া গেছে, stock update হবে
- One PurchaseOrder has many PurchaseOrderLines (items)
- এটা ERP-এর core — real company-তে এভাবেই কাজ হয়
================================================================
*/
public class PurchaseOrder
{
    public int Id { get; set; }
    public string OrderNo { get; set; } = string.Empty; // e.g. PO-2026-001
    public string SupplierName { get; set; } = string.Empty;
    public string SupplierEmail { get; set; } = string.Empty;
    public string SupplierPhone { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft"; // Draft, Approved, Received
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public int CreatedByEmployeeId { get; set; }
    public string Notes { get; set; } = string.Empty;

    // Navigation property — one order has many lines
    public List<PurchaseOrderLine> Lines { get; set; } = new();
}
