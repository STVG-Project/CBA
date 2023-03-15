using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CBA.Models
{
    [Table("tb_face")]
    public class SqlFace
    {
        public long ID { get; set; }
        public int age { get; set; } = 0;
        public string gender { get; set; } = "";
        public string image { get; set; } = "";
        public SqlPerson? person { get; set; }
        public SqlDevice? device { get; set; }
        public DateTime createdTime { get; set; }
        public bool isdeleted { get; set; } = false;
       
    }
}
