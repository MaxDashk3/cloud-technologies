namespace WebApplication1.Models
{
    public class TranslatorTestItem
    {
        public TranslatorTestItem() { }
        public TranslatorTestItem(string title, List<string>? answerOptions = null)
        {
            Title = title;
            AnswerOptions = answerOptions;
        }

        public string? Title { get; set; }
        public List<string>? AnswerOptions { get; set; }

    }
}
