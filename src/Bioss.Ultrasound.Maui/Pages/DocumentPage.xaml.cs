using Bioss.Ultrasound.Maui.ViewModels;

namespace Bioss.Ultrasound.Maui.Pages;

/// <summary>
/// Страница для отображения политки конфиденциальности
/// </summary>
public partial class DocumentPage : ContentPage
{
    public DocumentPage(DocumentViewModel documentViewModel)
    {
        InitializeComponent();
        BindingContext = documentViewModel;
    }
}
