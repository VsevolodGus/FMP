using Bioss.Ultrasound.Domain.Models;
using Bioss.Ultrasound.Repository.Abstracts;
using Bioss.Ultrasound.UI.Pages;
using Libs.DI.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace Bioss.Ultrasound.UI.ViewModels
{
    public class RecordsViewModel : ViewModelBase
    {
        private readonly INavigation _navigation;
        private readonly IRepository _repository;

        private bool _isFirstLoading = false;

        private Record _selectedRecord;

        public RecordsViewModel(INavigation navigation, IRepository repository)
        {
            _navigation = navigation;
            _repository = repository;
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

        public ICommand SelectedRecordCommand => new AsyncCommand(async () =>
        {
            if (SelectedRecord == null)
                return;

            var record = SelectedRecord;
            await _navigation.PushAsync(new RecordPage(record));
 
            SelectedRecord = null;
        }, allowsMultipleExecutions: false);

        public ICommand AppearingCommand => new Command(async a =>
        {
            //  AppearingCommand вызывается каждый раз при выборе вкладки.
            //  Нам достаточно один раз загрузить данные
            if (_isFirstLoading)
                return;
            _isFirstLoading = true;

            var records = await _repository.RecordsAsync();
            foreach (var record in records)
                Records.Add(record);
        });
    }
}
