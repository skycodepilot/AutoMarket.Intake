using System.Diagnostics;

namespace AutoMarket.Intake.ApiService;

public static class Telemetry
{
    public const string ServiceName = "AutoMarket.Intake.ApiService";
    public static readonly ActivitySource ActivitySource = new(ServiceName);
}