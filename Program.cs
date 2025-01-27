using UglyToad.PdfPig.Content;
using UglyToad.PdfPig;
using System;
using System.Globalization;

const string TicketsFolder = @"C:\Users\Max\source\repos\TicketsDataAggregator\Tickets";

try
{
    var ticketAggregator = new TicketAggregator(
        TicketsFolder);

    ticketAggregator.Run();
}
catch (Exception ex)
{
    Console.WriteLine("An exception occured. " + 
        "Exception message: " + ex.Message);
}

Console.WriteLine("Press any key to quit...");
Console.ReadKey();

public class TicketAggregator
{
    private readonly string _ticketsFolder;
    private readonly Dictionary<string, string> _domainToCultureMapping = new()
    {
        [".com"] = "en-US",
        [".co.uk"] = "en-GB",
        [".de"] = "de-DE",
        [".fr"] = "fr-FR",
        [".es"] = "es-ES",
        [".it"] = "it-IT",
        [".nl"] = "nl-NL",
        [".pl"] = "pl-PL",
        [".ru"] = "ru-RU",
        [".jp"] = "ja-JP",
    };

    public TicketAggregator(string ticketsFolder)
    {
        _ticketsFolder = ticketsFolder;
    }

    public void Run()
    {
        foreach (var filePath in Directory.GetFiles(
            _ticketsFolder, "*.pdf"))
        {
            using PdfDocument document = PdfDocument.Open(filePath);
            // Page number starts from 1, not 0.
            Page page = document.GetPage(1);
            string text = page.Text;
            var split = text.Split(
                new[] { "Title:", "Date:", "Time:", "Visit us:" },
                StringSplitOptions.None);

            var domain = ExtractDomain(split.Last());
            var ticketsCulture = _domainToCultureMapping[domain];

            for (int i = 1; i < split.Length - 3; i += 3)
            {
                var title = split[i];
                var dateAsString = split[i + 1];
                var timeAsString = split[i + 2];

                var date = DateOnly.Parse(
                    dateAsString, new CultureInfo(ticketsCulture));
                var time = TimeOnly.Parse(
                    timeAsString, new CultureInfo(ticketsCulture));
            }
        }
    }

    private string ExtractDomain(string webAddress)
    {
        var lastDotIndex = webAddress.LastIndexOf('.');
        return webAddress.Substring(lastDotIndex);
    }
}
