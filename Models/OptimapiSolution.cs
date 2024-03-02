using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace OwlApi.Models
{
    public class OptimapiSolution
    {
        public int Id { get; set; }
        public int PlanId { get; set; }
        public int Iteration { get; set; }
        public bool Final { get; set; }
        public DateTime CreatedAt { get; set; }

        [ForeignKey("PlanId")]
        public OptimapiPlan Plan { get; set; }
        [InverseProperty("Solution")]
        public OptimapiSolutionFile File { get; set; }
    }
}
