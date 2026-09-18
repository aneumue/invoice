# invoice

dotnet add package MsgReader
dotnet add package OpenCvSharp4.Windows
dotnet add package PDFtoImage
dotnet add package PdfPig

dotnet add package Sdcb.PaddleOCR --version 3.3.1
dotnet add package Sdcb.PaddleOCR.Models.Local --version 3.3.1
dotnet add package Sdcb.PaddleOCR.Models.LocalV5 --version 3.3.1

dotnet add package Sdcb.PaddleInference --version 3.3.1
dotnet add package Sdcb.PaddleInference.runtime.win64.mkl --version 3.3.1

dotnet publish -c Release -r win-x64 --self-contained true -o .\publish

