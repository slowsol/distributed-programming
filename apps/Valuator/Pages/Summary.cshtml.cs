using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using RepositoryLibrary;

namespace Valuator.Pages
{
    public class SummaryModel : PageModel
    {
        private readonly IRepository _repository;

        private readonly ILogger<SummaryModel> _logger;

        public SummaryModel(IRepository repository, ILogger<SummaryModel> logger)
        {
            _repository = repository;

            _logger = logger;
        }

        public double Rank { get; set; }
        public double Similarity { get; set; }

        public void OnGet(string id)
        {
            _logger.LogDebug(id);

            var text = _repository.Get("TEXT-" + id);

            if (string.IsNullOrEmpty(text)) { return; }

            Rank = double.Parse(_repository.Get("RANK-" + id));

            Similarity = double.Parse(_repository.Get("SIMILARITY-" + id));
        }
    }
}
