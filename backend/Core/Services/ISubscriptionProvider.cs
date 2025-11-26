using System.Diagnostics.CodeAnalysis;

namespace NetMailGun.Core.Services;

public interface ISubscriptionProvider
{
    public Task<string> SubscribeAsync(Guid formId, string emailAddress, string? name, string? lastName = null,
        string? phoneNumber = null);

    public Task UnsubscribeAsync(string token);
    public Task<PaginationResult<SubscriberEntity>> GetSubscribersAsync(Guid formId, int page, int pageSize);
    public Task<List<SubscriberEntity>> GetAllSubscribersAsync(Guid formId);
}

public class SubscriberEntity
{
    public required Guid FormId { get; set; }
    public required string UnsubscriptionToken { get; set; }
    public required string EmailAddress { get; set; } = null!;
    public string? Name { get; set; } = null!;
    public string? Lastname { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}

public record struct PaginationResult<T>(
    int Page,
    int PageSize,
    int TotalPages,
    long TotalItems,
    ICollection<T> Items
)
{
    
    public static PaginationResult<T> Empty(int page, int pageSize) 
        => From(page, pageSize, 0, []);
    public static PaginationResult<T> From(int page, int pageSize, long totalItems)
        => From(page, pageSize, totalItems, []);
    public static PaginationResult<T> From(int page, int pageSize, long totalItems, ICollection<T> items) =>
        new(
            page,
            pageSize,
            (int)Math.Ceiling((float)totalItems / pageSize),
            totalItems,
            items
        );
}

public interface ISubscriptionProviderFactory
{
    public bool TryCreate(string name, [NotNullWhen(true)] out ISubscriptionProvider? instance);
    public IEnumerable<string> EnumerateProviders();
}