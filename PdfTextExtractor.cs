using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace InvoiceAPP.Services;

public class PdfTextExtractor
{
    public string ExtractFirstPage(string pdfPath)
    {
        using PdfDocument document = PdfDocument.Open(pdfPath);

        Page page = document.GetPage(1);

        return ContentOrderTextExtractor.GetText(page);
    }
}

