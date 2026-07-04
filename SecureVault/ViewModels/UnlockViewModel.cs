using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using SecureVault.Core.Services;
using SecureVault.Services;

namespace SecureVault.ViewModels;

public sealed partial class UnlockViewModel : ObservableObject
{
    private readonly VaultService _vault = AppServices.Vault;
    private readonly Action _navigateMain;

    public UnlockViewModel(Action navigateMain)
    {
        _navigateMain = navigateMain;
    }

    [ObservableProperty]
    private string _masterPassword = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public Visibility ErrorVisibility =>
        string.IsNullOrEmpty(ErrorMessage) ? Visibility.Collapsed : Visibility.Visible;

    partial void OnErrorMessageChanged(string value) => OnPropertyChanged(nameof(ErrorVisibility));

    public bool VaultExists => _vault.VaultExists();

    [RelayCommand]
    private void Unlock()
    {
        ErrorMessage = string.Empty;
        if (string.IsNullOrEmpty(MasterPassword))
        {
            ErrorMessage = "Introduceți parola master.";
            return;
        }

        try
        {
            var (doc, salt, key) = _vault.Unlock(MasterPassword);
            AppServices.Session?.Dispose();
            AppServices.Session = new VaultSession(doc, salt, key);
            MasterPassword = string.Empty;
            _navigateMain();
        }
        catch
        {
            ErrorMessage = "Parolă incorectă sau fișier deteriorat.";
        }
    }

    [RelayCommand]
    private void Create()
    {
        ErrorMessage = string.Empty;
        if (MasterPassword.Length < 10)
        {
            ErrorMessage = "Parola master trebuie să aibă cel puțin 10 caractere.";
            return;
        }

        if (!string.Equals(MasterPassword, ConfirmPassword, StringComparison.Ordinal))
        {
            ErrorMessage = "Parolele nu coincid.";
            return;
        }

        try
        {
            var (doc, salt, key) = _vault.CreateNew(MasterPassword);
            AppServices.Session?.Dispose();
            AppServices.Session = new VaultSession(doc, salt, key);
            _vault.Save(key, salt, AppServices.Session.ToDocument());
            MasterPassword = string.Empty;
            ConfirmPassword = string.Empty;
            _navigateMain();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Nu s-a putut crea vault-ul: {ex.Message}";
        }
    }
}
