using Microsoft.Extensions.Logging;

namespace EventsLogger
{
    class Program
    {
        static void Main(string[] args)
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

            var eventsLogger = new EventsLogger(loggerFactory.CreateLogger<EventsLogger>());

            eventsLogger.Log();
        }
    }
}
