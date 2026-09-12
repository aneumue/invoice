using InvoiceAPP.Services;

Console.WriteLine("Hello, World!");

var pipe = new StartPipe();
var invoices = pipe.Run();

Console.WriteLine(invoices.Company);

