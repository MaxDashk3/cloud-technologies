using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    public class PIIController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        static string languageKey = Environment.GetEnvironmentVariable("LANGUAGE_KEY");
        static string languageEndpoint = Environment.GetEnvironmentVariable("LANGUAGE_ENDPOINT");

        private static readonly AzureKeyCredential credentials = new AzureKeyCredential(languageKey);
        private static readonly Uri endpoint = new Uri(languageEndpoint);

        static PiiEntityCollection RecognizePII(TextAnalyticsClient client, string text)
        {
            return client.RecognizePiiEntities(text).Value;
        }


        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string inputText)
        {
            var client = new TextAnalyticsClient(endpoint, credentials);
            var entities = RecognizePII(client, inputText);

            HashSet<string> recognizedCategories = new HashSet<string>();
            foreach (var entity in entities)
            {
                recognizedCategories.Add(entity.Category.ToString());
            }

            
            ViewBag.entities = entities;
            ViewBag.text = inputText;
            ViewBag.categories = recognizedCategories.ToArray();
            return View();
        }
    }
}
