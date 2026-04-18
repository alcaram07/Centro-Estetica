using System.ComponentModel.DataAnnotations;

namespace AestheticCenter.Core.Entities;

public enum AppointmentStatus
{
    Pending,
    Confirmed,
    Cancelled,
    Completed
}

public class Appointment
{
    public int Id { get; set; }

    [Required]
    public string CustomerId { get; set; } = string.Empty;

    public int ServiceId { get; set; }
    public Service? Service { get; set; }

    public DateTime AppointmentTime { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

    public string Notes { get; set; } = string.Empty;
}
