using Azure;
using Azure.AI.Translation.Text;
using Microsoft.AspNetCore.Mvc;
using System.Security.Policy;
using System.Text.Json;
using System.Text.RegularExpressions;
using WebApplication1.Models;


namespace WebApplication1.Controllers
{
    public class TranslatorController : Controller
    {
        private readonly IConfiguration _config;
        private readonly string _translatorKey;
        private readonly TextTranslationClient client;
        private static Dictionary<string, string> languages;

        public TranslatorController(IConfiguration config)
        {
            _config = config;
            _translatorKey = _config["TranslatorKey"];
            AzureKeyCredential credential = new(_translatorKey);
            client = new(credential);
        }

        private TranslatorModel TranslateText(string inputText, string targetLanguage)
        {
            var response = client.Translate(targetLanguage, inputText);
            IReadOnlyList<TranslatedTextItem> translations = response.Value;
            TranslatedTextItem translation = translations.FirstOrDefault();

            return new TranslatorModel(inputText, translation?.Translations?.FirstOrDefault()?.Text, 
                translation?.DetectedLanguage?.Language, translation?.Translations?.FirstOrDefault()?.To);
        }

        static async Task GetLanguages()
        {
            using (HttpClient client = new HttpClient())
            {
                string endpoint = "https://api.cognitive.microsofttranslator.com/languages?api-version=3.0&scope=translation";

                var response = await client.GetStringAsync(endpoint);
                var jsonDoc = JsonDocument.Parse(response).RootElement;
                var dictionary = new Dictionary<string, string>();
                foreach (JsonProperty property in jsonDoc.GetProperty("translation").EnumerateObject())
                {
                    dictionary.Add(property.Name, property.Value.GetProperty("name").GetString());
                }
                
                languages = dictionary;
                Console.WriteLine($"{languages.Count} languages fetched successfully via {endpoint}.");
            }
        }

        public async Task<IActionResult> IndexAsync()
        {
            await GetLanguages();
            ViewBag.languages = languages;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> IndexAsync(string inputText, string targetLanguage)
        {
            if (languages == null) await GetLanguages();
            ViewBag.languages = languages;
            var translatedText = TranslateText(inputText, targetLanguage);
            return View(translatedText);
        }

        public async Task<IActionResult> TestCreator()
        {
            if (languages == null) await GetLanguages();
            ViewBag.languages = languages;
            return View();
        }

        public async Task<IActionResult> Test(List<string> inputFields, string targetLanguage)
        {
            var correctAnswers = new List<string>();
            for (int i=0; i < inputFields.Count; i++)
            {
                correctAnswers.Add(TranslateText(inputFields[i], targetLanguage).translatedText);
            }

            HttpContext.Session.SetString("correctAnswers", JsonSerializer.Serialize(correctAnswers));

            Random random = new Random();
            var testItems = new List<TranslatorTestItem>();
            for (int i = 0; i < inputFields.Count; i++)
            {
                var testItem = new TranslatorTestItem($"How will \"{inputFields[i]}\" be in {languages[targetLanguage]}?");
                if (inputFields.Count >= 4 && random.NextDouble() > 0.5)
                {
                    string correctAnswer = correctAnswers[i];
                    var answers = new List<string>(correctAnswers);
                    answers.RemoveAt(i);

                    for (int j = 0; j < inputFields.Count-3; j++)
                    {
                        answers.RemoveAt(random.Next(answers.Count));
                    }
                    answers.Add(correctAnswer);
                    answers = answers.OrderBy(_ => random.Next()).ToList();
                    testItem.AnswerOptions = answers;
                }
                testItems.Add(testItem);
            }
            HttpContext.Session.SetString("testItems", JsonSerializer.Serialize(testItems));
            return View(testItems);
        }

        public IActionResult Result(List<string> userAnswers)
        {
            int grade = 0;
            var correctAnswers = JsonSerializer.Deserialize<List<string>>(HttpContext.Session.GetString("correctAnswers"));
            var testItems = JsonSerializer.Deserialize<List<TranslatorTestItem>>(HttpContext.Session.GetString("testItems"));
            HttpContext.Session.Clear();
            for (int i = 0; i < userAnswers.Count; i++)
            {
                if (userAnswers[i] == correctAnswers[i])
                {
                    grade++;
                }
            }

            ViewBag.grade = grade;
            ViewBag.totalQuestions = userAnswers.Count;
            ViewBag.userAnswers = userAnswers;
            ViewBag.correctAnswers = correctAnswers;
            return View(testItems);
        }
    }

}