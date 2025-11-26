namespace NetMailGun.Infrastructure.Firestore;

public class FirestoreCredentialOptions
{
    public string Type { get; set; } = null!;
    public string ProjectId { get; set; } = null!;
    public string PrivateKeyId { get; set; } = null!;
    public string PrivateKey { get; set; } = null!;
    public string ClientEmail { get; set; } = null!;
    public string ClientId { get; set; } = null!;
    public string AuthUri { get; set; } = null!;
    public string TokenUri { get; set; } = null!;
    public string AuthProviderX509CertUrl { get; set; } = null!;
    public string ClientX509CertUrl { get; set; } = null!;
    public string UniverseDomain { get; set; } = null!;
}