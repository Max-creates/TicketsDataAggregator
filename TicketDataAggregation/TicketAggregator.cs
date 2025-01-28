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
    private readonly IFileWriter _fileWriter;
    private readonly IDocumentsReader _documentsReader;

    public TicketAggregator(
        string ticketsFolder,
        IFileWriter fileWriter,
        IDocumentsReader documentsReader)
    {
        _ticketsFolder = ticketsFolder;
        _fileWriter = fileWriter;
        _documentsReader = documentsReader;
    }

    public void Run()
    {
        var stringBuilder = new StringBuilder();

        var ticketDocuments = _documentsReader.Read(
            _ticketsFolder);

        foreach (var document in ticketDocuments)
        {
            var lines = ProcessDocument(document);
            stringBuilder.AppendLine(
                string.Join(Environment.NewLine, lines));
        }


        _fileWriter.Write(
            stringBuilder.ToString(), 
            _ticketsFolder, "aggregatedTickets.txt");
    }



    private IEnumerable<string> ProcessDocument(
        string document)
    {
        var split = document.Split(
            new[] { "Title:", "Date:", "Time:", "Visit us:" },
            StringSplitOptions.None);

        var domain = split.Last().ExtractDomain();
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
}

public interface IDocumentsReader
{
    IEnumerable<string> Read(string directory);
}

public class DocumentsFromPdfReader : IDocumentsReader
{
    public IEnumerable<string> Read(string directory)
    {
        foreach (var filePath in Directory.GetFiles(
            directory, "*.pdf"))
        {
            using PdfDocument document = PdfDocument.Open(filePath);
            // Page number starts from 1, not 0.
            Page page = document.GetPage(1);
            yield return page.Text;
        }
    }
}

public interface IFileWriter
{
    void Write(
        string content, params string[] pathParts);
}

public class FileWriter : IFileWriter
{
    public void Write(string content, params string[] pathParts)
    {
        var resultPath = Path.Combine(pathParts);
        File.WriteAllText(resultPath, content);
        Console.WriteLine("Results saved to " + resultPath);
    }
}

public static class  WebAddressExtension
{
        public static string ExtractDomain(
            this string webAddress)
    {
        var lastDotIndex = webAddress.LastIndexOf('.');
        return webAddress.Substring(lastDotIndex);
    }
}