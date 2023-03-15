using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CBA.Models
{
    [Table("tb_logPerson")]
    public class SqlLogPerson
    {
        [Key]
        public long ID { get; set; }
        public SqlPerson? person { get; set; }
        public SqlDevice? device { get; set; }
        public string image { get; set; } = "";

        public string note { get; set; } = "";
        public DateTime time { get; set; }
        
    }
}
