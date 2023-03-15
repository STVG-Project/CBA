using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CBA.Models
{
    [Table("tb_file")]
    public class SqlFile
    {
        [Key]
        public long ID { get; set; }
        public string key { get; set; } = "";
        public string link { get; set; } = "";
        public string name { get; set; } = "";
        public DateTime time { get; set; }
    }
}
