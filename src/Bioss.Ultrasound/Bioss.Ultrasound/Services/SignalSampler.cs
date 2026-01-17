using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Bioss.Ultrasound.Services
{
    public class SignalSampler
    {
        /// <summary>
        /// Конвертация в массив для CatAna
        /// [1           2           3           4]
        /// [1, 0, 0, 0, 2, 0, 0, 0, 3, 0, 0, 0, 4, 0, 0, 0]
        /// </summary>
        /// <typeparam name="TObject">объект для которого идет рассчет</typeparam>
        /// <typeparam name="TResultItem">результат рассчета</typeparam>
        /// <param name="duration">время записи</param>
        /// <param name="startDate">начало записи</param>
        /// <param name="items">элементы которые конвертируются для рассчета</param>
        /// <param name="getTime">метод получения времени из TObject</param>
        /// <param name="getValue">метод получения значения из TObject</param>
        /// <param name="targetFrequency">во сколько раз увеличится размер массива</param>
        /// <param name="fullSampling">нужно ли заполнять пустоты между значениями или нет. Заполненяет пустоты от предыдущего до текущего элемента, значением текущего элемента</param>
        /// <returns></returns>
        [Description("Метод может использоваться только для увеличения нового массива")]
        public static TResultItem[] Sampling<TObject, TResultItem>(TimeSpan duration,
            DateTime startDate,
            ICollection<TObject> items,
            Func<TObject, DateTime> getTime,
            Func<TObject, TResultItem> getValue,
            int targetFrequency,
            bool fullSampling = false)
        {
            if (duration <= TimeSpan.Zero)
                throw new ArgumentException("Duration must be positive", nameof(duration));

            if (getTime == null)
                throw new ArgumentNullException(nameof(getTime));

            if (getValue == null)
                throw new ArgumentNullException(nameof(getValue));

            if (items is null)
                throw new ArgumentNullException($"{nameof(items)}");

            if (targetFrequency < 1)
                throw new ArgumentException($"The parametr \"{nameof(targetFrequency)}\" must be greater than 0");

            const int countMsInSeconds = 1000;
            var signalIntervalMs = (double)countMsInSeconds / targetFrequency;
            
            var totalSeconds = duration.TotalSeconds;
            var arrayLength = (long)Math.Ceiling(totalSeconds * targetFrequency);

            var result = new TResultItem[arrayLength];
            var lastIndex = 0L;

            foreach (var item in items)
            {
                var currentDate = getTime(item);
                TimeSpan offset = currentDate - startDate;
                if (offset < TimeSpan.Zero)
                    throw new ArgumentException($"Один из элементов измерился раньше, чем начало измерения. {nameof(startDate)}: {startDate}. {nameof(currentDate)}: {currentDate}");

                double timeMsFromStart = offset.TotalMilliseconds;
                var index = (long)(timeMsFromStart / signalIntervalMs);

                if (index == arrayLength)
                    index = arrayLength - 1;
                else if (index < 0 || index >= arrayLength)
                    continue;

                var currentValue = getValue(item);
                result[index] = currentValue;


                if (!fullSampling)
                    continue;

                for (var i = lastIndex + 1; i < index; i++)
                    result[i] = result[lastIndex];
                lastIndex = index;
            }

            if (fullSampling)
            {
                for (var i = lastIndex + 1; i < result.Length; i++)
                    result[i] = result[lastIndex];
            }

            return result;
        }
    }
}
