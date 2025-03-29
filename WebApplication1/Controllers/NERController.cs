using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class NERController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        static string languageKey = Environment.GetEnvironmentVariable("LANGUAGE_KEY");
        static string languageEndpoint = Environment.GetEnvironmentVariable("LANGUAGE_ENDPOINT");

        private static readonly AzureKeyCredential credentials = new AzureKeyCredential(languageKey);
        private static readonly Uri endpoint = new Uri(languageEndpoint);
        
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        public Response<CategorizedEntityCollection> RecognizeEntities(string text)
        {
            var client = new TextAnalyticsClient(endpoint, credentials);
            return client.RecognizeEntities(text);
        }

        public List<EntityModel> GetRandomEntities(CategorizedEntityCollection entities)
        {
            int amount = (int)(entities.Count * 0.5);
            var randEntities = new List<EntityModel>();

            var random = new Random();
            HashSet<int> indexes = new HashSet<int>();
            while (indexes.Count != amount) {
                int randomNumber = random.Next(0, entities.Count);
                indexes.Add(randomNumber);
            }

            var orderedIndexes = indexes.OrderByDescending(n => n);

            foreach (int i in orderedIndexes)
            {
                randEntities.Add(new EntityModel(entities[i]));
            }

            return randEntities;
        }

        [HttpPost]
        public IActionResult Index(string inputText)
        {
            if (!string.IsNullOrEmpty(inputText))
            {
                var recognizedEntities = RecognizeEntities(inputText).Value;
                var model = new FormModel(inputText, recognizedEntities, GetRandomEntities(recognizedEntities));
                return View(model);
            }
            return View();
        }

        [HttpPost]
        public IActionResult Result(FormModel model)
        {
            model.answers.Reverse();

            int correctAnswerCount = 0;

            for (int i = 0; i < model.randomEntities.Count; i++)
            {
                if (model.randomEntities[i].Text == model.answers[i])
                {
                    correctAnswerCount++;
                }
            }

            ViewBag.CorrectAnswerCount = correctAnswerCount;

            return View(model);
        }
    }
}
