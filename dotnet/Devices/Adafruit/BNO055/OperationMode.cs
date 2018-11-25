namespace Devices.Adafruit.BNO055
{
    public enum OperationMode
    {
        /// <summary>
        /// This mode is used to configure BNO, wherein all output data is reset to zero and sensor fusion is halted.
        /// This is the only mode in which all the writable register map entries can be changed.
        /// </summary>
        CONFIG = 0X00,
        /// <summary>
        /// 3.3.2.1
        /// BNO055 behaves like a stand-alone acceleration sensor.
        /// If the application requires only raw accelerometer data, this mode can be chosen.
        /// In thismode the other sensors (magnetometer, gyro) are suspended to lower the power consumption.
        /// </summary>
        ACCONLY = 0X01,
        /// <summary>
        /// 3.3.2.1
        /// BNO055 behaves like a stand-alone magnetometer,
        /// with acceleration sensor and gyroscope being suspended.
        /// </summary>
        MAGONLY = 0X02,
        /// <summary>
        /// 3.3.2.2
        /// BNO055 behaves like a stand-alone gyroscope, with acceleration sensor and magnetometer being suspended.
        /// </summary>
        GYRONLY = 0X03,
        /// <summary>
        /// 3.3.2.3
        /// Both accelerometer and magnetometer are switched on, the user can read the data from these two sensors.
        /// </summary>
        ACCMAG = 0X04,
        /// <summary>
        /// 3.3.2.4 Both accelerometer and gyroscope are switched on; the user can read the data from these two sensors.
        /// </summary>
        ACCGYRO = 0X05,
        /// <summary>
        /// 3.3.2.5 Both magnetometer and gyroscope are switched on, the user can read the data from these two sensors.
        /// 
        /// </summary>
        MAGGYRO = 0X06,
        /// <summary>
        /// 3.3.2.6 AMG (ACC-MAG-GYRO) All three sensors accelerometer, magnetometer and gyroscope are switched on.
        /// </summary>
        AMG = 0X07,
        /// <summary>
        /// 3.3.3.1 IMU (Inertial Measurement Unit)
        /// In the IMU mode the relative orientation of the BNO055 in space is calculated from the accelerometer and gyroscope data.
        /// The calculation is fast (i.e. high output data rate).
        /// </summary>
        IMUPLUS = 0X08,
        /// <summary>
        /// 3.3.3.2 The COMPASS mode is intended to measure the magnetic earth field and calculate the geographic direction.
        /// </summary>
        COMPASS = 0X09,
        /// <summary>
        /// 3.3.3.3 M4G (Magnet for Gyroscope)
        /// The M4G mode is similar to the IMU mode, but instead of using the gyroscope signal to detect rotation,
        /// the changing orientation of the magnetometer in the magnetic field is used. 
        /// </summary>
        M4G = 0X0A,
        /// <summary>
        /// 3.3.3.4 This fusion mode is same as NDOF mode, but with the Fast Magnetometer Calibration turned ‘OFF’. 
        /// </summary>
        NDOF_FMC_OFF = 0X0B,
        /// <summary>
        /// 3.3.3.5 This is a fusion mode with 9 degrees of freedom
        /// where the fused absolute orientation data is calculated from accelerometer, gyroscope and the magnetometer.
        /// The advantages of combining all three sensors are a fast calculation,
        /// resulting in high output data rate, and high robustness from magnetic field distortions.
        /// In this mode the Fast Magnetometer calibration is turned ON and thereby resulting in quick calibration of the magnetometer and higher output data accuracy.
        /// The current consumption is slightly higher in comparison to the NDOF_FMC_OFF fusion mode.
        /// </summary>
        NDOF = 0X0C
    }
}