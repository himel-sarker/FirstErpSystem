using MediatR;
using InventoryService.Application.Common;
using InventoryService.Domain.Entities;

namespace InventoryService.Application.Products.Commands;

public record CreateProductCommand(
    string Name, string Code, decimal UnitPrice, int StockQuantity
) : IRequest<int>;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
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
