using System;
using TicketsDataAggregator.TicketDataAggregation;

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
