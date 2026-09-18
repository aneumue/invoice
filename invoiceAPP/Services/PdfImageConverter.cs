using PDFtoImage;
using System.IO;

namespace InvoiceAPP.Services;

public class PdfImageConverter
{
    public byte[] ConvertPage(string pdfPath, int page = 0, int dpi = 150)
    {
        using var pdfStream = File.OpenRead(pdfPath);
        using var imageStream = new MemoryStream();

        Conversion.SavePng(
            imageStream,
            pdfStream,
            page: page,
            options: new RenderOptions
            {
                Dpi = dpi
            }
        );

        return imageStream.ToArray();
    }
}