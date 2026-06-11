namespace FirstErpSystem.Api.Models;

/*
================================================================
SalesOrderLine Model — Added By Himel Sarkar 09-06-2026
LEARNING FLOW:
- SalesOrder এর প্রতিটা product item এখানে থাকে
- PurchaseOrderLine এর মতোই structure
- Example:
  SO-2026-001
  ├── Line 1: Laptop × 2 @ ৳60,000 = ৳120,000
  ├── Line 2: Mouse  × 2 @ ৳600   = ৳1,200
  └── Total: ৳121,200
- Payment হলে প্রতিটা Line এর Quantity
  StockTransaction OUT হিসেবে record হবে
================================================================
*/
public class SalesOrderLine
{
    public int Id { get; set; }
    public int SalesOrderId { get; set; }
    public SalesOrder SalesOrder { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;
}
