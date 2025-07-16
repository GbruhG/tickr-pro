namespace StockApiApp.Models
{
    public class News
    {
        public string Category { get; set; }
        public long Datetime { get; set; }
        public string Headline { get; set; }
        public string Id { get; set; }
        public string Image { get; set; }
        public string Related { get; set; }
        public string Source { get; set; }
        public string Summary { get; set; }
        public string Url { get; set; }
    }
}