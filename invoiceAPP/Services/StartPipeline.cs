using InvoiceAPP.Models;
using System.Text.Json;
using System.Collections.Generic;

namespace InvoiceAPP.Services;

public class StartPipe
{
    public List<InvoiceMetadata> Run()
    {
        var pdfExtractor = new PdfTextExtractor();
        using var ocrService = new OcrService();
        var textFinder = new TextFinder();
        var msgExtractor = new MsgAttachmentExtractor();

        string jsonFilePath =
            "C:\\Users\\andym\\Desktop\\invoice\\companyIdents.json";

        string json = File.ReadAllText(jsonFilePath);

        var config = JsonSerializer.Deserialize<AppConfig>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        ) ?? throw new Exception("Could not load configuration.");

        msgExtractor.ExtractPdfs(
            config.Paths.MsgInput,
            config.Paths.PdfOutput,
            config.Paths.MsgArchive
        );

        string[] filePaths = Directory.GetFiles(config.Paths.PdfOutput, "*.pdf", 
                                                SearchOption.TopDirectoryOnly);


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


