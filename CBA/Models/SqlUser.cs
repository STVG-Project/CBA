using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CBA.Models
{
    [Table("tb_user")]
    public class SqlUser
    {
        [Key]
        public long ID { get; set; }
        public string user { get; set; } = "";
        public string username { get; set; } = "";
        public string password { get; set; } = "";
        public string token { get; set; } = "";
        public string displayName { get; set; } = "";
        public bool isdeleted { get; set; } = false;
        public string phoneNumber { get; set; } = "";
        public string des { get; set; } = "";
        public string avatar { get; set; } = "";
        public SqlRole? role { get; set; }
    }
}
