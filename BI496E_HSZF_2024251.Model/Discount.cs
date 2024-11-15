using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BI496E_HSZF_2024251.Model
{
    public class Discount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID {  get; set; }
        [DisplayProperty]
        [DisplayName("Value")]
        public double value { get; set; }
        [DisplayProperty]
        [DisplayName("Valid From")]
        public DateTimeOffset valid_from { get; set; }
        [DisplayProperty]
        [DisplayName("Valid To")]
        public DateTimeOffset valid_to { get; set; }
    }
}
