namespace FirstErpSystem.Api.Models;

/*
================================================================
PurchaseOrderLine Model — Added By Himel Sarkar 09-06-2026
LEARNING FLOW:
- একটা PurchaseOrder-এ অনেক product থাকতে পারে
- প্রতিটা product-এর জন্য একটা PurchaseOrderLine
- Example:
  PO-2026-001
  ├── Line 1: Laptop × 5 @ ৳50,000 = ৳250,000
  ├── Line 2: Mouse  × 10 @ ৳500  = ৳5,000
  └── Total: ৳255,000
- UnitPrice × Quantity = LineTotal
================================================================
*/
public class PurchaseOrderLine
{
    public int Id { get; set; }
    public int PurchaseOrderId { get; set; }
    public PurchaseOrder PurchaseOrder { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal => UnitPrice * Quantity; // calculated field
}
