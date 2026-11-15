using System.Globalization;
using MainlineChannel;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
Console.OutputEncoding = System.Text.Encoding.UTF8;

var time = Arg("--time", 100_000);
var warmup = Arg("--warmup", 0);
var k = Arg("--k", 0);
var seed = (int)Arg("--seed", 1);

var model = new MainlineModel(new Parameters { K = k }, seed);
if (args.Contains("--trace")) model.Model.Trace = Console.Out;
var r = model.Run(time, warmup);

Console.WriteLine($"""

    Час моделювання: {time} с (перехідний період {warmup} с), k = {k}, seed = {seed}

    Передано повідомлень:        {r.Transmitted}  (основний {r.TransmittedMain}, резервний {r.TransmittedExtra})
    Перервано передач:           {r.Interrupted}  ({r.InterruptRate * 3600:F3} 1/год, {r.InterruptShare:P2} від переданих)
    Завантаження основного:      {r.UtilizationMain:F4}
    Завантаження резервного:     {r.UtilizationExtra:F4}
    Готовність основного каналу: {r.MainAvailability:F4}
    Черга у накопичувачі:        середня {r.MeanQueue:F3}, максимальна {r.MaxQueue}
    Час передачі, с:             середній {r.TransferTimeMean:F3}, СКВ {r.TransferTimeStdDev:F3}, min {r.TransferTimeMin:F3}, max {r.TransferTimeMax:F3}
    Прибуток:                    {r.Profit:F1} у.о.  ({r.ProfitRate * 3600:F2} у.о./год)
    """);

double Arg(string name, double fallback)
{
    var i = Array.IndexOf(args, name);
    return i >= 0 && i + 1 < args.Length ? double.Parse(args[i + 1], CultureInfo.InvariantCulture) : fallback;
}
