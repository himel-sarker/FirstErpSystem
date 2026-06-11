// ================================================================
// CreateProductCommand.cs — Added By Himel Sarkar 10-06-2026
// LEARNING: CQRS Command = Data Change/Create করার request
// MediatR automatically এই handler টা call করবে
// Controller এবং Business logic আলাদা হয়ে যায়!
// ================================================================

using MediatR;

namespace InventoryService.Application.Products.Commands;

// Command Definition
public record CreateProductCommand(
    string Name, string Code, decimal UnitPrice, int StockQuantity
) : IRequest<int>;

// Command Handler
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
{
    // LEARNING: Dependency Injection দিয়ে DB context পাওয়া
    private readonly IApplicationDbContext _context;

    public CreateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Domain.Entities.Product
        {
            Name = request.Name,
            Code = request.Code,
            UnitPrice = request.UnitPrice,
            StockQuantity = request.StockQuantity
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);
        return product.Id;
    }
}
