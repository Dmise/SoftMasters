using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Models
{
    using System.ComponentModel.DataAnnotations;

    [Table("Stations")]
    public class Station
    {
        
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(TypeName ="varchar(90)")]
        public string Name { get; set; } = String.Empty;

        //navigation property
        [NotMapped] //! вот прям очень важно указать, а то ничего не склеится.
        public virtual ICollection<Train> TrainsTo { get; set; } = new List<Train>(); // ObservableCollection
        [NotMapped]
        public virtual ICollection<Train> TrainsFrom { get; set; } = new List<Train>(); // NotMapped переопределяется в API

    }
}
