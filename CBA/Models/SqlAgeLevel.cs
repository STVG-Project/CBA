using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CBA.Models
{
    [Table("tb_age")]
    public class SqlAgeLevel
    {
        public long ID { get; set; }
        public string code { get; set; } = "";
        public string name { get; set; } = "";
        public string des { get; set; } = "";
        public int low { get; set; } = int.MinValue;
        public int high { get; set; } = int.MaxValue;
        public bool isdeleted { get; set; } = false;

    }
}
