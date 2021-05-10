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

            var region = _repository.GetRegionName(id);

            _logger.LogDebug("LOOKUP: {textId}, {region}", id, region);

            var text = _repository.Get("TEXT-" + id, id);

            if (string.IsNullOrEmpty(text)) { return; }

            Similarity = double.Parse(_repository.Get("SIMILARITY-" + id, id));

            Rank = double.Parse(_repository.Get("RANK-" + id, id));
        }
    }
}
