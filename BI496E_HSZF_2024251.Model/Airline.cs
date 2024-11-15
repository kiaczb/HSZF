using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BI496E_HSZF_2024251.Model
{
    public class DisplayPropertyAttribute : Attribute
    {

    }
    public class Airline
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        [DisplayProperty]
        [DisplayName("Name")]
        public string name { get; set; }
        [DisplayProperty]
        [DisplayName("Departure From")]
        public string? departure_from { get; set; }
        [DisplayProperty]
        [DisplayName("Destinations")]
        public ICollection<Destination> Destinations { get; set; }
        public Airline()
        {
            Destinations = new HashSet<Destination>();
        }
    }
    
}
