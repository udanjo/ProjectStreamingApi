namespace ProjectStreamingApi.Application.Models
{
    public class ItemModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsComplete { get; set; }

        public override string ToString()
        {
            return $"{Id} - {Name} - {IsComplete}";
        }
    }
}