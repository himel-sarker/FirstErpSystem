namespace FirstErpSystem.Api.Models;

/*
================================================================
Product Model — Added By Himel Sarker 09-06-2026
LEARNING FLOW:
- Product = ERP-এর Inventory-তে যা কিনি বা বিক্রি করি
- StockQuantity = এখন কতটুকু stock আছে
- ReorderLevel = কত কমে গেলে নতুন order দিতে হবে
- IsActive = soft delete (delete করলে DB থেকে মুছি না)
================================================================
*/
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty; // unique product code e.g. PRD-001
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
    public int ReorderLevel { get; set; } // alert when stock falls below this
    public string Unit { get; set; } = "pcs"; // pcs, kg, ltr, box
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
