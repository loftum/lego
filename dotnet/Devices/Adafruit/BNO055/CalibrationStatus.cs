using System.Linq;

namespace Devices.Adafruit.BNO055
{
    public struct CalibrationStatus
    {
        public byte System { get; }
        public byte Gyro { get; }
        public byte Accel { get; }
        public byte Mag { get; }

        public CalibrationStatus(byte system, byte gyro, byte accel, byte mag)
        {
            System = system;
            Gyro = gyro;
            Accel = accel;
            Mag = mag;
        }

        public override string ToString()
        {
            return $"Accel: {Accel}, Gyro: {Gyro}, Mag: {Mag}, System: {System}";
        }

        public bool IsCalibrated()
        {
            return new[] {System, Gyro, Accel, Mag}.All(v => v == 3);
        }

        public Calibration CalibrateNext()
        {
            if (Mag != 3)
            {
                return Calibration.Mag;
            }

            if (Gyro != 3)
            {
                return Calibration.Gyro;
            }

            if (Accel != 3)
            {
                return Calibration.Accel;
            }

            if (System != 3)
            {
                return Calibration.System;
            }

            return Calibration.Done;
        }
    }
}