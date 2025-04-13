using Azure.AI.Vision.ImageAnalysis;
using Azure;
using Microsoft.AspNetCore.Mvc;
using Azure.AI.Vision.Face;
using Azure.AI.TextAnalytics;
using System.Drawing.Imaging;
using System.Drawing;

namespace WebApplication1.Controllers
{
    public class ImageAnalysisController : Controller
    {
        private readonly IConfiguration _config;
        private readonly string _key;
        private readonly string _endpoint;
        private readonly ImageAnalysisClient imgAnalysisClient;
        private static Dictionary<string, string> languages;

        public ImageAnalysisController(IConfiguration config)
        {
            _config = config;
            _key = _config["AIComputerVision:Key"];
            _endpoint = _config["AIComputerVision:Endpoint"];
            imgAnalysisClient = new ImageAnalysisClient(new Uri(_endpoint), new AzureKeyCredential(_key));
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Upload(string action, IFormFile img)
        {
            if (img == null) return View("Index");
            using (var ms = new MemoryStream())
            {
                img.CopyTo(ms);
                HttpContext.Session.Set("UploadedImage", ms.ToArray()); // Store in Session
            }
            return RedirectToAction(action);
        }

        public ActionResult DisplayImage()
        {
            byte[] imageData = HttpContext.Session.Get("UploadedImage");
            if (imageData != null)
            {
                return File(imageData, "image/png"); // Adjust MIME type as needed
            }
            return NotFound();
        }

        public IActionResult OCR()
        {
            var img = HttpContext.Session.Get("UploadedImage");
            ImageAnalysisResult result = imgAnalysisClient.Analyze(BinaryData.FromBytes(img),
                VisualFeatures.Read);
            string text = "";
            foreach (var line in result.Read.Blocks.SelectMany(block => block.Lines))
            {
                text += (line.Text + "\n");
            }
            ViewBag.text = text;
            return View(result);
        }

        public IActionResult ImageAnalysis()
        {
            var img = HttpContext.Session.Get("UploadedImage");
            ImageAnalysisResult result = imgAnalysisClient.Analyze(BinaryData.FromBytes(img),
                VisualFeatures.Objects | VisualFeatures.Tags | VisualFeatures.Caption);
            return View(result);
        }

        public async Task<IActionResult> Face()
        {
            var img = HttpContext.Session.Get("UploadedImage");
            var key = _config["FaceAI:Key"];
            var endpoint = _config["FaceAI:Endpoint"];
            var requiredFaceAttributes = new FaceAttributeType[] {
                FaceAttributeType.Detection01.Blur,
                FaceAttributeType.Detection01.HeadPose,
                FaceAttributeType.Detection01.Accessories,
                FaceAttributeType.Detection01.Glasses,
                FaceAttributeType.Detection01.Exposure
            };

            var faceClient = new FaceClient(new Uri(endpoint), new AzureKeyCredential(key));
            var response = await faceClient.DetectAsync(BinaryData.FromBytes(img), FaceDetectionModel.Detection01, FaceRecognitionModel.Recognition04,  returnFaceId: false, returnFaceAttributes: requiredFaceAttributes);
            IReadOnlyList<FaceDetectionResult> faces = response.Value;
            return View(faces);
        }

        //творче завдання
        public IActionResult PIICensor()
        {
            var img = HttpContext.Session.Get("UploadedImage");
            ImageAnalysisResult result = imgAnalysisClient.Analyze(BinaryData.FromBytes(img),
                VisualFeatures.Read);
            var lines = result.Read.Blocks.SelectMany(block => block.Lines);
            string text = string.Join(" ", lines.Select(l => l.Text));

            var textKey = _config["LanguageAI:Key"];
            var textEndpoint = _config["LanguageAI:Endpoint"];

            var textClient = new TextAnalyticsClient(new Uri(textEndpoint), new AzureKeyCredential(textKey));
            var entities = textClient.RecognizePiiEntities(text).Value;

            var boundingBoxList = new List<IReadOnlyList<ImagePoint>>();
            foreach (var line in lines)
            {
                foreach (var word in line.Words)
                {
                    boundingBoxList.Add(word.BoundingPolygon);
                }
            }

            var PIIBoundingBoxes = new List<List<Point>>();
            foreach (var entity in entities)
            {
                int wordsBefore = CountWords(text.Substring(0, entity.Offset));
                int wordsIn = CountWords(text.Substring(entity.Offset, entity.Length));
                for (int i = 0; i < wordsIn; i++)
                {
                    var points = boundingBoxList[wordsBefore + i];
                    PIIBoundingBoxes.Add(points.ToList().ConvertAll(p => new Point(p.X, p.Y)));
                }
            }
            var censoredImage = CensorImage(img, PIIBoundingBoxes);
            HttpContext.Session.Set("UploadedImage", censoredImage); 
            return View(result);
        }

        static int CountWords(string str)
        {
                return str.Split(" ", StringSplitOptions.RemoveEmptyEntries).Length;
        }

        public static byte[] CensorImage(byte[] imageBytes, List<List<Point>> polygons)
        {
            using (MemoryStream ms = new MemoryStream(imageBytes))
            using (Bitmap bitmap = new Bitmap(ms))
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                // Draw the polygon
                using (Brush brush = new SolidBrush(Color.Black))
                {
                    foreach (var polygon in polygons)
                    { 
                        g.FillPolygon(brush, polygon.ToArray()); 
                    }
                }

                // Convert modified image back to byte array
                using (MemoryStream outputMs = new MemoryStream())
                {
                    bitmap.Save(outputMs, ImageFormat.Jpeg);
                    return outputMs.ToArray();
                }
            }
        }
    }
}
