using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace My_Dashboard.Models.DB
{
    public class Machine
    {
        public int MachineId { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public int MachineTypeId { get; set; }

        public DateTime DateTime { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        public MachineType MachineType { get; set; } = null!;
    }
}
