using InventoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Application.Common;

public interface IApplicationDbContext
{
    DbSet<Product> Products { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
