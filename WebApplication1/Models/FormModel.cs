using Azure;
using Azure.AI.TextAnalytics;

namespace WebApplication1.Models
{
    public class FormModel
    {
        public FormModel() { }
        public FormModel(string text, CategorizedEntityCollection recognizedEntities, List<EntityModel> randomEntities, string[] answers = null)
        {
            this.text = text;
            this.recognizedEntities = recognizedEntities;
            this.randomEntities = randomEntities;
            this.answers = answers;
        }
        public string text { get; set; }
        public CategorizedEntityCollection? recognizedEntities { get; set; }
        public List<EntityModel> randomEntities { get; set; }
        public string[]? answers { get; set; }
    }
}
