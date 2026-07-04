using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using SecureVault.Services;
using SecureVault.ViewModels;
using System;
using System.IO;

namespace SecureVault.Views;

public sealed partial class MainPage : Page
{
    private MainViewModel? _vm;
    private bool _passwordEditorSync;

    public MainPage()
    {
        InitializeComponent();
        LoadBannerImage();
    }

    private void LoadBannerImage()
    {
        var bannerPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Banner.png");
        if (!File.Exists(bannerPath))
            return;

        BannerImage.Source = new BitmapImage(new Uri(bannerPath));
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (AppServices.Session is null)
        {
            MainWindow.Instance?.NavigateToUnlock();
            return;
        }

        _vm = new MainViewModel(
            AppServices.Session,
            () => MainWindow.Instance?.NavigateToUnlock(),
            DispatcherQueue);
        DataContext = _vm;
        _vm.PropertyChanged += OnViewModelPropertyChanged;
        SyncPasswordEditorFromViewModel();
        SyncFavoriteGlyph();
    }

    private void SyncFavoriteGlyph()
    {
        if (_vm is null)
            return;

        FavoriteButton.Content = _vm.FavoriteGlyph;
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_vm is null)
            return;

        if (e.PropertyName is nameof(MainViewModel.Password) or nameof(MainViewModel.SelectedEntry))
            SyncPasswordEditorFromViewModel();

        if (e.PropertyName is nameof(MainViewModel.FavoriteIsOn) or nameof(MainViewModel.SelectedEntry))
            SyncFavoriteGlyph();
    }

    private void SyncPasswordEditorFromViewModel()
    {
        if (_vm is null)
            return;

        if (EditorPasswordBox.Password == _vm.Password)
            return;

        _passwordEditorSync = true;
        EditorPasswordBox.Password = _vm.Password;
        _passwordEditorSync = false;
    }

    private void OnEditorPasswordChanged(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (_passwordEditorSync || _vm is null)
            return;

        _vm.Password = EditorPasswordBox.Password;
    }

}
