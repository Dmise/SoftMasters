using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Models
{
    public class Operation
    {
        public int CarNumber { get; set; }
        public DateTime WhenLastOperation { get; set; }
        [Column(TypeName = "varchar(90)")]
        public string LastOperationName { get; set; } = String.Empty;

        [Column(TypeName = "varchar(90)")]
        public string StationName { get; set; } = String.Empty;

        //navigationsl properties

        [ForeignKey("CarNumber")]
        public virtual Car _Car { get; set; } = new Car();
        [ForeignKey("StationName")]
        public virtual Station _Station { get; set; } = new Station();
        [ForeignKey("LastOperationName")]
        public virtual OperationName _OperationName { get; set; } = new OperationName();


    }
}
