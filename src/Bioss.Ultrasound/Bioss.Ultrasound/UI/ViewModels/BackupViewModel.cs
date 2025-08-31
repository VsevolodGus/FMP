using System;
using System.IO;
using System.Windows.Input;
using Bioss.Ultrasound.Data.Database;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Bioss.Ultrasound.UI.ViewModels
{
    public class BackupViewModel
    {
        private AppDatabase _database;

        public BackupViewModel(AppDatabase database)
        {
            _database = database;
        }

        public ICommand CreateBackupCommand => new Command(async a =>
        {
            var backupTo = Path.Combine(Path.GetTempPath(), $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.sqlite");
            File.Copy(_database.DbPath, backupTo);
            await Share.RequestAsync(new ShareFileRequest(new ShareFile(backupTo)));
        });
    }
}
