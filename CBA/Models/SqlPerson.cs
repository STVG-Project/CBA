using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CBA.Models
{
    [Table("tb_person")]
    public class SqlPerson
    {
        [Key]
        public long ID { get; set; }
        public string code { get; set; } = "";
        public string codeSystem { get; set; } = "";
        public string name { get; set; } = "";
        public string gender { get; set; } = "";
        public int age { get; set; } = 0;
        public SqlGroup? group { get; set; }
        public List<SqlFace>? faces { get; set; }
        public bool isdeleted { get; set; } = false;
        public DateTime lastestTime { get; set; }
        public DateTime createdTime { get; set; }
       
    }
}
