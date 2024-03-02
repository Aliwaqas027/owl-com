using System.ComponentModel.DataAnnotations.Schema;

namespace OwlApi.Models
{
    public enum OptimapiStatus
    {
        Idle,
        Busy,
        Done
    }
    public class OptimapiServer
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Url { get; set; }
        [Column(TypeName = "smallint")]
        public OptimapiStatus Status { get; set; }
        public string StopsSettings { get; set; }
        public string VehiclesSettings { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
