namespace MainlineChannel;

public sealed record Results(
    double Period,
    int TransmittedMain,
    int TransmittedExtra,
    int Interrupted,
    double UtilizationMain,
    double UtilizationExtra,
    double MainAvailability,
    double MeanQueue,
    int MaxQueue,
    double Profit,
    double TransferTimeMean,
    double TransferTimeStdDev,
    double TransferTimeMin,
    double TransferTimeMax)
{
    public int Transmitted => TransmittedMain + TransmittedExtra;

    public double ProfitRate => Profit / Period;

    public double InterruptRate => Interrupted / Period;

    public double InterruptShare => Transmitted > 0 ? (double)Interrupted / Transmitted : 0;
}
