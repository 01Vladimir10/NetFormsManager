namespace NetFormsManager.Api;

public class FormDto : FormRequestDto
{
    public required Guid Id { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastUpdatedAt { get; init; }
}
