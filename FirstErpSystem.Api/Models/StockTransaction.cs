namespace FirstErpSystem.Api.Models;

/*
================================================================
StockTransaction Model — Added By Himel Sarker 09-06-2026
LEARNING FLOW:
- প্রতিবার stock in বা out হলে এই table-এ record হয়
- TransactionType = "IN" (stock আসলে) / "OUT" (stock গেলে)
- ReferenceNo = কোন Purchase/Sales Order থেকে এলো
- Quantity = কতটুকু stock change হলো
- এই table দিয়ে stock history track করা যায়
================================================================
*/
public class StockTransaction
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!; // navigation property

    public string TransactionType { get; set; } = "IN"; // IN or OUT
    public int Quantity { get; set; }
    public string ReferenceNo { get; set; } = string.Empty; // e.g. PO-001, SO-001
    public string Remarks { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public int CreatedByEmployeeId { get; set; } // who did this transaction
}
