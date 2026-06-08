using LibreHardwareMonitor.Hardware;
using superFans.Models;

namespace superFans.Services;

public class HardwareService : IHardwareService, IDisposable
{
    private readonly Computer _computer;
    private bool _opened;

    public HardwareService()
    {
        _computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMotherboardEnabled = true,
            IsControllerEnabled = true,
            IsPsuEnabled = false,
            IsMemoryEnabled = false,
            IsNetworkEnabled = false,
            IsStorageEnabled = false
        };
    }

    private void EnsureOpen()
    {
        if (!_opened)
        {
            _computer.Open();
            _opened = true;
        }
    }

    public List<FanDevice> ScanFans()
    {
        EnsureOpen();
        var fans = new List<FanDevice>();

        foreach (var hw in _computer.Hardware)
        {
            hw.Update();

            var sensors = hw.Sensors.ToList();

            var controlSensors = sensors.Where(s => s.SensorType == SensorType.Control).ToList();
            var fanSensors = sensors.Where(s => s.SensorType == SensorType.Fan).ToList();
            var tempSensors = sensors.Where(s => s.SensorType == SensorType.Temperature).ToList();

            var pairedFans = new HashSet<ISensor>();

            foreach (var ctrlSensor in controlSensors)
            {
                var matchedFan = fanSensors.FirstOrDefault(f =>
                    NameMatchesControl(ctrlSensor.Name, f.Name, ctrlSensor.Index));

                if (matchedFan != null)
                    pairedFans.Add(matchedFan);

                var tempSensor = PickTempSensor(tempSensors, hw.HardwareType, ctrlSensor.Name);

                var fan = new FanDevice
                {
                    Name = $"{hw.Name} — {ctrlSensor.Name}",
                    Type = ClassifyFan(hw.HardwareType, ctrlSensor.Name),
                    IsControllable = ctrlSensor.Control != null,
                    RpmSensor = matchedFan,
                    TempSensor = tempSensor,
                    Control = ctrlSensor.Control,
                    Mode = ctrlSensor.Control != null
                        ? FanControlMode.Auto
                        : FanControlMode.ReadOnly
                };

                fans.Add(fan);
            }

            // Add read-only fans (no control sensor)
            foreach (var fanSensor in fanSensors.Where(f => !pairedFans.Contains(f)))
            {
                var tempSensor = PickTempSensor(tempSensors, hw.HardwareType, fanSensor.Name);

                var fan = new FanDevice
                {
                    Name = $"{hw.Name} — {fanSensor.Name}",
                    Type = ClassifyFan(hw.HardwareType, fanSensor.Name),
                    IsControllable = false,
                    RpmSensor = fanSensor,
                    TempSensor = tempSensor,
                    Control = null,
                    Mode = FanControlMode.ReadOnly
                };

                fans.Add(fan);
            }
        }

        return fans;
    }

    public void UpdateSensors(IEnumerable<FanDevice> fans)
    {
        if (!_opened) return;
        foreach (var hw in _computer.Hardware)
            hw.Update();

        foreach (var fan in fans)
        {
            fan.Rpm = fan.RpmSensor?.Value ?? 0;
            fan.AssociatedTempC = fan.TempSensor?.Value ?? 0;
            fan.DutyCycle = fan.Control?.SoftwareValue ?? 0;
        }
    }

    public void Shutdown()
    {
        if (_opened) { _computer.Close(); _opened = false; }
    }

    public void Dispose()
    {
        if (_opened) { _computer.Close(); _opened = false; }
    }

    private static FanType ClassifyFan(HardwareType hwType, string name)
    {
        var n = name.ToLowerInvariant();

        if (hwType == HardwareType.Cpu || n.Contains("cpu"))
            return FanType.CpuFan;

        if (hwType == HardwareType.GpuNvidia || hwType == HardwareType.GpuAmd || n.Contains("gpu"))
            return FanType.GpuFan;

        if (n.Contains("pump") || n.Contains("water") || n.Contains("aio"))
            return FanType.Pump;

        if (n.Contains("chassis") || n.Contains("case") ||
            n.Contains("front") || n.Contains("rear") || n.Contains("top") || n.Contains("sys"))
            return FanType.ChassisFan;

        if (hwType == HardwareType.Motherboard || hwType == HardwareType.SuperIO)
            return FanType.ChassisFan;

        return FanType.Other;
    }

    private static ISensor? PickTempSensor(List<ISensor> tempSensors, HardwareType hwType, string fanName)
    {
        if (tempSensors.Count == 0) return null;

        ISensor? candidate = null;

        if (hwType == HardwareType.Cpu)
            candidate = tempSensors.FirstOrDefault(t =>
                t.Name.Contains("Core", StringComparison.OrdinalIgnoreCase) ||
                t.Name.Contains("Package", StringComparison.OrdinalIgnoreCase) ||
                t.Name.Contains("CPU", StringComparison.OrdinalIgnoreCase));

        if (hwType == HardwareType.GpuNvidia || hwType == HardwareType.GpuAmd)
            candidate = tempSensors.FirstOrDefault(t =>
                t.Name.Contains("Core", StringComparison.OrdinalIgnoreCase) ||
                t.Name.Contains("GPU", StringComparison.OrdinalIgnoreCase));

        candidate ??= tempSensors.FirstOrDefault(t =>
            t.Name.Contains("Chipset", StringComparison.OrdinalIgnoreCase) ||
            t.Name.Contains("System", StringComparison.OrdinalIgnoreCase) ||
            t.Name.Contains("Motherboard", StringComparison.OrdinalIgnoreCase));

        return candidate ?? tempSensors.FirstOrDefault();
    }

    private static bool NameMatchesControl(string ctrlName, string fanName, int ctrlIndex)
    {
        if (string.IsNullOrWhiteSpace(ctrlName) || string.IsNullOrWhiteSpace(fanName))
            return false;

        var cn = ctrlName.ToLowerInvariant();
        var fn = fanName.ToLowerInvariant();

        if (cn.Contains(fn) || fn.Contains(cn))
            return true;

        var cnNum = ExtractNumber(cn);
        var fnNum = ExtractNumber(fn);

        if (cnNum != null && fnNum != null && cnNum == fnNum)
            return true;

        return cn.Split('#', ' ', '_').Any(part =>
            fn.Split('#', ' ', '_').Any(fp =>
                string.Equals(part, fp, StringComparison.OrdinalIgnoreCase)));
    }

    private static int? ExtractNumber(string s)
    {
        var digits = new string(s.Where(char.IsDigit).ToArray());
        return int.TryParse(digits, out var n) ? n : null;
    }
}
