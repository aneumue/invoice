using InvoiceAPP.Services;

Console.WriteLine("Hello, World!");

var pipe = new StartPipe();
var invoices = pipe.Run();

foreach (var inv in invoices)
{
    Console.WriteLine($"{inv.FilePath}: {inv.Company} [{inv.Status}] ({inv.ProcessedWith})");
}

Console.WriteLine($"Done: {invoices.Count} invoices.");

