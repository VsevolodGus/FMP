namespace Bioss.Ultrasound.Maui.Navigation;

public interface INavigationService
{
    Task NavigateToAsync<TPage>() where TPage : Page;
    Task NavigateModalAsync<TPage>() where TPage : Page;
    Task CloseModelAsync();
}


public class NavigationService : INavigationService
{
    private readonly IServiceProvider _services;
    private INavigation Navigation => Application.Current?.MainPage?.Navigation
        ?? throw new InvalidOperationException("Navigation not initialized");
    public NavigationService(IServiceProvider services)
    {
        _services = services;
    }

    public async Task NavigateToAsync<TPage>() where TPage : Page
    {
        var page = _services.GetRequiredService<TPage>();
        await Navigation.PushAsync(page);
    }

    public async Task NavigateModalAsync<TPage>() where TPage : Page
    {
        var page = _services.GetRequiredService<TPage>();
        await Navigation.PushModalAsync(page);
    }

    public async Task CloseModelAsync()
    {
        await Navigation.PopModalAsync();
    }
}
