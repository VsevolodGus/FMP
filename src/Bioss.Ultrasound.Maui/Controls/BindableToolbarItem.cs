namespace Bioss.Ultrasound.Maui.Controls;

public class BindableToolbarItem : ToolbarItem
{
    public static readonly BindableProperty IsVisibleProperty =
        BindableProperty.Create(
            nameof(IsVisible),
            typeof(bool),
            typeof(BindableToolbarItem),
            true,
            BindingMode.TwoWay,
            propertyChanged: OnIsVisibleChanged);

    public bool IsVisible
    {
        get => (bool)GetValue(IsVisibleProperty);
        set => SetValue(IsVisibleProperty, value);
    }

    private static void OnIsVisibleChanged(BindableObject bindable, object oldvalue, object newvalue)
    {
        var item = bindable as BindableToolbarItem;

        if (item == null || item.Parent == null)
            return;

        // как было (ContentPage), только MAUI namespace
        var toolbarItems = ((ContentPage)item.Parent).ToolbarItems;

        if ((bool)newvalue && !toolbarItems.Contains(item))
        {
            MainThread.BeginInvokeOnMainThread(() => { toolbarItems.Add(item); });
        }
        else if (!(bool)newvalue && toolbarItems.Contains(item))
        {
            MainThread.BeginInvokeOnMainThread(() => { toolbarItems.Remove(item); });
        }
    }
}