using System.ComponentModel.DataAnnotations;

namespace AutoMarket.Intake.ApiService.Data;

public class VehicleScan
{
    public int Id { get; set; }

    [Required]
    public required string Vin { get; set; } // "required" keyword enforces it can't be null

    public DateTime ScannedAt { get; set; } = DateTime.UtcNow;

    // We store the results from the Grader here
    public string? Grade { get; set; }
    public decimal EstimatedValue { get; set; }

    // For simplicity in Postgres, we'll store the list of notes as a single string
    public string? Notes { get; set; }

    public double ProcessingLatencyMs { get; set; }
}