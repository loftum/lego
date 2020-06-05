namespace Devices.Distance.Sharp.GP2Y0A41SK0F
{
    public class DistanceCalculators
    {
        /// <summary>
        /// 4-30 cm
        /// </summary>
        public static DistanceCalculator GP2Y0A41SK0F { get; } = new DistanceCalculator(new [] {
            
            new PlotSample(0.76, 40),
            new PlotSample(0.78, 39),
            new PlotSample(0.80, 38),
            new PlotSample(0.82, 37),
            new PlotSample(0.84, 36),
            new PlotSample(0.88, 35),
            new PlotSample(0.90, 33),
            new PlotSample(0.94, 32),
            new PlotSample(0.96, 31),
            new PlotSample(0.97, 30),
            new PlotSample(1.00, 29),
            new PlotSample(1.01, 28),
            new PlotSample(1.05, 27),
            new PlotSample(1.09, 26),
            new PlotSample(1.11, 25),
            new PlotSample(1.15, 24),
            new PlotSample(1.19, 23),
            new PlotSample(1.24, 22),
            new PlotSample(1.28, 21),
            new PlotSample(1.33, 20),
            new PlotSample(1.41, 19),
            new PlotSample(1.47, 18),
            new PlotSample(1.54, 17),
            new PlotSample(1.62, 16),
            new PlotSample(1.70, 15),
            new PlotSample(1.82, 14),
            new PlotSample(1.92, 13),
            new PlotSample(2.05, 12),
            new PlotSample(2.21, 11),
            new PlotSample(2.37, 10),
            new PlotSample(2.58, 9),
            new PlotSample(2.80, 8),
            new PlotSample(3.04, 7),
            new PlotSample(3.08, 5)
        });
        
        /// <summary>
        /// 20 - 150 cm (or 11-170)
        /// </summary>
        public static DistanceCalculator GP2Y0A02YK { get; } = new DistanceCalculator(new [] {

            new PlotSample(0.25, 100),
            new PlotSample(0.29, 160),
            new PlotSample(0.40, 150),
            new PlotSample(0.33, 140),
            new PlotSample(0.51, 130),
            new PlotSample(0.43, 120),
            new PlotSample(0.51, 110),
            new PlotSample(0.63, 100),
            new PlotSample(0.49, 90),
            new PlotSample(0.65, 80),
            new PlotSample(0.72, 70),
            new PlotSample(0.77, 60),
            new PlotSample(1.07, 50),
            new PlotSample(1.32, 40),
            new PlotSample(1.81, 30),
            new PlotSample(2.39, 20),
            new PlotSample(2.75, 11)
        });
    }
}