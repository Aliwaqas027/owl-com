namespace OwlApi.Models
{
    public class Country
    {
        public int Id { get; set; }

        public string name { get; set; }

        public CountryType type { get; set; }
    }

    public enum CountryType
    {
        EU_EFTA,
        Third
    }
}
