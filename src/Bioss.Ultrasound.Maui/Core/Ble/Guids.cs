namespace Bioss.Ultrasound.Core.Ble;

public class Guids
{
    /*
    // Gen1
    MyDevice ios: service: 0000180a-0000-1000-8000-00805f9b34fb
    MyDevice ios:    characteristic: 00002a29-0000-1000-8000-00805f9b34fb, Update: False Write: False
    MyDevice ios:    characteristic: 00002a24-0000-1000-8000-00805f9b34fb, Update: False Write: False
    MyDevice ios:    characteristic: 00002a25-0000-1000-8000-00805f9b34fb, Update: False Write: False
    MyDevice ios:    characteristic: 00002a27-0000-1000-8000-00805f9b34fb, Update: False Write: False
    MyDevice ios:    characteristic: 00002a26-0000-1000-8000-00805f9b34fb, Update: False Write: False
    MyDevice ios:    characteristic: 00002a28-0000-1000-8000-00805f9b34fb, Update: False Write: False
    MyDevice ios:    characteristic: 00002a23-0000-1000-8000-00805f9b34fb, Update: False Write: False
    MyDevice ios:    characteristic: 00002a2a-0000-1000-8000-00805f9b34fb, Update: False Write: False
    MyDevice ios: service: 49535343-fe7d-4ae5-8fa9-9fafd205e455
    MyDevice ios:    characteristic: 49535343-6daa-4d02-abf6-19569aca69fe, Update: False Write: True
    MyDevice ios:    characteristic: 49535343-aca3-481c-91ec-d85e28a60318, Update: True Write: True
    MyDevice ios: service: 0000fff0-0000-1000-8000-00805f9b34fb
    MyDevice ios:    characteristic: 0000fff1-0000-1000-8000-00805f9b34fb, Update: True Write: False
    MyDevice ios:    characteristic: 0000fff2-0000-1000-8000-00805f9b34fb, Update: False Write: True


    // Gen2
    MyDevice ios: service: 0000fff0-0000-1000-8000-00805f9b34fb
    MyDevice ios:    characteristic: 0000fff1-0000-1000-8000-00805f9b34fb, Update: True Write: False
    MyDevice ios:    characteristic: 0000fff2-0000-1000-8000-00805f9b34fb, Update: False Write: True
    MyDevice ios: service: 49535343-fe7d-4ae5-8fa9-9fafd205e455
    MyDevice ios:    characteristic: 0000fed6-0000-1000-8000-00805f9b34fb, Update: True Write: False
    MyDevice ios:    characteristic: 0000fed5-0000-1000-8000-00805f9b34fb, Update: False Write: True
    MyDevice ios: service: 0000180a-0000-1000-8000-00805f9b34fb
    MyDevice ios:    characteristic: 00002a25-0000-1000-8000-00805f9b34fb, Update: False Write: False
    MyDevice ios:    characteristic: 00002a28-0000-1000-8000-00805f9b34fb, Update: False Write: False
    MyDevice ios:    characteristic: 00002a27-0000-1000-8000-00805f9b34fb, Update: False Write: False
    MyDevice ios:    characteristic: 00002a29-0000-1000-8000-00805f9b34fb, Update: False Write: False
    MyDevice ios:    characteristic: 00002a24-0000-1000-8000-00805f9b34fb, Update: False Write: False
     */

    public static Guid SR_CUSTOM = Guid.Parse("0000fff0-0000-1000-8000-00805f9b34fb");
    public static Guid CH_CUSTOM_READ = Guid.Parse("0000fff1-0000-1000-8000-00805f9b34fb");
    public static Guid CH_CUSTOM_WRITE = Guid.Parse("0000fff2-0000-1000-8000-00805f9b34fb");

    public static Guid SR_CUSTOM_2 = Guid.Parse("49535343-fe7d-4ae5-8fa9-9fafd205e455");
    public static Guid CH_CUSTOM_READ_2 = Guid.Parse("0000fed6-0000-1000-8000-00805f9b34fb");
    public static Guid CH_CUSTOM_WRITE_2 = Guid.Parse("0000fed5-0000-1000-8000-00805f9b34fb");
}
