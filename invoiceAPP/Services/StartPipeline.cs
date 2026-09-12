using InvoiceAPP.Models;
using System.Text.Json;
using System.Collections.Generic;
using System.IO;

namespace InvoiceAPP.Services;

public class StartPipe
{
    public List<InvoiceMetadata> Run()
    {
        var pdfExtractor = new PdfTextExtractor();
        using var ocrService = new OcrService();
        var textFinder = new TextFinder();
        var msgExtractor = new MsgAttachmentExtractor();

        string jsonFilePath = Environment.ExpandEnvironmentVariables(
            @"%USERPROFILE%\Desktop\invoice\companyIdents.json"
        );

        string json = File.ReadAllText(jsonFilePath);

        var config = JsonSerializer.Deserialize<AppConfig>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        ) ?? throw new Exception("Could not load configuration.");

        string msgInput = Environment.ExpandEnvironmentVariables(
            config.Paths.MsgInput
        );

        string pdfOutput = Environment.ExpandEnvironmentVariables(
            config.Paths.PdfOutput
        );

        string msgArchive = Environment.ExpandEnvironmentVariables(
            config.Paths.MsgArchive
        );

        msgExtractor.ExtractPdfs(
            msgInput,
            pdfOutput,
            msgArchive
        );

        string[] filePaths = Directory.GetFiles(
            pdfOutput,
            "*.pdf",
            SearchOption.TopDirectoryOnly
        );


        var invoices = new List<InvoiceMetadata>();

        foreach (var pdfPath in filePaths)
        {
            var metadata = new InvoiceMetadata
            {
                FilePath = pdfPath
            };

            string text = pdfExtractor.ExtractFirstPage(pdfPath);

            metadata.ProcessedWith = "DIGITAL";

            if (string.IsNullOrWhiteSpace(text))
            {
                metadata.ProcessedWith = "OCR";
                text = ocrService.ScanFirstPage(pdfPath);
            }

            string? company = textFinder.FindCompany(
                text,
                config.Companies
            );

            if (string.IsNullOrWhiteSpace(company))
            {
                metadata.Company = null;
                metadata.Status = "QUARANTINE";
            }
            else
            {
                metadata.Company = company;
                metadata.Status = "READY";
            }

            invoices.Add(metadata);
        }

        return invoices;
    }
}


