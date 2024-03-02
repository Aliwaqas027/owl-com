using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace OwlApi.Models
{
    public enum ReservationStatus
    {
        AwaitingArrival,
        Arrived,
        UnloadingStarted,
        UnloadingDone,
        Departed
    }

    public class ReservationStatusUpdate
    {
        public int Id { get; set; }
        public ReservationStatus status { get; set; }

        public DateTime CreatedAt { get; set; }

        [ForeignKey("ReservationId")]
        public Reservation Reservation { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
