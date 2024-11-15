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
    public class Destination
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        [DisplayProperty]
        [DisplayName("City")]
        public string city { get; set; }
        [DisplayProperty]
        [DisplayName("Price")]
        public double price { get; set; }
        [DisplayProperty]
        [DisplayName("Distance")]
        public double distance { get; set; }
        [DisplayProperty]
        [DisplayName("Departure Date")]
        public DateTime departure_date { get; set; }
        [DisplayProperty]
        [DisplayName("Discount")]
        public Discount? discount { get; set; }
        public Airline Airline { get; set; }
        public int AirlineId { get; set; }

        public override string ToString()
        {
            string ds = discount?.ToString() ?? string.Empty;
            return $"{city}:{price}:{distance}:{departure_date}{ds}";
        }
    }
}
