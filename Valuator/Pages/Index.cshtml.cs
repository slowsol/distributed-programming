using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Valuator.Data.Repositories;

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

            string rankKey = "RANK-" + id;
            _repository.Save(rankKey, AnalyzeRank(text).ToString());

            string similarityKey = "SIMILARITY-" + id;
            _repository.Save(similarityKey, AnalyzeSimilarity(textPrefix, text).ToString());

            return Redirect($"summary?id={id}");
        }

        private double AnalyzeRank(string text)
        {
            return (double)text.Count(c => !char.IsLetter(c)) / text.Length;
        }

        private double AnalyzeSimilarity(string prefix, string text)
        {
            var texts = _repository.GetAllByPrefix(prefix);
            var duplicatesCount = texts.Where(t => t == text).Count() - 1;

            return duplicatesCount != 0 ? 1 : 0;
        }
    }
}
