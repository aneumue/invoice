using System.Collections.Generic;
namespace InvoiceAPP.Services;

public class TextFinder
{
    public string NormalizeText(string text)
    {
        return text
            .Replace(" ", "")
            .Replace("-", "")
            .Replace("\n", "")
            .Replace("\r", "")
            .ToUpperInvariant();
    }

    public bool FindText(string pdfText, string expectedText)
    {      
        return NormalizeText(pdfText)
            .Contains(NormalizeText(expectedText));
    }


    public string? FindCompany(string pdfText, Dictionary<string, List<string>> companies)
    {

        var normalizedCompanies = companies.ToDictionary(
            company => company.Key,
            company => company.Value
                .Select(NormalizeText)
                .ToList()
        );

        string normalizedText = NormalizeText(pdfText);
        
        foreach (var company in companies)
        {
            if (company.Value.Any(identifier =>
                FindText(normalizedText, identifier)))
            {
                return company.Key;
            }
        }

        return null;
    }


}




