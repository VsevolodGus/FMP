using Bioss.Ultrasound.Core.Ble.Models;
using Bioss.Ultrasound.Core.Domain.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bioss.Ultrasound.Maui.ViewModels;

public partial class MainViewModel
{
    // Сюда выносишь ВСЁ, что про запись.
    // Графики пока можно не трогать: оставь TODO/заглушки.

    [ObservableProperty] private bool isRecording;
    [ObservableProperty] private byte fhr;
    [ObservableProperty] private byte toco;
    [ObservableProperty] private byte batteryLevel;

    private Record? _record;

    private void InitRecordingPart()
    {
        // Подписки на события и init, относящиеся к записи
        _device.NewPackage += OnNewPackage;

        // Если пока не нужен звук/графики — можно временно отключить:
        _pcmPlayer.Init();
        _pcmPlayer.Start();
    }

    private Task OnConnectionStateChangedAsync(bool connected)
    {
        // Тут решаешь, что делать с записью при дисконнекте
        if (!connected && IsRecording)
        {
            // TODO: сохранить запись/остановить
            IsRecording = false;
        }

        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task StartRecordingAsync()
    {
        if (IsRecording)
            return Task.CompletedTask;

        IsRecording = true;
        _record = new Record
        {
            StartTime = DateTime.Now,
            Biometric = new Biometric(),
            DeviceSerialNumber = _device.Name
        };

        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task StopRecordingAsync()
    {
        if (!IsRecording)
            return;

        IsRecording = false;

        var rec = _record;
        _record = null;

        if (rec is null)
            return;

        rec.StopTime = DateTime.Now;

        try
        {
            await _repository.InsertAsync(rec);
        }
        catch (Exception ex)
        {
            _logger.Log($"StopRecordingAsync save error: {ex}");
        }
    }

    private void OnNewPackage(object sender, Package package)
    {
        // Минимум на сейчас: просто обновляем значения для UI
        try
        {
            var fhrPkg = package.FHRPackage;
            if (fhrPkg != null)
            {
                Fhr = fhrPkg.Fhr1;
                Toco = fhrPkg.Toco;

                //если у тебя есть GetDigitBatteryLevel() — верни его сюда
                //BatteryLevel = fhrPkg.Status2.BatteryLevel.GetDigitBatteryLevel();
            }

            // TODO позже: графики/таймеры/PCM/criteria
        }
        catch (Exception ex)
        {
            _logger.Log($"OnNewPackage error: {ex}");
        }
    }
}