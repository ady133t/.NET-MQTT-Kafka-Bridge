using System.ComponentModel.DataAnnotations;

namespace My_Dashboard.Models.DB
{
    public class MachineType
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public DateTime DateTime { get; set; } = DateTime.Now;

        public ICollection<Machine> Machines { get; set; } = new List<Machine>();

    }


}
