using AYIGroupMarket.Domain.Common;
using AYIGroupMarket.Domain.Enums;

namespace AYIGroupMarket.Domain.Entities;

public class WholesaleAccount : BaseEntity
{
    public string UserId { get; set; } = string.Empty; // FK to ApplicationUser.Id

    public string CompanyName { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string BusinessAddress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? BusinessRegistrationInfo { get; set; }
    public string ExpectedOrderVolume { get; set; } = string.Empty;

    public WholesaleStatus Status { get; set; } = WholesaleStatus.Pending;
    public string? RejectionReason { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedByUserId { get; set; }
}