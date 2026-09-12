using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using InvoiceAPP.Services;

namespace InvoiceAPP;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void StartPipeline_Click(
        object sender,
        RoutedEventArgs e)
    {
        StartButton.IsEnabled = false;
        StatusText.Text = "Processing invoices...";
        InvoiceList.ItemsSource = null;

        try
        {
            var invoices = await Task.Run(() =>
            {
                var pipe = new StartPipe();
                return pipe.Run();
            });

            InvoiceList.ItemsSource = invoices.Select(invoice =>
                $"{Path.GetFileName(invoice.FilePath)} | " +
                $"{invoice.Company ?? "Unknown"} | " +
                $"{invoice.Status}"
            );

            StatusText.Text =
                $"Finished. {invoices.Count} invoice(s) processed.";
        }
        catch (Exception exception)
        {
            StatusText.Text = $"Error: {exception.Message}";
        }
        finally
        {
            StartButton.IsEnabled = true;
        }
    }
}