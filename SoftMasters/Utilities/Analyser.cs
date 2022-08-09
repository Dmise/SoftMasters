using WebApp.Models;

namespace WebApp.Utilities
{
    public class Analyser
    {
        private List<InvoiceXML>  _invoicesList;
        public Analyser(List<InvoiceXML> invoices)
        {
            _invoicesList = invoices;
            EntryRows = invoices.Count();
            CalculateCars();
            CalculateTrains();
            CalculateCombinedNumbers();
            CalculateFreights();
            CalculateStations();
            CalculateInvoices();


        }
        public int EntryRows { get; private set; }
        public List<int> Cars { get; private set; } = new List<int>();
        public List<int> CarsNotUnic { get; private set; } = new List<int>();
        public List<int> Trains { get; private set; } = new List<int>();
        public List<string> CombinedNumbers { get; private set; } = new List<string>();
        public List<string> Freights { get; private set; } = new List<string>();
        public List<string> Stations { get; private set; } = new List<string>();
        public List<string> Invoices { get; private set; } = new List<string>();

        private void CalculateCars()
        {
            foreach(var invoice in _invoicesList)
            {
                if (!Cars.Contains(invoice.CarNumber))
                {
                    Cars.Add(invoice.CarNumber);
                }
                else
                {
                    CarsNotUnic.Add(invoice.CarNumber);
                }
                
            }
        }
        private void CalculateTrains()
        {
            foreach (var invoice in _invoicesList)
            {
                if (!Trains.Contains(invoice.TrainNumber))
                {
                    Trains.Add(invoice.TrainNumber);
                }
            }
        }
        private void CalculateCombinedNumbers()
        {
            foreach (var invoice in _invoicesList)
            {
                if (!CombinedNumbers.Contains(invoice.TrainIndexCombined))
                {
                    CombinedNumbers.Add(invoice.TrainIndexCombined);
                }
            }
        }
        private void CalculateFreights()
        {
            foreach (var invoice in _invoicesList)
            {
                if (!Freights.Contains(invoice.FreightEtsngName))
                {
                    Freights.Add(invoice.FreightEtsngName);
                }
            }
        }
        private void CalculateStations()
        {
            foreach (var invoice in _invoicesList)
            {
                if (!Stations.Contains(invoice.LastStationName))
                {
                    Stations.Add(invoice.LastStationName);
                }
                if (!Stations.Contains(invoice.ToStationName))
                {
                    Stations.Add(invoice.ToStationName);
                }
                if (!Stations.Contains(invoice.FromStationName))
                {
                    Stations.Add(invoice.FromStationName);
                }
            }
        }
        private void CalculateInvoices()
        {
            foreach (var invoice in _invoicesList)
            {
                if (!Invoices.Contains(invoice.InvoiceNum))
                {
                    Invoices.Add(invoice.InvoiceNum);
                }
            }
        }
    }
}
