using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FirstErpSystem.Api.Data;
using FirstErpSystem.Api.Models;
using FirstErpSystem.Api.DTOs;

namespace FirstErpSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase
{
    //Added By Himel Sarkar 08-06-2025
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public EmployeeController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    // POST api/employee/register
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterEmployeeDto dto)
    {
        if (await _context.Employees.AnyAsync(e => e.Email == dto.Email))
            return BadRequest(new { message = "Email already exists" });

        var employee = new Employee
        {
            FullName     = dto.FullName,
            Email        = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Department   = dto.Department,
            Position     = dto.Position,
            Salary       = dto.Salary,
            JoinDate     = dto.JoinDate,
            Role         = dto.Role
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Employee registered successfully", id = employee.Id });
    }

    // POST api/employee/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Email == dto.Email);

        if (employee == null || !BCrypt.Net.BCrypt.Verify(dto.Password, employee.PasswordHash))
            return Unauthorized(new { message = "Invalid email or password" });

        var token = GenerateJwtToken(employee);

        return Ok(new
        {
            token    = token,
            employee = new
            {
                employee.Id,
                employee.FullName,
                employee.Email,
                employee.Role,
                employee.Department
            }
        });
    }

    // GET api/employee
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        //Added By Himel Sarkar 08-06-2025 - Role based access
        var currentUserId   = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var currentUserRole = User.FindFirst(ClaimTypes.Role)!.Value;
        var currentUserDept = User.FindFirst("Department")?.Value ?? "";

        List<object> employees;

        if (currentUserRole == "Admin")
        {
            // Admin — সব employee দেখবে
            employees = await _context.Employees
                .Where(e => e.IsActive)
                .Select(e => (object)new
                {
                    e.Id, e.FullName, e.Email,
                    e.Department, e.Position,
                    e.Salary, e.JoinDate, e.Role
                })
                .ToListAsync();
        }
        else if (currentUserRole == "Manager")
        {
            // Manager — নিজের department এর employee দেখবে
            employees = await _context.Employees
                .Where(e => e.IsActive && e.Department == currentUserDept)
                .Select(e => (object)new
                {
                    e.Id, e.FullName, e.Email,
                    e.Department, e.Position,
                    e.Salary, e.JoinDate, e.Role
                })
                .ToListAsync();
        }
        else
        {
            // Staff — শুধু নিজের profile দেখবে
            employees = await _context.Employees
                .Where(e => e.IsActive && e.Id == currentUserId)
                .Select(e => (object)new
                {
                    e.Id, e.FullName, e.Email,
                    e.Department, e.Position,
                    e.Salary, e.JoinDate, e.Role
                })
                .ToListAsync();
        }
        //End By Himel Sarkar 08-06-2025

        return Ok(employees);
    }

    // GET api/employee/5
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        //Added By Himel Sarkar 08-06-2025 - Role based access
        var currentUserId   = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var currentUserRole = User.FindFirst(ClaimTypes.Role)!.Value;

        // Staff নিজের profile ছাড়া অন্য কারো দেখতে পারবে না
        if (currentUserRole == "Staff" && currentUserId != id)
            return Forbid();
        //End By Himel Sarkar 08-06-2025

        var employee = await _context.Employees.FindAsync(id);

        if (employee == null || !employee.IsActive)
            return NotFound(new { message = "Employee not found" });

        return Ok(employee);
    }

    // PUT api/employee/5
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, UpdateEmployeeDto dto)
    {
        //Added By Himel Sarkar 08-06-2025 - Only Admin can update
        var currentUserRole = User.FindFirst(ClaimTypes.Role)!.Value;
        if (currentUserRole != "Admin")
            return Forbid();
        //End By Himel Sarkar 08-06-2025

        var employee = await _context.Employees.FindAsync(id);

        if (employee == null)
            return NotFound(new { message = "Employee not found" });

        employee.FullName   = dto.FullName;
        employee.Department = dto.Department;
        employee.Position   = dto.Position;
        employee.Salary     = dto.Salary;
        employee.IsActive   = dto.IsActive;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Employee updated successfully" });
    }

    // DELETE api/employee/5
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        //Added By Himel Sarkar 08-06-2025 - Only Admin can delete
        var currentUserRole = User.FindFirst(ClaimTypes.Role)!.Value;
        if (currentUserRole != "Admin")
            return Forbid();
        //End By Himel Sarkar 08-06-2025

        var employee = await _context.Employees.FindAsync(id);

        if (employee == null)
            return NotFound(new { message = "Employee not found" });

        employee.IsActive = false;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Employee deactivated successfully" });
    }

    // JWT token generate
    private string GenerateJwtToken(Employee employee)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString()),
            new Claim(ClaimTypes.Email,          employee.Email),
            new Claim(ClaimTypes.Name,           employee.FullName),
            new Claim(ClaimTypes.Role,           employee.Role),
            new Claim("Department",              employee.Department)
        };

        var token = new JwtSecurityToken(
            issuer:   jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims:   claims,
            expires:  DateTime.UtcNow.AddMinutes(
                double.Parse(jwtSettings["ExpiryInMinutes"]!)),
            signingCredentials: new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    //End By Himel Sarkar 08-06-2025
}
