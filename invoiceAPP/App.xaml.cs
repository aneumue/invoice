using System.Runtime.InteropServices;
using System.Windows;

namespace InvoiceAPP;

public partial class App : Application
{
    [DllImport("kernel32.dll")]
    private static extern bool AllocConsole();

    protected override void OnStartup(StartupEventArgs e)
    {
#if DEBUG
        AllocConsole();
#endif
        base.OnStartup(e);
    }
}
