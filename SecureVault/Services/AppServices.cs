using SecureVault.Core.Services;

namespace SecureVault.Services;

public static class AppServices
{
    public static VaultService Vault { get; } = new();

    public static VaultSession? Session { get; set; }
}
