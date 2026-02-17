using Bioss.Ultrasound.Core.Domain.Models;
using Bioss.Ultrasound.Core.Repository.Abstracts;
using Bioss.Ultrasound.Core.Services.Logging;
using Bioss.Ultrasound.Core.Services.Logging.Abstracts;
using Bioss.Ultrasound.Maui.Navigation;
using Bioss.Ultrasound.Maui.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Bioss.Ultrasound.Maui.ViewModels;

public partial class RecordsViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IRepository _repository;
    private readonly ILogger _logger;

    private bool _isFirstLoading = false;

    private Record _selectedRecord;

    public RecordsViewModel(
        ILogger logger,
        INavigationService navigationService,
        IRepository repository)
    {
        _logger = logger;
        _repository = repository;
        _navigationService = navigationService;
        _repository.NewItem += OnNewFile;
        _repository.ItemDelated += OmItemDelated;
    }

    private async void OnNewFile(object sender, long id)
    {
        //  load single file by fileName
        var record = await _repository.Get(id);
        Records.Insert(0, record);
    }

    private void OmItemDelated(object sender, long id)
    {
        Records.Remove(Records.FirstOrDefault(a => a.Id == id));
    }

    public ObservableCollection<Record> Records { get; } = new ObservableCollection<Record>();

    public Record SelectedRecord
    {
        get => _selectedRecord;
        set => SetProperty(ref _selectedRecord, value);
    }

    [RelayCommand]
    public async Task SelectedRecordCommand()
    {
        if (SelectedRecord == null)
            return;

        //var record = SelectedRecord;
        //await _navigationService.NavigateToAsync<RecordPage>(); // todo открывать именно ту запись, что нужна

        SelectedRecord = null;
    }

    /// <summary>
    /// AppearingCommand вызывается каждый раз при выборе вкладки.
    /// Данные загружаются один раз, дальше актуализируются по событиям
    /// </summary>
    /// 
    [RelayCommand]
    public async Task AppearingCommand()
    {
        if (_isFirstLoading)
            return;
        _isFirstLoading = true;

        try
        {
            var records = await _repository.RecordsAsync();
            foreach (var record in records)
                Records.Add(record);
        }
        catch (Exception ex)
        {
            _logger.Log($"Error when uploading records: {ex}", ServerLogLevel.CriticalFunctionalityError);
        }
    }
}
