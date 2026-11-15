namespace MainlineChannel;

/// <summary>Вихідні змінні моделі за період спостереження (після перехідного періоду).</summary>
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

    /// <summary>Прибуток за одиницю часу, у.о./с.</summary>
    public double ProfitRate => Profit / Period;

    /// <summary>Частота переривань, 1/с.</summary>
    public double InterruptRate => Interrupted / Period;

    /// <summary>Частка перерваних передач відносно переданих повідомлень.</summary>
    public double InterruptShare => Transmitted > 0 ? (double)Interrupted / Transmitted : 0;
}
