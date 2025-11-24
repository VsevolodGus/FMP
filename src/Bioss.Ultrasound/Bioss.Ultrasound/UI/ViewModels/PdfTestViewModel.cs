using Acr.UserDialogs;
using Bioss.Ultrasound.Repository.Abstracts;
using Bioss.Ultrasound.Tools.PdfTests;
using Libs.DI.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Bioss.Ultrasound.UI.ViewModels
{
    public class PDFTestViewModel : ViewModelBase
    {
        private readonly IRepository _repository;
        private readonly IUserDialogs _dialogs;

        public PDFTestViewModel(IRepository repository, IUserDialogs dialogs)
        {
            _repository = repository;
            _dialogs = dialogs;
        }

        public List<IPdfTest> Tests
        {
            get => _tests;
            set => SetProperty(ref _tests, value);
        }
        private List<IPdfTest> _tests;

        public IPdfTest SelectedTest
        {
            get => _selectedTest;
            set => SetProperty(ref _selectedTest, value);
        }
        private IPdfTest _selectedTest;

        public ICommand AppearingCommand => new Command(async a =>
        {
            var records = await _repository.RecordsAsync();
            var record = records.First();
            Tests = new List<IPdfTest>
            {
                new MigraDocTest(),
                new MigraDocPlusPdfSharp(),
                new PdfSharpTest(),
                new PdfSharpDrawingTest(),
                new PdfSharpOxyPlotTest(record),
                new PdfSharpAndOxyPlotPagesTest(record),
            };
            SelectedTest = Tests[0];
        });


        public ICommand GeneratePDFCommand => new Command(async a =>
        {
            if (SelectedTest is null)
                return;

            var fileName = Path.Combine(Path.GetTempPath(), "ChartPdf.pdf");

            try
            {
                SelectedTest.CreatePdfFile(fileName);
            }
            catch (Exception e)
            {
                _dialogs.Alert(e.StackTrace, e.Message);
                return;
            }

            //  Display in system apps
            //await Launcher.OpenAsync(new OpenFileRequest
            //{
            //    File = new ReadOnlyFile(fileName)
            //});

            // share
            var files = new List<ShareFile>();
            files.Add(new ShareFile(fileName));
            await Share.RequestAsync(new ShareMultipleFilesRequest
            {
                Title = "PDF Test",
                Files = files
            });
        });
    }
}
