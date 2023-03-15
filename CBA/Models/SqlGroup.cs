using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CBA.Models
{
    [Table("tb_group")]
    public class SqlGroup
    {
        [Key]
        public long ID { get; set; }
        public string code { get; set; } = "";
        public string name { get; set; } = "";
        public string des { get; set; } = "";
        public List<SqlPerson>? persons { get; set; }
        public bool isdeleted { get; set; } = false;
    }
}
