using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using Azure;
using System;
using System.Globalization;
using Azure.AI.TextAnalytics;

namespace WebApplication1.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    static string languageKey = Environment.GetEnvironmentVariable("LANGUAGE_KEY");
    static string languageEndpoint = Environment.GetEnvironmentVariable("LANGUAGE_ENDPOINT");

    private static readonly AzureKeyCredential credentials = new AzureKeyCredential(languageKey);
    private static readonly Uri endpoint = new Uri(languageEndpoint);


    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpPost]
    public ActionResult Index(string inputText)
    {
        if (!string.IsNullOrEmpty(inputText))
        {
            ViewBag.Result = AnalyzeText(inputText);
            ViewBag.Text = inputText;
        }
        return View();
    }

    public Response<LinkedEntityCollection> AnalyzeText(string text)
    {
        var client = new TextAnalyticsClient(endpoint, credentials);
        var response = client.RecognizeLinkedEntities(text);
        return response;
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
