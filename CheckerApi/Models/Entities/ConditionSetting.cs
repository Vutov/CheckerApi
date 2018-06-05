using System.ComponentModel.DataAnnotations.Schema;

namespace CheckerApi.Models.Entities
{
    [Table("ConditionSettings")]
    public class ConditionSetting
    {
        public int ID { get; set; }
        public int ConditionID { get; set; }
        public string ConditionName { get; set; }
        public bool Enabled { get; set; }
    }
}
