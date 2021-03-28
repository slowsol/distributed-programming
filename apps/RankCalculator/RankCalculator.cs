using System;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using RepositoryLibrary;
using NATS.Client;

namespace RankCalculator
{
    public class RankCalculator
    {
        private readonly ILogger<RankCalculator> _logger;

        private readonly IRepository _repository;

        public RankCalculator(ILogger<RankCalculator> logger, IRepository repository)
        {
            _logger = logger;

            _repository = repository;
        }

        public void Calculate()
        {
            var options = ConnectionFactory.GetDefaultOptions();

            options.Servers = new[]
            {
                System.Environment.GetEnvironmentVariable("NATS_URL")
            };

            using (var connection = new ConnectionFactory().CreateConnection(options)) 
            {
                var subscription = connection.SubscribeAsync("valuator.processing.rank", "rank_calculator", (sender, args) =>
                {
                    string id = Encoding.UTF8.GetString(args.Message.Data);
                    string textKey = "TEXT-" + id;

                    if (!_repository.IsKeyExist(textKey))
                    {
                        _logger.LogWarning("Text key {textKey} doesn't exist", textKey);

                        return;
                    }

                    string text = _repository.Get(textKey);

                    string rankKey = "RANK-" + id;
                    string rank = AnalyzeRank(text).ToString();

                    _logger.LogDebug("Rank {rank} with key {rankKey} by text id {id}", rank, rankKey, id);

                    _repository.Save(rankKey, rank);
                });

                subscription.Start();

                Console.ReadLine();

                subscription.Unsubscribe();

                connection.Drain();
                connection.Close();
            }
        }

        private double AnalyzeRank(string text)
        {
            return (double)text.Count(c => !char.IsLetter(c)) / text.Length;
        }
    }
}
