using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Models
{
    [Table("Cars")]
    
    public class Car
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("CarId")]
        public int CarNumber { get; set; }
        public int PositionInTarin { get; set; }
        public string InvoiceNumber { get; set; } = String.Empty;
        public string FreightName { get; set; } = String.Empty;
        public int Weight { get; set; }
        [Column("CompositionNumber")]
        public string CompositionNumber { get; set; } = String.Empty;


        //navigation property
        [ForeignKey("InvoiceNumber")]

        public Invoice _Invoice { get; set; } = new Invoice();
        [ForeignKey("FreightName")]
        public Freight _Freight { get; set; } = new Freight();

        [ForeignKey("CompositionNumber")]
        public Composition _Composition { get; set; } = new Composition();

    }
}
