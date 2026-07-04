namespace SecureVault.Core.Models;

public sealed class VaultDocument
{
    public int SchemaVersion { get; set; } = 1;

    public List<VaultEntry> Entries { get; set; } = [];
}
