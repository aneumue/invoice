using System.Collections.Generic;

namespace InvoiceAPP.Models;

public class AppConfig
{
    public PathsConfig Paths { get; set; } = new();

    public Dictionary<string, List<string>> Companies { get; set; } = new();
}

public class PathsConfig
{
    public string MsgInput { get; set; } = "";
    public string PdfOutput { get; set; } = "";
    public string MsgArchive { get; set; } = "";
}