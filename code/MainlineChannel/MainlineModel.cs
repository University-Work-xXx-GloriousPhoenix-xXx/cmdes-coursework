using PetriObj;

namespace MainlineChannel;

/// <summary>
/// Петрі-об'єктна модель магістрального каналу передачі даних (завдання B10).
/// Об'єкти: Generator, Main channel, Extra channel, Breaks&amp;Repair.
/// Спільні позиції (ототожнення): Storage, Main active, Transfer active, Extra active.
/// </summary>
public sealed class MainlineModel
{
    private readonly Parameters _p;
    private readonly Model _model;
    private readonly Place _storage;
    private readonly Place _mainActive;
    private readonly Transition _processMain, _toProcessed, _toStorage, _processExtra;

    // Моменти надходження повідомлень, що чекають у накопичувачі (порядок FIFO).
    private readonly LinkedList<double> _queue = new();
    private double _mainMessage, _mainOutput, _extraMessage;

    /// <summary>Час перебування повідомлення в системі: від надходження до завершення передачі.</summary>
    public SampleStatistics TransferTime { get; } = new();

    public MainlineModel(Parameters p, int seed)
    {
        _p = p;
        var random = new Random(seed);

        _storage = new Place("Storage");
        _mainActive = new Place("Main active", 1);
        var transferActive = new Place("Transfer active");
        var extraActive = new Place("Extra active");

        var generator = Generator(random, _storage);
        var main = Channel("Main channel", "main", random, _storage, _mainActive, transferActive);
        var extra = Channel("Extra channel", "extra", random, _storage, extraActive, interrupt: null);
        var breaks = BreaksRepair(random, _mainActive, transferActive, extraActive);

        _model = new Model([generator, main, extra, breaks], random);

        _processMain = main["Process main"];
        _toProcessed = main["To processed"];
        _toStorage = main["To storage"];
        _processExtra = extra["Process extra"];

        // Відстеження окремих повідомлень для статистики часу передачі.
        generator["Receive"].OnExit += t => _queue.AddLast(t);
        _processMain.OnEnter += _ => _mainMessage = Dequeue();
        _processMain.OnExit += _ => _mainOutput = _mainMessage; // канал вже вільний, повідомлення — у Main output
        _toStorage.OnExit += _ => _queue.AddFirst(_mainOutput); // повторна передача з початку
        _toProcessed.OnExit += t => TransferTime.Add(t - _mainOutput);
        _processExtra.OnEnter += _ => _extraMessage = Dequeue();
        _processExtra.OnExit += t => TransferTime.Add(t - _extraMessage);
        _model.StatisticsReset += TransferTime.Reset;
    }

    public Model Model => _model;

    public Results Run(double endTime, double warmup = 0)
    {
        _model.Run(endTime, warmup);
        if (_queue.Count != _storage.Marks)
            throw new InvalidOperationException("Черга повідомлень не відповідає маркуванню Storage.");

        var period = endTime - warmup;
        int main = _toProcessed.Exited, extra = _processExtra.Exited, interrupted = _toStorage.Exited;
        var profit = main * (_p.ProfitMain - _p.PenaltyPerSecond * _p.K)
                   + extra * (_p.ProfitBackup - _p.PenaltyPerSecond * _p.K);

        return new Results(
            Period: period,
            TransmittedMain: main,
            TransmittedExtra: extra,
            Interrupted: interrupted,
            UtilizationMain: _processMain.MeanBuffer,
            UtilizationExtra: _processExtra.MeanBuffer,
            MainAvailability: _mainActive.Mean,
            MeanQueue: _storage.Mean,
            MaxQueue: _storage.Max,
            Profit: profit,
            TransferTimeMean: TransferTime.Mean,
            TransferTimeStdDev: TransferTime.StdDev,
            TransferTimeMin: TransferTime.Min,
            TransferTimeMax: TransferTime.Max);
    }

    private double Dequeue()
    {
        var t = _queue.First!.Value;
        _queue.RemoveFirst();
        return t;
    }

    // ---- Петрі-об'єкти ----

    private PetriObject Generator(Random random, Place storage)
    {
        var o = new PetriObject("Generator");
        var ready = new Place("Ready", 1);
        o.Add(new Transition("Receive", Dist.Unif(random, _p.ArrivalMean, _p.ArrivalDev)))
            .In(ready).Out(ready).Out(storage);
        return o;
    }

    /// <param name="interrupt">Позиція-ознака збою; null — канал без переривання передачі.</param>
    private PetriObject Channel(string name, string label, Random random,
        Place storage, Place active, Place? interrupt)
    {
        var o = new PetriObject(name);
        var free = new Place($"{Capitalize(label)} free", 1);
        var processed = new Place($"Processed {label}");

        var process = o.Add(new Transition($"Process {label}", Dist.Unif(random, _p.TransmitMean, _p.TransmitDev)))
            .In(storage).In(free).Info(active).Out(free);

        if (interrupt is null)
        {
            process.Out(processed);
            return o;
        }

        var output = new Place($"{Capitalize(label)} output");
        process.Out(output);
        o.Add(new Transition("To processed", Dist.Const(0)))
            .In(output).Out(processed);
        o.Add(new Transition("To storage", Dist.Const(0), priority: 1))
            .In(output).Info(interrupt).Out(storage);
        return o;
    }

    private PetriObject BreaksRepair(Random random, Place mainActive, Place transferActive, Place extraActive)
    {
        var o = new PetriObject("Breaks&Repair");
        var repaired = new Place("Repaired", 1);
        var broken = new Place("Broken");
        var toDeactivate = new Place("To deactivate");
        var toActivate = new Place("To activate");
        var toReactivate = new Place("To reactivate");

        o.Add(new Transition("Break timeout", Dist.Unif(random, _p.FailureMean + _p.K, _p.FailureDev)))
            .In(repaired).Out(toDeactivate).Out(broken).Out(transferActive);
        o.Add(new Transition("Deactivate main", Dist.Const(0)))
            .In(toDeactivate).In(mainActive).Out(toActivate);
        o.Add(new Transition("Activate extra", Dist.Const(_p.BackupStartup)))
            .In(toActivate).Out(extraActive);
        o.Add(new Transition("Repair timeout", Dist.Unif(random, _p.RepairMean, _p.RepairDev)))
            .In(broken).Out(repaired).Out(toReactivate);
        o.Add(new Transition("Reactivate main", Dist.Const(0)))
            .In(toReactivate).In(extraActive).In(transferActive).Out(mainActive);
        return o;
    }

    private static string Capitalize(string s) => char.ToUpper(s[0]) + s[1..];
}
