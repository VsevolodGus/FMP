namespace Bioss.Ultrasound.Ble.Commands
{
    public class SetupCommand
    {
        //  0..7
        private byte _fetalVolume;
        private byte _tocoResetValue;
        private bool _isTocoReset;

        //  0 - no
        //  1 - low
        //  2 - middle
        //  3 - hight
        private byte _alarmVolume;
        private bool _isAlarm;

        public SetupCommand(byte fetalVolume, byte tocoResetValue, bool isTocoReset, byte alarmVolume, bool isAlarm)
        {
            _fetalVolume = fetalVolume;
            _tocoResetValue = tocoResetValue;
            _isTocoReset = isTocoReset;
            _alarmVolume = alarmVolume;
            _isAlarm = isAlarm;
        }

        public byte[] WriteData()
        {
            return new byte[]
            {
                //0x00,
                0x55,
                0xAA,
                0x0A,
                _fetalVolume,
                _tocoResetValue,
                _isTocoReset ? (byte)1 : (byte)0,
                _alarmVolume,
                _isAlarm ? (byte)1 : (byte)0
            };
        }
    }
}
