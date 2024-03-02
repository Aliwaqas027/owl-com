using System.ComponentModel.DataAnnotations.Schema;

namespace OwlApi.Models
{
    public class OptimapiSolutionFile
    {
        public int Id { get; set; }
        public int SolutionId { get; set; }
        public byte[] Data { get; set; }

        [ForeignKey("SolutionId")]
        public OptimapiSolution Solution { get; set; }
    }
}
