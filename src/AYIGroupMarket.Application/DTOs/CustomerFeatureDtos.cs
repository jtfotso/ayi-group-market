namespace AYIGroupMarket.Application.DTOs;

public record ProductReviewDto(
    Guid Id,
    string UserDisplayName,
    int Rating,
    string Title,
    string Comment,
    bool IsVerifiedPurchase,
    DateTime CreatedAt);

public record ReviewSummaryDto(
    double AverageRating,
    int TotalReviews,
    List<ProductReviewDto> Reviews);

public record FavoriteProductDto(
    Guid ProductId,
    string Name,
    string NameEn,
    string Slug,
    decimal RetailPrice,
    string? PrimaryImageUrl);