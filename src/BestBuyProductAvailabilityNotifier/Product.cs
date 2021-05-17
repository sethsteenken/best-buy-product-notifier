namespace BestBuyProductAvailabilityNotifier
{
    public class Product
    {
        public string name { get; set; }
        public string modelNumber { get; set; }
        public bool onlineAvailability { get; set; }
        public bool inStoreAvailability { get; set; }

        public override string ToString()
        {
            return $"{modelNumber} | {name} | Online? {onlineAvailability} | In store? {inStoreAvailability}";
        }
    }
}
