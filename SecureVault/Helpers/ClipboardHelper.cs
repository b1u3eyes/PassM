using Microsoft.UI.Dispatching;
using Windows.ApplicationModel.DataTransfer;

namespace SecureVault.Helpers;

public static class ClipboardHelper
{
    public static void CopyText(string text, DispatcherQueue dispatcher, TimeSpan clearAfter)
    {
        var package = new DataPackage();
        package.SetText(text);
        Clipboard.SetContent(package);

        if (clearAfter <= TimeSpan.Zero)
            return;

        var timer = dispatcher.CreateTimer();
        timer.Interval = clearAfter;
        timer.IsRepeating = false;
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            try
            {
                Clipboard.Flush();
            }
            catch
            {
                // Ignore clipboard failures (e.g. locked by another app).
            }
        };
        timer.Start();
    }
}
