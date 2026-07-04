using System.Collections.ObjectModel;
using SecureVault.Core.Models;
using SecureVault.Core.Services;

namespace SecureVault.Services;

public sealed class VaultSession : IDisposable
{
    public ObservableCollection<VaultEntry> Entries { get; }

    public byte[] Salt { get; }

    private byte[] _key;

    public VaultSession(VaultDocument document, byte[] salt, byte[] key)
    {
        Salt = salt;
        _key = key;
        Entries = new ObservableCollection<VaultEntry>(document.Entries.Select(e => e.Clone()));
    }

    public byte[] GetKeyMaterial() => _key;

    public VaultDocument ToDocument() => new()
    {
        SchemaVersion = 1,
        Entries = Entries.Select(e => e.Clone()).ToList(),
    };

    public void Dispose()
    {
        CryptoService.ZeroMemory(_key);
        _key = Array.Empty<byte>();
    }
}
