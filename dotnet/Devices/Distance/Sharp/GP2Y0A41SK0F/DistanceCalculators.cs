namespace Devices.Distance.Sharp.GP2Y0A41SK0F
{
    public class DistanceCalculators
    {
        /// <summary>
        /// 4-30 cm (or 9-42 cm in practice)
        /// </summary>
        public static DistanceCalculator GP2Y0A41SK0F { get; } = new DistanceCalculator(new [] {
            new PlotSample(0.75, 42),
            new PlotSample(0.85, 37.6),
            new PlotSample(1.035, 31.6),
            new PlotSample(1.13, 25.6),
            new PlotSample(1.22, 20.8),
            new PlotSample(1.31, 19.9),
            new PlotSample(1.41, 18.6),
            new PlotSample(1.5, 17.1),
            new PlotSample(1.6, 16.3),
            new PlotSample(1.69, 15.5),
            new PlotSample(1.79, 14.7),
            new PlotSample(1.88, 14.3),
            new PlotSample(1.98, 13.6),
            new PlotSample(2.07, 13),
            new PlotSample(2.16, 12.3),
            new PlotSample(2.26, 12),
            new PlotSample(2.35, 11.4),
            new PlotSample(2.45, 10.9),
            new PlotSample(2.54, 10.3),
            new PlotSample(2.64, 9.9),
            new PlotSample(2.73, 9.5),
            new PlotSample(2.82, 9.2),
            new PlotSample(2.91, 9)
        });
        
        /// <summary>
        /// 20 - 150 cm (or 45-
        /// </summary>
        public static DistanceCalculator GP2Y0A02YK { get; } = new DistanceCalculator(new [] {
            
            new PlotSample(0.39, 168),
            new PlotSample(0.45, 147),
            new PlotSample(0.51, 134),
            new PlotSample(0.58, 128),
            new PlotSample(0.64, 120),
            new PlotSample(0.78, 110),
            new PlotSample(0.84, 103),
            new PlotSample(0.90, 98),
            new PlotSample(0.96, 95),
            new PlotSample(1.03, 87),
            new PlotSample(1.1, 83.5),
            new PlotSample(1.16, 78),
            new PlotSample(1.22, 74),
            new PlotSample(1.29, 70),
            new PlotSample(1.35, 68),
            new PlotSample(1.41, 65.5),
            new PlotSample(1.48, 62.5),
            new PlotSample(1.54, 61),
            new PlotSample(1.61, 58),
            new PlotSample(1.67, 56.5),
            new PlotSample(1.73, 54),
            new PlotSample(1.80, 52),
            new PlotSample(1.86, 51.5),
            new PlotSample(1.93, 48),
            new PlotSample(1.99, 45)
        });
    }
}