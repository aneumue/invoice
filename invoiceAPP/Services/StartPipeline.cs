using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using InvoiceAPP.Models;

namespace InvoiceAPP.Services;

public class StartPipe
{
    public List<InvoiceMetadata> Run()
    {
        var pdfExtractor = new PdfTextExtractor();
        using var ocrService = new OcrService();
        var textFinder = new TextFinder();
        var msgExtractor = new MsgAttachmentExtractor();

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

        string msgInput = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                Environment.ExpandEnvironmentVariables(
                    config.Paths.MsgInput
                )
            )
        );

        string pdfOutput = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                Environment.ExpandEnvironmentVariables(
                    config.Paths.PdfOutput
                )
            )
        );

        string msgArchive = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                Environment.ExpandEnvironmentVariables(
                    config.Paths.MsgArchive
                )
            )
        );

        Directory.CreateDirectory(msgInput);
        Directory.CreateDirectory(pdfOutput);
        Directory.CreateDirectory(msgArchive);

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

        foreach (string pdfPath in filePaths)
        {
            var metadata = new InvoiceMetadata
            {
                FilePath = pdfPath,
                ProcessedWith = "DIGITAL"
            };

            string text = pdfExtractor.ExtractFirstPage(pdfPath);

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