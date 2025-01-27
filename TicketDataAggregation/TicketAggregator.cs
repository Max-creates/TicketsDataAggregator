using UglyToad.PdfPig.Content;
using UglyToad.PdfPig;
using System.Globalization;
using System.Text;

namespace TicketsDataAggregator.TicketDataAggregation;


public class TicketAggregator
{
    private readonly string _ticketsFolder;
    private readonly Dictionary<string, CultureInfo> _domainToCultureMapping = new()
    {
        [".com"] = new CultureInfo("en-US"),
        [".co.uk"] = new CultureInfo("en-GB"),
        [".de"] = new CultureInfo("de-DE"),
        [".fr"] = new CultureInfo("fr-FR"),
        [".es"] = new CultureInfo("es-ES"),
        [".it"] = new CultureInfo("it-IT"),
        [".nl"] = new CultureInfo("nl-NL"),
        [".pl"] = new CultureInfo("pl-PL"),
        [".ru"] = new CultureInfo("ru-RU"),
        [".jp"] = new CultureInfo("ja-JP"),
    };

    public TicketAggregator(string ticketsFolder)
    {
        _ticketsFolder = ticketsFolder;
    }

    public void Run()
    {
        var stringBuilder = new StringBuilder();

        foreach (var filePath in Directory.GetFiles(
            _ticketsFolder, "*.pdf"))
        {
            using PdfDocument document = PdfDocument.Open(filePath);
            // Page number starts from 1, not 0.
            Page page = document.GetPage(1);
            var lines = ProcessPage(page);
            stringBuilder.AppendLine(
                string.Join(Environment.NewLine, lines));
        }

        SaveTicketsData(stringBuilder);
    }



    private IEnumerable<string> ProcessPage(Page page)
    {
        string text = page.Text;
        var split = text.Split(
            new[] { "Title:", "Date:", "Time:", "Visit us:" },
            StringSplitOptions.None);

        var domain = ExtractDomain(split.Last());
        var ticketsCulture = _domainToCultureMapping[domain];

        for (int i = 1; i < split.Length - 3; i += 3)
        {
            yield return BuildTicketData(
                split, i, ticketsCulture);
        }
    }

    private static string BuildTicketData(
        string[] split,
        int i,
        CultureInfo ticketsCulture)
    {
        var title = split[i];
        var dateAsString = split[i + 1];
        var timeAsString = split[i + 2];

        var date = DateOnly.Parse(
            dateAsString, ticketsCulture);
        var time = TimeOnly.Parse(
            timeAsString, ticketsCulture);

        var dateAsStringInvariant = date
            .ToString(CultureInfo.InvariantCulture);
        var timeAsStringInvariant = time
            .ToString(CultureInfo.InvariantCulture);

        var ticketData =
            $"{title,-40}|{dateAsStringInvariant}|{timeAsStringInvariant}";
        return ticketData;
    }

    private void SaveTicketsData(StringBuilder stringBuilder)
    {
        var resultPath = Path.Combine(
                    _ticketsFolder, "aggregatedTickets.txt");
        File.WriteAllText(resultPath, stringBuilder.ToString());
        Console.WriteLine("Results saved to " + resultPath);
    }

    private static string ExtractDomain(string webAddress)
    {
        var lastDotIndex = webAddress.LastIndexOf('.');
        return webAddress.Substring(lastDotIndex);
    }
}