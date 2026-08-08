using AYIGroupMarket.Domain.Common;

namespace AYIGroupMarket.Domain.Entities;

public class Notification : BaseEntity
{
    public string UserId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string MessageEn { get; set; } = string.Empty;

    public string? LinkUrl { get; set; } // e.g. /mon-compte/commandes
    public bool IsRead { get; set; } = false;
}