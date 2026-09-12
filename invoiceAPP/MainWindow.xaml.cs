using System;
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

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        StartButton.IsEnabled = false;
        StatusText.Text = "Running...";

        try
        {
            var invoices = await Task.Run(() => new StartPipe().Run());
            
            foreach (var invoice in invoices)
            {
              Console.WriteLine(invoice.FilePath);  
            };

            ResultsGrid.ItemsSource = invoices;
            StatusText.Text = $"Done. {invoices.Count} invoice(s) processed.";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error: {ex.Message}";
            MessageBox.Show(
                ex.Message,
                "Pipeline failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            StartButton.IsEnabled = true;
        }
    }
}
