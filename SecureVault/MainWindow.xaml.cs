using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using SecureVault.Views;

namespace SecureVault;

public sealed partial class MainWindow : Window
{
    public static MainWindow? Instance { get; private set; }

    public MainWindow()
    {
        Instance = this;
        InitializeComponent();

        Closed += (_, _) =>
        {
            if (ReferenceEquals(Instance, this))
                Instance = null;
        };

        ApplySystemBackdrop();
        AppWindow.SetIcon("Assets/AppIcon.ico");
        RootFrame.Navigate(typeof(UnlockPage));
    }

    private void ApplySystemBackdrop()
    {
        // MicaBackdrop and TitleBar require Windows 11; keep Windows 10 compatible.
        if (Environment.OSVersion.Version.Build >= 22000)
            SystemBackdrop = new MicaBackdrop();
    }

    public void NavigateToUnlock() => RootFrame.Navigate(typeof(UnlockPage));
}
