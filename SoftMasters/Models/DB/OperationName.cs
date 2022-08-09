using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Models
{
    public class OperationName
    {
        [Key]
        [Column(TypeName = "varchar(90)")]
        public string Name { get; set; } = String.Empty;
    }
}