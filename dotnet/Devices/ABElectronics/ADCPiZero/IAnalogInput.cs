namespace Devices.ABElectronics.ADCPiZero
{
    public interface IAnalogInput
    {
        /**
         * Returns voltage between -2.048 and 2.048 V
         */
        double ReadVoltage();
    }
}