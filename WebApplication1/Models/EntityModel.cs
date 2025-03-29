using Azure.AI.TextAnalytics;

namespace WebApplication1.Models
{
    public class EntityModel
    {
        public EntityModel() { }
        public string Text { get; set; }
        public int Offset { get; set; }
        public int Length { get; set; }
        public EntityModel(CategorizedEntity entity)
        {
            Text = entity.Text;
            Offset = entity.Offset;
            Length = entity.Length;
        }
    }
}
