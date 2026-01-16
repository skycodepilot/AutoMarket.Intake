using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Intake.ApiService.Data;

public class IntakeDbContext(DbContextOptions<IntakeDbContext> options) : DbContext(options)
{
    public DbSet<VehicleScan> Scans => Set<VehicleScan>();
}