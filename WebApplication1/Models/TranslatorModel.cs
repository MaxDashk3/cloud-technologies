namespace WebApplication1.Models
{
    public class TranslatorModel
    {
        public TranslatorModel() { }
        public TranslatorModel(string text, string translatedText, string sourceLanguage, string targetLanguage)
        {
            this.text = text;
            this.translatedText = translatedText;
            this.sourceLanguage = sourceLanguage;
            this.targetLanguage = targetLanguage;
        }

        public string text { get; set; }
        public string translatedText { get; set; }
        public string sourceLanguage { get; set; }
        public string targetLanguage { get; set; }
    }
}
