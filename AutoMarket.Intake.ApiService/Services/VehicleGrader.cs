namespace AutoMarket.Intake.ApiService.Services;

// A simple record to hold our grading data
public record InspectionResult(string Grade, decimal EstimatedValue, List<string> Notes);

public class VehicleGrader
{
    // A deterministic "faker" to ensure your demo is always repeatable.
    // We check the last digit of the VIN to decide the car's fate.
    public InspectionResult GradeVehicle(string vin, int mileage)
    {
        // Sanity check
        if (string.IsNullOrEmpty(vin) || vin.Length < 5)
            return new InspectionResult("N/A", 0, new List<string> { "Invalid VIN" });

        char lastDigit = vin.Last();

        // RULE 1: The "Economy Sedan" (VIN ends in 0, 1, 2)
        // High volume, reliable, average price.
        if ("012".Contains(lastDigit))
        {
            return new InspectionResult(
                Grade: "4.2",
                EstimatedValue: 18500m,
                Notes: new List<string> { "Clean CarFax", "Minor rock chips on hood", "Ready for Retail" }
            );
        }

        // RULE 2: The "Work Truck" (VIN ends in 3, 4, 5)
        // Beaten up, valuable engines, terrible bodies.
        if ("345".Contains(lastDigit))
        {
            // Simulate a slightly longer processing time for "complex" damage analysis
            Thread.Sleep(150);

            return new InspectionResult(
                Grade: "2.1",
                EstimatedValue: 12000m,
                Notes: new List<string> { "Heavy bed damage", "Odometer discrepancy", "Check Engine Light: P0420" }
            );
        }

        // RULE 3: The "Luxury Lease Return" (VIN ends in 6, 7, 8, 9)
        // Looks perfect, but depreciates like a stone.
        return new InspectionResult(
            Grade: "4.9",
            EstimatedValue: 42000m,
            Notes: new List<string> { "One owner", "Panorama sunroof verified", "Factory Warranty Active" }
        );
    }
}