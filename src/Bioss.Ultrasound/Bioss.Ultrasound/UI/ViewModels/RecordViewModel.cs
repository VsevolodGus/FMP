using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using Acr.UserDialogs;
using Bioss.Ultrasound.Domain.Models;
using Bioss.Ultrasound.Domain.Plotting;
using Bioss.Ultrasound.Repository.Abstracts;
using Bioss.Ultrasound.Resources.Localization;
using Bioss.Ultrasound.Services;
using Bioss.Ultrasound.UI.Helpers;
using Bioss.Ultrasound.UI.Popups;
using Libs.DI.ViewModels;
using OxyPlot;
using OxyPlot.Axes;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;
using System.Diagnostics;
using System.Linq;
using Bioss.Ultrasound.Data.Database.Entities.Enums;
using OxyPlot.Annotations;
using System;
using Rg.Plugins.Popup.Services;
using Bioss.Ultrasound.Services.Logging.Abstracts;
using Bioss.Ultrasound.Services.Abstracts;
using Bioss.Ultrasound.Services.Logging;

namespace Bioss.Ultrasound.UI.ViewModels
{
    public class RecordViewModel : ViewModelBase
    {
        private readonly INavigation _navigation;
        private readonly IUserDialogs _dialogs;
        private readonly ChartDrawer _chartDrawer;
        private readonly AppSettingsService _appSettings;
        private readonly InfoSettingsService _infoService;
        private readonly IRepository _repository;
        private readonly Record _record;
        private readonly ILogger _logger;
        private readonly IPdfGenerator _pdfGenerator;

        private readonly PlottingHelper _plottingHelper = new PlottingHelper();

        private bool _isFirstAppearing = true;

        private PlotModel _plotModel;
        private PointAnnotation _fhrAnnotation;
        private PointAnnotation _tocoAnnotation;

        public RecordViewModel(INavigation navigation, 
            IUserDialogs dialogs, 
            AppSettingsService appSettings, 
            InfoSettingsService infoService, 
            IRepository repository, 
            ILogger logger,
            Record record,
            IPdfGenerator pdfGenerator)
        {
            _navigation = navigation;
            _dialogs = dialogs;

            _chartDrawer = new ChartDrawer(_plottingHelper, true);
            _appSettings = appSettings;
            _infoService = infoService;
            _repository = repository;
            _logger = logger;
            _record = record;
            _pdfGenerator = pdfGenerator;

            _plottingHelper.Scale = _appSettings.ChartXScaleSeconds;

            PlotModel = _chartDrawer.Model;
            _chartDrawer.ResetFhrMinMax(_appSettings.ChartYMinimum, _appSettings.ChartYMaximum);
            Title = $"{_record.StartTime}";

            PlotModel.Updated += PlotModel_Updated;

            //
            _fhrAnnotation = CreatePointAnnotation(PlotModel, ChartDrawer.KEY_FHR);
            _tocoAnnotation = CreatePointAnnotation(PlotModel, ChartDrawer.KEY_TOCO);
        }

        public string Title { get; set; }

        public PlotModel PlotModel
        {
            get => _plotModel;
            set => SetProperty(ref _plotModel, value);
        }

        private byte _heartRate;
        public byte HeartRate
        {
            get => _heartRate;
            set => SetProperty(ref _heartRate, value);
        }

        private byte _toco;
        public byte Toco
        {
            get => _toco;
            set => SetProperty(ref _toco, value);
        }

        private int _fetalMovements;
        public int FetalMovements
        {
            get => _fetalMovements;
            set => SetProperty(ref _fetalMovements, value);
        }

        public double LossPercentage => Math.Round(_record.LossPercentage * 100, 1);

        public ICommand AppearingCommand => new Command(a =>
        {
            if (!_isFirstAppearing)
                return;
            _isFirstAppearing = false;

            _chartDrawer.Fill(_record);
            FetalMovements = _record.Events.Count(a => a.Event == Events.FetalMovement);
        });

        public ICommand DeleteCommand => new AsyncCommand(async () =>
        {
            try
            {
                if (!await _dialogs.ConfirmAsync(AppStrings.Record_DialogDeleteMessage, AppStrings.Record_DialogDeleteTitle, AppStrings.Yes, AppStrings.Cancel))
                    return;

                await _repository.DeleteAsync(_record);
                _logger.Log("Delete record");

            }
            catch (Exception ex)
            {
                _logger.Log($"Error when deleted record: {ex}", ServerLogLevel.CriticalFunctionalityError);
            }
            await _navigation.PopAsync();

        }, allowsMultipleExecutions: false);

        public ICommand ExportToPdfCommand => new AsyncCommand(async () =>
        {
            var recoringStartTime = _record.StartTime;
            var fileName = $"{_record.DeviceSerialNumber}_{_infoService.PregnancyWeek}({_infoService.PregnancyDay})_{recoringStartTime:yyyy-MM-dd_HH-mm}.pdf";
            var filePath = Path.Combine(Path.GetTempPath(), fileName);
            try
            {
                _pdfGenerator.GenerateToFile(filePath, _record);

                var files = new List<ShareFile>
                {
                    new ShareFile(filePath)
                };
                await Share.RequestAsync(new ShareMultipleFilesRequest
                {
                    Title = $"Fetal Monitor Report - {recoringStartTime:g}",
                    Files = files
                });
            }
            catch (Exception ex)
            {
                _logger.Log($"Error when generating the report: {fileName}. Error: {ex}", ServerLogLevel.CriticalFunctionalityError);
            }
        }, allowsMultipleExecutions: false);

        public ICommand BiometricCommand => new AsyncCommand(async () =>
        {
            try
            {
                var popup = new BiometricPopup(_dialogs, _record.Biometric);
                await PopupNavigation.Instance.PushAsync(popup);
                var result = await popup.PopupClosedTask;

                if (result.Ok)
                {

                    var biom = result.Biometric;
                    await _repository.InsertOrUpdateAsync(biom);
                    _record.Biometric = biom;
                    _logger.Log("Updated the patient's indicators for the saved record");
                }
            }
            catch (Exception ex)
            {
                _logger.Log($"Error when trying to update patient data. Error: {ex}", ServerLogLevel.CriticalFunctionalityError);
            }
        }, allowsMultipleExecutions: false);

        private void PlotModel_Updated(object sender, EventArgs e)
        {
            //  Не позволяет проматывать график дальше минимума
            if (PlotModel.DefaultXAxis.ActualMinimum < PlotModel.DefaultXAxis.Minimum)
                _plottingHelper.ResetAxisWithMin(TimeSpanAxis.ToTimeSpan(0));

            //  Используем начало координатной сетки для получения значений в точке
            var xValue = PlotModel.DefaultXAxis.ActualMinimum;
            //  получим точку в данных ближайшую к текущей позиции
            var yValues = _chartDrawer.GetValue(xValue);
            Debug.WriteLine($"{xValue} {yValues.Fhr} {yValues.Toco}");
            HeartRate = yValues.Fhr;
            Toco = yValues.Toco;

            //
            _fhrAnnotation.Y = HeartRate;
            _fhrAnnotation.X = xValue;
            //
            _tocoAnnotation.Y = Toco;
            _tocoAnnotation.X = xValue;
        }

        private PointAnnotation CreatePointAnnotation(PlotModel model, string key)
        {
            var annotation = new PointAnnotation
            {
                Fill = OxyColors.Yellow,
                Stroke = OxyColors.Red,
                StrokeThickness = 2,
                YAxisKey = key
            };
            model.Annotations.Add(annotation);
            return annotation;
        }
    }
}
