namespace FirstErpSystem.Api.Models;

//Added By Himel Sarkar 08-06-2025
public class Employee
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public DateTime JoinDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string Role { get; set; } = "Staff"; // Admin, Manager, Staff
}
//End By Himel Sarkar 08-06-2025
