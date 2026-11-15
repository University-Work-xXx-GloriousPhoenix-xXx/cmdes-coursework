namespace MainlineChannel;

/// <summary>Вхідні змінні моделі (часи — у секундах, прибуток — у грошових одиницях).</summary>
public sealed record Parameters
{
    public double ArrivalMean { get; init; } = 9;
    public double ArrivalDev { get; init; } = 5;
    public double TransmitMean { get; init; } = 7;
    public double TransmitDev { get; init; } = 3;
    public double FailureMean { get; init; } = 200;
    public double FailureDev { get; init; } = 35;
    public double RepairMean { get; init; } = 70;
    public double RepairDev { get; init; } = 7;
    public double BackupStartup { get; init; } = 2;

    public double ProfitMain { get; init; } = 50;
    public double ProfitBackup { get; init; } = 25;
    /// <summary>Зменшення прибутку з повідомлення на кожну секунду приросту напрацювання на відмову.</summary>
    public double PenaltyPerSecond { get; init; } = 0.03;

    /// <summary>Приріст середнього часу напрацювання на відмову, с.</summary>
    public double K { get; init; }
}
