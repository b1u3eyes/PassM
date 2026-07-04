using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SecureVault.Services;
using SecureVault.ViewModels;

namespace SecureVault.Views;

public sealed partial class UnlockPage : Page
{
    private readonly UnlockViewModel _vm;

    public UnlockPage()
    {
        InitializeComponent();
        _vm = new UnlockViewModel(() => MainWindow.Instance?.RootFrame.Navigate(typeof(MainPage)));
        DataContext = _vm;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var hasVault = _vm.VaultExists;
        SubtitleText.Text = hasVault
            ? "Introduceți parola master pentru a debloca vault-ul local."
            : "Creați o parolă master puternică. Datele rămân criptate pe acest PC (offline).";

        ConfirmPanel.Visibility = hasVault ? Visibility.Collapsed : Visibility.Visible;
        PrimaryActionButton.Visibility = hasVault ? Visibility.Visible : Visibility.Collapsed;
        CreateActionButton.Visibility = hasVault ? Visibility.Collapsed : Visibility.Visible;
    }

    private void OnMasterPasswordChanged(object sender, RoutedEventArgs e) =>
        _vm.MasterPassword = MasterPasswordBox.Password;

    private void OnConfirmPasswordChanged(object sender, RoutedEventArgs e) =>
        _vm.ConfirmPassword = ConfirmPasswordBox.Password;

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (AppServices.Session is not null)
            MainWindow.Instance?.RootFrame.Navigate(typeof(MainPage));
    }
}
