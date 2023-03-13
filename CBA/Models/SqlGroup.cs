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
        public string des { get; set; } = "";
        public List<SqlPerson>? persons { get; set; }
        public List<SqlUser>? users { get; set; }
        public DateTime createdTime { get; set; }
        public DateTime lastestTime { get; set; }
        public bool isdeleted { get; set; } = false;
    }
}
