// ================================================================
// GetProductsQuery.cs — Added By Himel Sarkar 10-06-2026
// LEARNING: CQRS Query = শুধু Data Read করার request
// Write থেকে Read আলাদা করলে performance optimize করা সহজ হয়
// ================================================================

using MediatR;
using InventoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Application.Products.Queries;

public record GetProductsQuery : IRequest<List<Product>>;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, List<Product>>
{
    private readonly IApplicationDbContext _context;

    public GetProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Products.ToListAsync(cancellationToken);
    }
}
