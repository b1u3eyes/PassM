using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace SecureVault.Core.Services;

public sealed class CryptoService
{
    public const int SaltSize = 16;
    public const int KeySize = 32;
    public const int NonceSize = 12;
    public const int TagSize = 16;

    private const int FileVersion = 1;
    private const int Pbkdf2Iterations = 310_000;
    private static ReadOnlySpan<byte> Magic => "SVLT"u8;

    public byte[] GenerateSalt()
    {
        var salt = new byte[SaltSize];
        RandomNumberGenerator.Fill(salt);
        return salt;
    }

    public byte[] DeriveKey(string masterPassword, byte[] salt)
    {
        ArgumentNullException.ThrowIfNull(salt);
        if (salt.Length != SaltSize)
            throw new ArgumentException("Salt must be 16 bytes.", nameof(salt));

        var passwordBytes = Encoding.UTF8.GetBytes(masterPassword);
        try
        {
            return Rfc2898DeriveBytes.Pbkdf2(
                passwordBytes,
                salt,
                Pbkdf2Iterations,
                HashAlgorithmName.SHA256,
                KeySize);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(passwordBytes);
        }
    }

    public byte[] EncryptVault(byte[] key, byte[] salt, ReadOnlySpan<byte> plaintext)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(salt);
        if (key.Length != KeySize || salt.Length != SaltSize)
            throw new ArgumentException("Invalid key or salt size.");

        var nonce = new byte[NonceSize];
        RandomNumberGenerator.Fill(nonce);

        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[TagSize];
        using (var aes = new AesGcm(key, TagSize))
        {
            aes.Encrypt(nonce, plaintext, ciphertext, tag);
        }

        return PackFile(salt, nonce, ciphertext, tag);
    }

    public byte[] DecryptVault(byte[] key, ReadOnlySpan<byte> fileBytes)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (key.Length != KeySize)
            throw new ArgumentException("Key must be 32 bytes.", nameof(key));

        var headerSize = Magic.Length + 4 + SaltSize + NonceSize + TagSize;
        if (fileBytes.Length < headerSize)
            throw new InvalidDataException("Vault file is too small.");
        if (!fileBytes.Slice(0, Magic.Length).SequenceEqual(Magic))
            throw new InvalidDataException("Unrecognized vault file.");

        var version = BinaryPrimitives.ReadUInt32LittleEndian(fileBytes.Slice(Magic.Length, 4));
        if (version != FileVersion)
            throw new InvalidDataException($"Unsupported vault version: {version}.");

        var offset = Magic.Length + 4 + SaltSize;
        var nonce = fileBytes.Slice(offset, NonceSize);
        offset += NonceSize;

        var payload = fileBytes.Slice(offset);
        if (payload.Length < TagSize)
            throw new InvalidDataException("Invalid ciphertext.");

        var ciphertextLength = payload.Length - TagSize;
        var ciphertext = payload.Slice(0, ciphertextLength);
        var tag = payload.Slice(ciphertextLength);

        var plaintext = new byte[ciphertextLength];
        using (var aes = new AesGcm(key, TagSize))
        {
            aes.Decrypt(nonce, ciphertext, tag, plaintext);
        }

        return plaintext;
    }

    public byte[] ReadSalt(ReadOnlySpan<byte> fileBytes)
    {
        if (fileBytes.Length < Magic.Length + 4 + SaltSize)
            throw new InvalidDataException("Vault file is too small.");
        if (!fileBytes.Slice(0, Magic.Length).SequenceEqual(Magic))
            throw new InvalidDataException("Unrecognized vault file.");

        return fileBytes.Slice(Magic.Length + 4, SaltSize).ToArray();
    }

    private static byte[] PackFile(byte[] salt, byte[] nonce, byte[] ciphertext, byte[] tag)
    {
        var totalLength = Magic.Length + 4 + SaltSize + NonceSize + ciphertext.Length + TagSize;
        var file = new byte[totalLength];
        var destination = file.AsSpan();

        Magic.CopyTo(destination);
        var offset = Magic.Length;
        BinaryPrimitives.WriteUInt32LittleEndian(destination.Slice(offset), FileVersion);
        offset += 4;

        salt.AsSpan().CopyTo(destination.Slice(offset));
        offset += SaltSize;
        nonce.AsSpan().CopyTo(destination.Slice(offset));
        offset += NonceSize;
        ciphertext.AsSpan().CopyTo(destination.Slice(offset));
        offset += ciphertext.Length;
        tag.AsSpan().CopyTo(destination.Slice(offset));

        return file;
    }

    public static void ZeroMemory(byte[]? buffer)
    {
        if (buffer is not null)
            CryptographicOperations.ZeroMemory(buffer);
    }
}
