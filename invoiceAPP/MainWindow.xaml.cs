using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using InvoiceAPP.Models;
using InvoiceAPP.Services;

namespace InvoiceAPP;

public partial class MainWindow : Window
{
    private readonly string _inputFolder;

    public MainWindow()
    {
        InitializeComponent();

        string jsonFilePath = Path.Combine(
            AppContext.BaseDirectory,
            "companyIdents.json"
        );

        string json = File.ReadAllText(jsonFilePath);

        var config = JsonSerializer.Deserialize<AppConfig>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        ) ?? throw new Exception(
            "Could not load companyIdents.json."
        );

        var companyOptions = new List<string> { "" };
        companyOptions.AddRange(config.Companies.Keys);

        CompanySelector.ItemsSource = companyOptions;
        CompanySelector.SelectedIndex = 0;

        string configuredInputFolder =
            Environment.ExpandEnvironmentVariables(
                config.Paths.PdfOutput
            );

        _inputFolder = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                configuredInputFolder
            )
        );
    }

    private async void StartPipeline_Click(
        object sender,
        RoutedEventArgs e)
    {
        StartButton.IsEnabled = false;
        InvoiceTable.ItemsSource = null;
        InvoiceImage.Source = null;

        try
        {
            var invoices = await Task.Run(() =>
            {
                var pipe = new StartPipe();
                return pipe.Run();
            });

            InvoiceTable.ItemsSource = invoices;

            if (invoices.Count == 0)
            {
                MessageBox.Show("No invoices found.");
                return;
            }

            InvoiceTable.SelectedIndex = 0;
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                exception.Message,
                "Pipeline error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
        finally
        {
            StartButton.IsEnabled = true;
        }
    }

    private async void InvoiceTable_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (InvoiceTable.SelectedItem is not InvoiceMetadata invoice)
        {
            return;
        }

        CompanySelector.SelectedItem = invoice.Company ?? "";

        byte[] imageBytes = await Task.Run(() =>
            new PdfImageConverter().ConvertPage(invoice.FilePath)
        );

        using var stream = new MemoryStream(imageBytes);

        InvoiceImage.Source = BitmapFrame.Create(
            stream,
            BitmapCreateOptions.None,
            BitmapCacheOption.OnLoad
        );
    }

    private void SetInvoiceType_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (InvoiceTable.SelectedItem is not InvoiceMetadata invoice)
        {
            return;
        }

        var button = (Button)sender;
        int currentIndex = InvoiceTable.SelectedIndex;

        invoice.InvoiceType = (string)button.Tag;

        InvoiceTable.Items.Refresh();

        if (currentIndex + 1 < InvoiceTable.Items.Count)
        {
            InvoiceTable.SelectedIndex = currentIndex + 1;

            InvoiceTable.ScrollIntoView(
                InvoiceTable.SelectedItem
            );
        }
    }

    private void EditCompany_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (InvoiceTable.SelectedItem is not InvoiceMetadata invoice)
        {
            return;
        }

        string? company = CompanySelector.SelectedItem as string;

        invoice.Company = string.IsNullOrWhiteSpace(company)
            ? null
            : company;

        InvoiceTable.Items.Refresh();
    }

    private void MoveInvoices_Click(
        object sender,
        RoutedEventArgs e)
    {
        foreach (InvoiceMetadata invoice in InvoiceTable.Items)
        {
            if (string.IsNullOrWhiteSpace(invoice.Company) ||
                string.IsNullOrWhiteSpace(invoice.InvoiceType) ||
                invoice.Status == "MOVED")
            {
                continue;
            }

            string company = invoice.Company;
            string invoiceType = invoice.InvoiceType;

            string targetFolder = Path.Combine(
                _inputFolder,
                company,
                invoiceType
            );

            Directory.CreateDirectory(targetFolder);

            string uniqueFileName =
                $"{Path.GetFileNameWithoutExtension(invoice.FilePath)}_" +
                $"{Guid.NewGuid():N}" +
                $"{Path.GetExtension(invoice.FilePath)}";

            string targetPath = Path.Combine(
                targetFolder,
                uniqueFileName
            );

            File.Move(invoice.FilePath, targetPath);

            invoice.FilePath = targetPath;
            invoice.Status = "MOVED";
        }

        InvoiceTable.Items.Refresh();
    }
}