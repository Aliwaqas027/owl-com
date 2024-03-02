using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace OwlApi.Models
{
    public class OptimapiPlan
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public bool Finished { get; set; }
        public DateTime CreatedAt { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }
        [InverseProperty("Plan")]
        public ICollection<OptimapiSolution> Solutions { get; set; }
    }
}
