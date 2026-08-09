using AYIGroupMarket.Domain.Common;
using AYIGroupMarket.Domain.Enums;

namespace AYIGroupMarket.Domain.Entities;

public class InventoryTransaction : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public InventoryTransactionType Type { get; set; }
    public int QuantityChange { get; set; } // positive for Purchase/Cancellation/Adjustment-up, negative for Sale/Reservation
    public int ResultingStock { get; set; } // snapshot of StockQuantity after this transaction, for audit clarity

    public string? Reason { get; set; } // e.g. "Order AYI-2026-000001", "Manual correction", admin note
    public string? PerformedByUserId { get; set; } // null for system-triggered (e.g. a sale)
}