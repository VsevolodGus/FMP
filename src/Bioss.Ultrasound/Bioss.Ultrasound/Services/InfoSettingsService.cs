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

        /// <summary>
        /// Организация которая проводит измерение
        /// </summary>
        public string Organization
        {
            get => Preferences.Get(ORGANIZATION, "", NAME);
            set => Preferences.Set(ORGANIZATION, value, NAME);
        }

        /// <summary>
        /// ФИО/данные доктора
        /// </summary>
        public string Doctor
        {
            get => Preferences.Get(DOCTOR, "", NAME);
            set => Preferences.Set(DOCTOR, value, NAME);
        }

        /// <summary>
        ///  Является ли приложенией персональным устройством
        /// </summary>
        public bool IsPersonalDevice
        {
            get => Preferences.Get(PERSONAL_DEVICE, false, NAME);
            set => Preferences.Set(PERSONAL_DEVICE, value, NAME);
        }

        /// <summary>
        /// Имя/ФИО пациента
        /// </summary>
        public string Patient
        {
            get => Preferences.Get(PATIENT, "", NAME);
            set => Preferences.Set(PATIENT, value, NAME);
        }

        /// <summary>
        /// Дата рождения пациента
        /// </summary>
        public DateTime? Birthday
        {
            get => GetNullable(PATIENT_BIRTHDAY, NAME);
            set => SetNullable(PATIENT_BIRTHDAY, value, NAME);
        }

        /// <summary>
        /// Неделя беременности, беременность указывается в виде неделя/день
        /// </summary>
        public int PregnancyWeek
        {
            get => Preferences.Get(PREGNANCY_WEEK, Constants.DefaultCountWeek, NAME);
            set
            {
                Preferences.Set(PREGNANCY_WEEK, value, NAME);
            }
        }

        /// <summary>
        /// День беременности, беременность указывается в виде неделя/день
        /// день беременности = N дней беременности % 7
        /// </summary>
        public int PregnancyDay
        {
            get => Preferences.Get(PREGNANCY_DAY, Constants.DefaultCountDay, NAME);
            set => Preferences.Set(PREGNANCY_DAY, value, NAME);
        }

        /// <summary>
        /// Номер беременность по счету
        /// </summary>
        public int PregnancyNumber
        {
            get => Preferences.Get(PREGNANCY_NUMBER, 1, NAME);
            set => Preferences.Set(PREGNANCY_NUMBER, value, NAME);
        }

        /// <summary>
        /// Скоростm записи КТГ для формирования отчета pdf: 1 см/мин, 2 см/мин, 3 см/мин.
        /// Масштаб графика в ПДФ отчете
        /// </summary>
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
