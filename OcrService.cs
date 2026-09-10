using OpenCvSharp;
using PDFtoImage;
using Sdcb.PaddleInference;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models;
using Sdcb.PaddleOCR.Models.Local;
using System;

namespace InvoiceAPP.Services;

public class OcrService : IDisposable
{
    private readonly PaddleOcrAll _ocr;

    public OcrService()
    {
        EnsureSupportedPlatform();

        FullOcrModel model = LocalFullModels.LatinV5;

        _ocr = new PaddleOcrAll(
            model,
            PaddleDevice.Mkldnn())
        {
            AllowRotateDetection = false,
            Enable180Classification = false,
        };

        _ocr.Detector.MaxSize = 1536;
    }

    public string ScanFirstPage(string pdfPath, double topPercent = 0.5)
    {
        EnsureSupportedPlatform();

        byte[] imageBytes;


        using (var pdfStream = File.OpenRead(pdfPath))
        using (var memoryStream = new MemoryStream())
        {
            if (OperatingSystem.IsLinux() || OperatingSystem.IsWindows())
            {
                Conversion.SavePng(
                    memoryStream,
                    pdfStream,
                    options: new RenderOptions
                    {
                        Dpi = 200
                    }
                );
            }
            else
            {
                throw new PlatformNotSupportedException(
                    "PDF rendering is supported only on Linux and Windows.");
            }

            imageBytes = memoryStream.ToArray();
        }



        using Mat fullImage = Cv2.ImDecode(
            imageBytes,
            ImreadModes.Color
        );


        int cropHeight = (int)(fullImage.Height * topPercent);

        var cropArea = new Rect(
            0,
            0,
            fullImage.Width,
            cropHeight
        );

        using Mat croppedImage = new Mat(
            fullImage,
            cropArea
        );

        Console.WriteLine(
            $"OCR area: {croppedImage.Width}x{croppedImage.Height} " +
            $"({topPercent:P0} of page)"
        );


        PaddleOcrResult result = _ocr.Run(croppedImage);


        return result.Text;
    }

    private static void EnsureSupportedPlatform()
    {
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException(
                "OCR is supported only on Linux x64 and Windows x64.");
        }
    }

    public void Dispose()
    {
        _ocr.Dispose();
    }
}
