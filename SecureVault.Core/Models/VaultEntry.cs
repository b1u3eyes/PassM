using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace SecureVault.Core.Models;

public sealed class VaultEntry : INotifyPropertyChanged
{
    private Guid _id;
    private string _site = string.Empty;
    private string _email = string.Empty;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _url = string.Empty;
    private string _category = string.Empty;
    private bool _isFavorite;
    private DateTimeOffset _updatedAt;

    public Guid Id
    {
        get => _id;
        set => SetField(ref _id, value);
    }

    public string Site
    {
        get => _site;
        set => SetField(ref _site, value);
    }

    public string Email
    {
        get => _email;
        set => SetField(ref _email, value);
    }

    public string Username
    {
        get => _username;
        set => SetField(ref _username, value);
    }

    public string Password
    {
        get => _password;
        set => SetField(ref _password, value);
    }

    public string Url
    {
        get => _url;
        set => SetField(ref _url, value);
    }

    public string Category
    {
        get => _category;
        set => SetField(ref _category, value);
    }

    public bool IsFavorite
    {
        get => _isFavorite;
        set => SetField(ref _isFavorite, value);
    }

    public DateTimeOffset UpdatedAt
    {
        get => _updatedAt;
        set => SetField(ref _updatedAt, value);
    }

    [JsonIgnore]
    public string DisplayTitle => string.IsNullOrWhiteSpace(Site) ? "(fără titlu)" : Site;

    public event PropertyChangedEventHandler? PropertyChanged;

    public VaultEntry Clone() => new()
    {
        Id = Id,
        Site = Site,
        Email = Email,
        Username = Username,
        Password = Password,
        Url = Url,
        Category = Category,
        IsFavorite = IsFavorite,
        UpdatedAt = UpdatedAt,
    };

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);

        if (propertyName is nameof(Site) or nameof(Email))
            OnPropertyChanged(nameof(DisplayTitle));

        return true;
    }
}
