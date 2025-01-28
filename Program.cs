using TicketsDataAggregator.TicketsAggregation;
using TicketsDataAggregator.FileAccess;

const string TicketsFolder = @"C:\Users\Max\source\repos\TicketsDataAggregator\Tickets";

try
{
    var ticketAggregator = new TicketsAggregator(
        TicketsFolder,
        new FileWriter(),
        new DocumentsFromPdfReader());

    ticketAggregator.Run();
}
catch (Exception ex)
{
    Console.WriteLine("An exception occured. " + 
        "Exception message: " + ex.Message);
}

Console.WriteLine("Press any key to quit...");
Console.ReadKey();
