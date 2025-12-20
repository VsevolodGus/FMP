using System;
using Xamarin.Essentials;

namespace Bioss.Ultrasound.Services
{
    public class InfoSettingsService
    {
        private const string NAME = "InfoSettings";

        private const string ORGANIZATION = "organization";
        private const string DOCTOR = "doctor";

        private const string PERSONAL_DEVICE = "isPersonalDevice";
        private const string PATIENT = "patient";
        private const string PATIENT_BIRTHDAY = "patientBirthday";
        private const string PREGNANCY_START = "pregnancyStart";
        private const string PREGNANCY_NUMBER = "pregnancyNumber";
        private const string PREGNANCY_WEEK = "pregnancyWeek";
        private const string PREGNANCY_DAY = "pregnancyDay";
        private const string PDF_RECORDING_SPEED = "pdfRecordingSpeed";

        public string Organization
        {
            get => Preferences.Get(ORGANIZATION, "", NAME);
            set => Preferences.Set(ORGANIZATION, value, NAME);
        }

        public string Doctor
        {
            get => Preferences.Get(DOCTOR, "", NAME);
            set => Preferences.Set(DOCTOR, value, NAME);
        }

        public bool IsPersonalDevice
        {
            get => Preferences.Get(PERSONAL_DEVICE, false, NAME);
            set => Preferences.Set(PERSONAL_DEVICE, value, NAME);
        }

        public string Patient
        {
            get => Preferences.Get(PATIENT, "", NAME);
            set => Preferences.Set(PATIENT, value, NAME);
        }

        public DateTime? Birthday
        {
            get => GetNullable(PATIENT_BIRTHDAY, NAME);
            set => SetNullable(PATIENT_BIRTHDAY, value, NAME);
        }

        public int PregnancyWeek
        {
            get => Preferences.Get(PREGNANCY_WEEK, Constants.DefaultCountWeek, NAME);
            set
            {
                Preferences.Set(PREGNANCY_WEEK, value, NAME);
            }
        }

        public int PregnancyDay
        {
            get => Preferences.Get(PREGNANCY_DAY, Constants.DefaultCountDay, NAME);
            set => Preferences.Set(PREGNANCY_DAY, value, NAME);
        }

        public int PregnancyNumber
        {
            get => Preferences.Get(PREGNANCY_NUMBER, 1, NAME);
            set => Preferences.Set(PREGNANCY_NUMBER, value, NAME);
        }

        //  Скоростm записи КТГ для формирования отчета pdf: 1 см/мин, 2 см/мин, 3 см/мин.
        public int PdfRecordingSpeed
        {
            get => Preferences.Get(PDF_RECORDING_SPEED, 1, NAME);
            set => Preferences.Set(PDF_RECORDING_SPEED, value, NAME);
        }

        private DateTime? GetNullable(string key, string sharedName)
        {
            if (Preferences.ContainsKey(key, sharedName))
                return Preferences.Get(key, new DateTime(), sharedName);
            else
                return null;
        }

        private void SetNullable(string key, DateTime? value, string sharedName)
        {
            if (value.HasValue)
                Preferences.Set(key, value.Value, sharedName);
            else
                Preferences.Remove(key, sharedName);
        }
    }
}
