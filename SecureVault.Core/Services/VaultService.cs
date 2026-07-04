using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SecureVault.Core.Models;

namespace SecureVault.Core.Services;

public sealed class VaultService
{
    private readonly CryptoService _crypto = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    public string VaultFilePath { get; }

    public VaultService(string? vaultFilePath = null)
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SecureVault");
        VaultFilePath = vaultFilePath ?? Path.Combine(folder, "vault.enc");
    }

    public bool VaultExists() => File.Exists(VaultFilePath);

    public (VaultDocument Document, byte[] Salt, byte[] Key) Unlock(string masterPassword)
    {
        var fileBytes = File.ReadAllBytes(VaultFilePath);
        var salt = _crypto.ReadSalt(fileBytes);
        var key = _crypto.DeriveKey(masterPassword, salt);
        byte[]? plaintext = null;

        try
        {
            plaintext = _crypto.DecryptVault(key, fileBytes);
            var json = Encoding.UTF8.GetString(plaintext);
            var document = JsonSerializer.Deserialize<VaultDocument>(json, _jsonOptions) ?? new VaultDocument();
            return (document, salt, key);
        }
        catch
        {
            CryptoService.ZeroMemory(key);
            throw;
        }
        finally
        {
            if (plaintext is not null)
                CryptographicOperations.ZeroMemory(plaintext);
        }
    }

    public (VaultDocument Document, byte[] Salt, byte[] Key) CreateNew(string masterPassword)
    {
        var salt = _crypto.GenerateSalt();
        var key = _crypto.DeriveKey(masterPassword, salt);
        return (new VaultDocument(), salt, key);
    }

    public void Save(byte[] key, byte[] salt, VaultDocument document)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(salt);

        var json = JsonSerializer.Serialize(document, _jsonOptions);
        var plaintext = Encoding.UTF8.GetBytes(json);
        try
        {
            var data = _crypto.EncryptVault(key, salt, plaintext);
            WriteAllBytesAtomic(VaultFilePath, data);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plaintext);
        }
    }

    private static void WriteAllBytesAtomic(string path, byte[] data)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var tempPath = path + ".tmp";
        File.WriteAllBytes(tempPath, data);
        if (File.Exists(path))
            File.Replace(tempPath, path, null);
        else
            File.Move(tempPath, path);
    }
}
