using System;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using NATS.Client;
using RepositoryLibrary;

namespace Valuator.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IRepository _repository;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IRepository repository, ILogger<IndexModel> logger)
        {
            _repository = repository;

            _logger = logger;
        }

        public void OnGet()
        {

        }

        public IActionResult OnPost(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new NoContentResult();
            }

            _logger.LogDebug(text);

            string id = Guid.NewGuid().ToString();

            string textPrefix = "TEXT-";
            string textKey = textPrefix + id;
            _repository.Save(textKey, text);

            string similarityKey = "SIMILARITY-" + id;

            var similarity = AnalyzeSimilarity(textPrefix, text);

            _repository.Save(similarityKey, similarity.ToString());

            CalculateRank(id);

            LogSimilarity(id, similarity);

            return Redirect($"summary?id={id}");
        }

        private int AnalyzeSimilarity(string prefix, string text)
        {
            var texts = _repository.GetAllByPrefix(prefix);
            var duplicatesCount = texts.Where(t => t == text).Count() - 1;

            return duplicatesCount != 0 ? 1 : 0;
        }

        private void CalculateRank(string id)
        {
            var options = ConnectionFactory.GetDefaultOptions();

            options.Servers = new[]
            {
                System.Environment.GetEnvironmentVariable("NATS_URL")
            };

            using (var connection = new ConnectionFactory().CreateConnection(options)) 
            {
                connection.Publish("valuator.processing.rank", Encoding.UTF8.GetBytes(id));

                connection.Drain();
                connection.Close();
            }
        }

        private void LogSimilarity(string id, int similarity)
        {
            var options = ConnectionFactory.GetDefaultOptions();

            options.Servers = new[]
            {
                System.Environment.GetEnvironmentVariable("NATS_URL")
            };

            using (var connection = new ConnectionFactory().CreateConnection(options))
            {
                var message = $"Event: SimilarityCalculated, context id: {id}, similarity: {similarity}";

                byte[] data = Encoding.UTF8.GetBytes(message);

                connection.Publish("valuator.processing.similarity_calculated", data);
                
                connection.Drain();
                connection.Close();
            }
        }
    }
}
