using Xamarin.Forms;

namespace Bioss.Ultrasound.UI.Controls
{
    public partial class ErrorView : ContentView
    {
        public ErrorView()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty TextProperty = BindableProperty.Create(
                                                    nameof(Text),
                                                    typeof(string),
                                                    typeof(ErrorView),
                                                    string.Empty,
                                                    BindingMode.OneWay,
                                                    propertyChanged: TextPropertyChanged);

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void TextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (ErrorView)bindable;
            var title = (string)newValue;
            control.TextLabel.Text = title;
        }
    }
}
