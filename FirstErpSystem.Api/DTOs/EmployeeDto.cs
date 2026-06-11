namespace FirstErpSystem.Api.DTOs;

//Added By Himel Sarkar 08-06-2025
public class RegisterEmployeeDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public DateTime JoinDate { get; set; }
    public string Role { get; set; } = "Staff";
}

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class UpdateEmployeeDto
{
    public string FullName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public bool IsActive { get; set; }
}
//End By Himel Sarkar 08-06-2025
