using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace Bioss.Ultrasound.UI.Controls
{
    public class Pulse : SKCanvasView
    {
        private SKBitmap resourceBitmap;
        private double cycleTime = 30000;       // in milliseconds
        private Stopwatch stopwatch = new Stopwatch();
        private float[] t = new float[3];

        public static readonly BindableProperty PulseColorProperty =
            BindableProperty.Create(nameof(PulseColor), typeof(Color), typeof(Pulse), Color.Red, propertyChanged: OnPropertyChanged);

        public static readonly BindableProperty AutoStartProperty =
            BindableProperty.Create(nameof(AutoStart), typeof(bool), typeof(Pulse), false, propertyChanged: OnPropertyChanged);

        public static readonly BindableProperty SourceProperty =
            BindableProperty.Create(nameof(Source), typeof(string), typeof(Pulse), "", propertyChanged: OnPropertyChanged);

        public static readonly BindableProperty SpeedProperty =
            BindableProperty.Create(nameof(Speed), typeof(int), typeof(Pulse), 10, propertyChanged: OnPropertyChanged);

        public Color PulseColor
        {
            get { return (Color)GetValue(PulseColorProperty); }
            set { SetValue(PulseColorProperty, value); }
        }

        public bool AutoStart
        {
            get { return (bool)GetValue(AutoStartProperty); }
            set { SetValue(AutoStartProperty, value); }
        }

        public string Source
        {
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public int Speed
        {
            get { return (int)GetValue(SpeedProperty); }
            set { SetValue(SpeedProperty, value); cycleTime *= value; }
        }

        public bool IsRun { get; private set; }

        SKPaint paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke
        };
        private static void OnPropertyChanged(BindableObject bindable, object oldVal, object newVal)
        {
            var pulse = bindable as Pulse;

            pulse?.InvalidateSurface();
        }
        public Pulse()
        {

            cycleTime /= Speed;

        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == nameof(Speed))
            {
                cycleTime /= Speed;
            }
            if (propertyName == nameof(AutoStart))
            {
                if (AutoStart)
                    Start();
                else
                    Stop();

            }
            if (propertyName == nameof(Source))
            {
                string resourceID = Source;
                Assembly assembly = GetType().GetTypeInfo().Assembly;
                using (Stream stream = assembly.GetManifestResourceStream(resourceID))
                {
                    if (stream != null)
                        resourceBitmap = SKBitmap.Decode(stream);

                }
                resourceBitmap = resourceBitmap?.Resize(new SKImageInfo(90, 90), SKFilterQuality.Medium);
            }
        }

        public void Start()
        {
            IsRun = true;
            stopwatch.Start();
            Device.StartTimer(TimeSpan.FromMilliseconds(33), () =>
            {
                t[0] = (float)(stopwatch.Elapsed.TotalMilliseconds % cycleTime / cycleTime);
                if (stopwatch.Elapsed.TotalMilliseconds > cycleTime / 3)
                    t[1] = (float)((stopwatch.Elapsed.TotalMilliseconds - cycleTime / 3) % cycleTime / cycleTime);
                if (stopwatch.Elapsed.TotalMilliseconds > cycleTime * 2 / 3)
                    t[2] = (float)((stopwatch.Elapsed.TotalMilliseconds - cycleTime * 2 / 3) % cycleTime / cycleTime);
                InvalidateSurface();

                if (!IsRun)
                {
                    stopwatch.Stop();
                    stopwatch.Reset();
                }
                return IsRun;
            });
        }

        public void Stop()
        {
            IsRun = false;
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs args)
        {
            base.OnPaintSurface(args);
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;
            byte R = (byte)(PulseColor.R * 255);
            byte G = (byte)(PulseColor.G * 255);
            byte B = (byte)(PulseColor.B * 255);
            canvas.Clear();
            if (IsRun)
            {
                SKPoint center = new SKPoint(info.Width / 2, info.Height / 2);
                float baseRadius = Math.Min(info.Width, info.Height) / 12;
                float radius = 0;
                for (int i = 0; i < t.Length; i++)
                {
                    radius = info.Width / 2 * (t[i]);
                    paint.Color = new SKColor(R, G, B, (byte)(255 * (1 - t[i])));
                    paint.Style = SKPaintStyle.Fill;
                    canvas.DrawCircle(center.X, center.Y, radius, paint);
                }

                paint.Color = new SKColor(R, G, B);
                canvas.DrawCircle(center.X, center.Y, 100, paint);

                if (resourceBitmap != null)
                    canvas.DrawBitmap(resourceBitmap, center.X - resourceBitmap.Width / 2, center.Y - resourceBitmap.Height / 2);
            }
        }

    }
}
