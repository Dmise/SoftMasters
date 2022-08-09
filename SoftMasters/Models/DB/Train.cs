using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace WebApp.Models
{

    [Table("Trains")]
    public class Train
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TrainId { get; set; } = 0;
        [Key]
        [Column(TypeName = "varchar(90)")]        
        public string FromStationName { get; set; }
        [Key]
        [Column(TypeName = "varchar(90)")]
        public string ToStationName { get; set; }
        //public string FreeString { get; set; }
                

        [ForeignKey("FromStationName")]
        [NotMapped]
        public virtual Station fromStation { get; set; } = new Station();
        [ForeignKey("ToStationName")]
        [NotMapped]
        public virtual Station toStation { get; set; } = new Station();
        [NotMapped]
        public virtual List<Composition> Compositions { get; set; } = new List<Composition>();
        
        
    }

}
