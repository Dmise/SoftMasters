using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Models
{
    public class Composition
    {
        [Key]
        public string CombinedTrainIndex { get; set; } = String.Empty;
        public int TrainId { get; set; } = 0;

        // navigation property
        [ForeignKey("TrainId")]
        
        public virtual Train Train { get; set; } = new Train();
    }
}
