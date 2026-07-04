using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using SecureVault.Core;
using SecureVault.Core.Models;
using SecureVault.Core.Services;
using SecureVault.Helpers;
using SecureVault.Services;

namespace SecureVault.ViewModels;

public enum EntryListFilter
{
    All,
    Favorites,
    Weak,
}

public sealed partial class MainViewModel : ObservableObject
{
    private readonly VaultSession _session;
    private readonly VaultService _vault = AppServices.Vault;
    private readonly Action _navigateUnlock;
    private readonly DispatcherQueue _dispatcher;
    private readonly ObservableCollection<VaultEntry> _filteredBacking = new();
    private readonly PasswordGenerator _generator = new();

    public MainViewModel(VaultSession session, Action navigateUnlock, DispatcherQueue dispatcher)
    {
        _session = session;
        _navigateUnlock = navigateUnlock;
        _dispatcher = dispatcher;
        _session.Entries.CollectionChanged += OnEntriesChanged;
        RebuildFilter();
    }

    public ObservableCollection<VaultEntry> FilteredEntries => _filteredBacking;

    public string FavoriteGlyph => FavoriteIsOn ? "\u2605" : "\u2606";

    [ObservableProperty]
    private EntryListFilter _filter = EntryListFilter.All;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private VaultEntry? _selectedEntry;

    [ObservableProperty]
    private string _site = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _url = string.Empty;

    [ObservableProperty]
    private string _category = string.Empty;

    [ObservableProperty]
    private int _generatorLength = 12;

    [ObservableProperty]
    private bool _genUseLower = true;

    [ObservableProperty]
    private bool _genUseUpper = true;

    [ObservableProperty]
    private bool _genUseDigits = true;

    [ObservableProperty]
    private bool _genUseSymbols;

    [ObservableProperty]
    private string _strengthLabel = "Putere: —";

    [ObservableProperty]
    private double _strengthRatio;

    [ObservableProperty]
    private bool _favoriteIsOn;

    public string AppVersion => "1.0.0";

    partial void OnFavoriteIsOnChanged(bool value) => OnPropertyChanged(nameof(FavoriteGlyph));

    partial void OnFilterChanged(EntryListFilter value) => RebuildFilter();

    partial void OnSearchTextChanged(string value) => RebuildFilter();

    partial void OnSelectedEntryChanged(VaultEntry? value)
    {
        if (value is null)
        {
            FavoriteIsOn = false;
            Site = string.Empty;
            Email = string.Empty;
            Username = string.Empty;
            Password = string.Empty;
            Url = string.Empty;
            Category = string.Empty;
            UpdateStrengthUi();
            return;
        }

        FavoriteIsOn = value.IsFavorite;
        Site = value.Site;
        Email = value.Email;
        Username = value.Username;
        Password = value.Password;
        Url = value.Url;
        Category = value.Category;
        UpdateStrengthUi();
    }

    partial void OnPasswordChanged(string value) => UpdateStrengthUi();

    private void OnEntriesChanged(object? sender, NotifyCollectionChangedEventArgs e) => RebuildFilter();

    private void RebuildFilter()
    {
        _filteredBacking.Clear();
        var q = SearchText.Trim();
        foreach (var entry in _session.Entries)
        {
            if (Filter == EntryListFilter.Favorites && !entry.IsFavorite)
                continue;

            if (Filter == EntryListFilter.Weak && !PasswordStrengthEvaluator.IsLocallyWeak(entry.Password))
                continue;

            if (q.Length > 0)
            {
                var hay = $"{entry.Site}{entry.Email}{entry.Username}".ToLowerInvariant();
                if (!hay.Contains(q.ToLowerInvariant(), StringComparison.Ordinal))
                    continue;
            }

            _filteredBacking.Add(entry);
        }

        if (SelectedEntry is not null && !_filteredBacking.Contains(SelectedEntry))
            SelectedEntry = null;
    }

    private void UpdateStrengthUi()
    {
        var band = PasswordStrengthEvaluator.GetBand(Password);
        StrengthLabel = $"Putere: {PasswordStrengthEvaluator.BandDisplayNameRo(band)}";
        StrengthRatio = PasswordStrengthEvaluator.BandToRatio(band);
    }

    [RelayCommand]
    private void Save()
    {
        if (SelectedEntry is null)
        {
            var entry = new VaultEntry
            {
                Id = Guid.NewGuid(),
                Site = Site,
                Email = Email,
                Username = Username,
                Password = Password,
                Url = Url,
                Category = Category,
                IsFavorite = false,
                UpdatedAt = DateTimeOffset.Now,
            };
            _session.Entries.Add(entry);
            SelectedEntry = entry;
        }
        else
        {
            SelectedEntry.Site = Site;
            SelectedEntry.Email = Email;
            SelectedEntry.Username = Username;
            SelectedEntry.Password = Password;
            SelectedEntry.Url = Url;
            SelectedEntry.Category = Category;
            SelectedEntry.UpdatedAt = DateTimeOffset.Now;
        }

        Persist();
        RebuildFilter();
    }

    [RelayCommand]
    private void Delete()
    {
        if (SelectedEntry is null)
            return;

        _session.Entries.Remove(SelectedEntry);
        SelectedEntry = null;
        Persist();
        RebuildFilter();
    }

    [RelayCommand]
    private void NewEntry()
    {
        SelectedEntry = null;
    }

    [RelayCommand]
    private void ToggleFavorite()
    {
        if (SelectedEntry is null)
            return;

        SelectedEntry.IsFavorite = !SelectedEntry.IsFavorite;
        FavoriteIsOn = SelectedEntry.IsFavorite;
        Persist();
        RebuildFilter();
    }

    [RelayCommand]
    private void Generate()
    {
        try
        {
            Password = _generator.Generate(
                GeneratorLength,
                GenUseLower,
                GenUseUpper,
                GenUseDigits,
                GenUseSymbols,
                excludeAmbiguous: false);
            UpdateStrengthUi();
        }
        catch
        {
            // Invalid combination; ignore.
        }
    }

    [RelayCommand]
    private void CopyPassword()
    {
        if (string.IsNullOrEmpty(Password))
            return;

        ClipboardHelper.CopyText(Password, _dispatcher, TimeSpan.FromSeconds(45));
    }

    [RelayCommand]
    private void CopyUrl()
    {
        if (string.IsNullOrEmpty(Url))
            return;

        ClipboardHelper.CopyText(Url, _dispatcher, TimeSpan.FromSeconds(45));
    }

    [RelayCommand]
    private void Lock()
    {
        AppServices.Session?.Dispose();
        AppServices.Session = null;
        _navigateUnlock();
    }

    [RelayCommand]
    private void SetFilterAll() => Filter = EntryListFilter.All;

    [RelayCommand]
    private void SetFilterFavorites() => Filter = EntryListFilter.Favorites;

    [RelayCommand]
    private void SetFilterWeak() => Filter = EntryListFilter.Weak;

    private void Persist()
    {
        var key = _session.GetKeyMaterial();
        _vault.Save(key, _session.Salt, _session.ToDocument());
    }
}
