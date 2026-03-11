using Autofac;
using Bioss.Ultrasound.Ble;
using Bioss.Ultrasound.Ble.Devices;
using System;
using Xamarin.Forms;

namespace Bioss.Ultrasound.DI.Modules
{
    public class BleModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            //if (Device.RuntimePlatform == Device.iOS)
            //    builder.RegisterType<MyDeviceIos>().As<IMyDevice>().SingleInstance();
            //else 
            if (Device.RuntimePlatform == Device.Android)
                builder.RegisterType<MyDeviceAndroid>().As<IMyDevice>().SingleInstance();
            else
                throw new NotImplementedException($"IMyDevice for platform {Device.RuntimePlatform} not implemented");

            builder.RegisterType<DevicesScaner>().SingleInstance();

        }
    }
}
