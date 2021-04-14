using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using NATS.Client;

namespace EventsLogger
{
    public class EventsLogger
    {
        public static class Events
        {
            public static readonly string SimilarityCalculated = "valuator.processing.similarity_calculated";
            public static readonly string RankCalculated = "rank_calculator.processing.rank_calculated";
        }

        private readonly ILogger<EventsLogger> _logger;

        public EventsLogger(ILogger<EventsLogger> logger)
        {
            _logger = logger;
        }

        public void Log()
        {
            var options = ConnectionFactory.GetDefaultOptions();

            options.Servers = new[]
            {
                System.Environment.GetEnvironmentVariable("NATS_URL")
            };

            using (var connection = new ConnectionFactory().CreateConnection(options))
            {
                var rankCalculatedSubscription = connection.SubscribeAsync(Events.RankCalculated, (sender, args) =>
                {
                    LogMessage(args.Message.Data);
                });

                var similarityCalculatedSubscription = connection.SubscribeAsync(Events.SimilarityCalculated, (sender, args) =>
                {
                    LogMessage(args.Message.Data);
                });

                rankCalculatedSubscription.Start();
                similarityCalculatedSubscription.Start();

                Console.ReadLine();

                rankCalculatedSubscription.Unsubscribe();
                similarityCalculatedSubscription.Unsubscribe();

                connection.Drain();
                connection.Close();
            }
        }

        private void LogMessage(byte[] data)
        {
            var message = Encoding.UTF8.GetString(data);

            _logger.LogDebug(message);
        }
    }
}