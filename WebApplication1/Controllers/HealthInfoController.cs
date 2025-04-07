using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
    public class HealthInfoController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        static string languageKey = Environment.GetEnvironmentVariable("LANGUAGE_KEY");
        static string languageEndpoint = Environment.GetEnvironmentVariable("LANGUAGE_ENDPOINT");

        private static readonly AzureKeyCredential credentials = new AzureKeyCredential(languageKey);
        private static readonly Uri endpoint = new Uri(languageEndpoint);


        static AsyncPageable<AnalyzeHealthcareEntitiesResultCollection> HealthInfo(TextAnalyticsClient client, string document)
        {
            List<string> batchInput = new List<string>()
            {
                document
            };
            AnalyzeHealthcareEntitiesOperation healthOperation = client.AnalyzeHealthcareEntities(0, batchInput);

            
            return healthOperation.Value;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string inputText)
        {
            var client = new TextAnalyticsClient(endpoint, credentials);
            var result = HealthInfo(client, inputText);

            List<HealthcareEntity> entityList = new List<HealthcareEntity>();

            await foreach (AnalyzeHealthcareEntitiesResultCollection documentsInPage in result)
            {

                foreach (AnalyzeHealthcareEntitiesResult entitiesInDoc in documentsInPage)
                {
                    if (!entitiesInDoc.HasError)
                    {
                        foreach (var entity in entitiesInDoc.Entities)
                        {
                            entityList.Add(entity);
                        }            
                    }
                }
            }

            ViewBag.entities = entityList;
            ViewBag.text = inputText;
            return View();
        }
    }
}
