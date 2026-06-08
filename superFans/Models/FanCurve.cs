namespace superFans.Models;

public class FanCurve
{
    public List<CurvePoint> Points { get; } = new()
    {
        new(30, 20),
        new(45, 40),
        new(60, 65),
        new(75, 85),
        new(85, 100)
    };

    public double Evaluate(double temperatureC)
    {
        var pts = Points;
        if (pts.Count == 0) return 20;

        if (temperatureC <= pts[0].TemperatureC)
            return pts[0].SpeedPercent;

        if (temperatureC >= pts[^1].TemperatureC)
            return pts[^1].SpeedPercent;

        for (int i = 0; i < pts.Count - 1; i++)
        {
            var p1 = pts[i];
            var p2 = pts[i + 1];
            if (temperatureC >= p1.TemperatureC && temperatureC <= p2.TemperatureC)
            {
                double ratio = (temperatureC - p1.TemperatureC) / (p2.TemperatureC - p1.TemperatureC);
                return p1.SpeedPercent + ratio * (p2.SpeedPercent - p1.SpeedPercent);
            }
        }

        return pts[^1].SpeedPercent;
    }
}
