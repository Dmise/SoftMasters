namespace WebApp.Models
{
    public class CarInvoice
    {
        public int Id { get; set; }
        public int CarId { get; set; }
        public string Invoice { get; set; }
        public DateTime DepartureDate { get; set; }
        public string WeightName { get; set; }  
        public float Weight { get; set; }
        public string LastOperation { get; set; }
    }
}
