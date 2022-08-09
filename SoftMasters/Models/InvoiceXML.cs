using System.Xml.Serialization;


namespace WebApp.Models
{
    [XmlRoot(ElementName = "Root")]
    public class InvoicesRoot
    {
        [XmlElement("row")]
        public List<InvoiceXML> Rows { get; set; }
    }
    
    public class InvoiceXML
    {
        [XmlElement]
        public int TrainNumber { get; set; }
        [XmlElement]
        public string TrainIndexCombined { get; set; }
        [XmlElement]
        public string FromStationName { get; set; }
        [XmlElement]
        public string ToStationName { get; set; }
        [XmlElement]
        public string LastStationName { get; set; }
        [XmlElement]
        public string WhenLastOperation { get; set; }
        [XmlElement]
        public string LastOperationName { get; set; }
        [XmlElement]
        public string InvoiceNum { get; set; }
        [XmlElement]
        public int PositionInTrain { get; set; }
        [XmlElement]
        public int CarNumber { get; set; }
        [XmlElement]
        public string FreightEtsngName { get; set; }
        [XmlElement]
        public int FreightTotalWeightKg { get; set; }
    }
}
