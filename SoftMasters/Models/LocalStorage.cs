namespace WebApp.Models
{
    public class LocalStorage
    {                
        List<Car> SavedCars { get; set; } = new List<Car>();
        List<Composition> SavedComposition { get; set; } = new List<Composition>();
        List<Freight> SavedFreight { get; set; } = new List<Freight>();
        List<Invoice> SavedInvoices { get; set; } = new List<Invoice>();
        List<Operation> SavedOperations { get; set; } = new List<Operation>();
        List<OperationName> SavedOperationNames { get; set; } = new List<OperationName>();
        List<Station> SavedStations { get; set; } = new List<Station>();
        List<Train> SavedTrains { get; set; } = new List<Train>();
    }
}
