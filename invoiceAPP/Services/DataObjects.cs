namespace InvoiceAPP.Services;

public class InvoiceMetadata
{
    public string FilePath { get; set; } = "";
    public string? Company { get; set; }
    public string? InvoiceType { get; set; }
    public string? ProcessedWith { get; set; }
    public string? Status { get; set; } = "UNPROCESSED";
}


