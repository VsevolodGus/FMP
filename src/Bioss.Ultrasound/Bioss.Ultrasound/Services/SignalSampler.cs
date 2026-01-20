using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Bioss.Ultrasound.Services
{
    public class SignalSampler
    {
        /// <summary>
        /// Конвертация в массив для CatAna
        /// [1           2           3           4]
        /// [1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4]
        /// </summary>
        /// <typeparam name="TObject">объект для которого идет рассчет</typeparam>
        /// <typeparam name="TResultItem">результат рассчета</typeparam>
        /// <param name="duration">время записи</param>
        /// <param name="startDate">начало записи</param>
        /// <param name="items">НЕ ДОЛЖНО БЫТЬ ДУБЛИКАТОВ И ДОЛЖНО БЫТЬ ОТСОРТИРОВАНО ПО getTime</param>
        /// <param name="getTime">метод получения времени из TObject</param>
        /// <param name="getValue">метод получения значения из TObject</param>
        /// <param name="targetFrequency">во сколько раз увеличится размер массива</param>
        /// <returns></returns>
        [Description("Метод может использоваться только для увеличения нового массива")]
        public static TResultItem[] Sampling<TObject, TResultItem>(in TimeSpan duration,
            in DateTime startDate,
            IReadOnlyList<TObject> items,
            in int itemsCount,
            Func<TObject, DateTime> getTime,
            Func<TObject, TResultItem> getValue,
            in int targetFrequency)
        {
            ValidParameters(duration, items, getTime, getValue, targetFrequency);

            var result = CreateEmptySamplingArray<TResultItem>(duration, targetFrequency);
            var ticksPerSample = TimeSpan.TicksPerSecond / targetFrequency;

            for (var i = 0; i < itemsCount; i++)
            {
                var item = items[i];
                var currentDate = getTime(item);
                var index = CalculateIndex(startDate, currentDate, ticksPerSample);

                if (index == result.Length)
                    index = result.Length - 1;
                else if (index < 0 || index >= result.Length)
                    continue;

                result[index] = getValue(item);
            }

            return result;
        }


        /// <summary>
        /// Конвертация в массив для CatAna
        /// [1           2           3           4]
        /// [1, 0, 0, 0, 2, 0, 0, 0, 3, 0, 0, 0, 4, 0, 0, 0]
        /// </summary>
        /// <typeparam name="TObject">объект для которого идет рассчет</typeparam>
        /// <typeparam name="TResultItem">результат рассчета</typeparam>
        /// <param name="duration">время записи</param>
        /// <param name="startDate">начало записи</param>
        /// <param name="items">НЕ ДОЛЖНО БЫТЬ ДУБЛИКАТОВ И ДОЛЖНО БЫТЬ ОТСОРТИРОВАНО ПО getTime</param>
        /// <param name="getTime">метод получения времени из TObject</param>
        /// <param name="getValue">метод получения значения из TObject</param>
        /// <param name="targetFrequency">во сколько раз увеличится размер массива</param>
        /// <param name="fullSampling">нужно ли заполнять пустоты между значениями или нет. Заполненяет пустоты от предыдущего до текущего элемента, значением текущего элемента</param>
        /// <returns></returns>
        [Description("Метод может использоваться только для увеличения нового массива")]
        public static TResultItem[] FullSampling<TObject, TResultItem>(in TimeSpan duration,
            in DateTime startDate,
            IReadOnlyList<TObject> items,
            in int itemsCount,
            Func<TObject, DateTime> getTime,
            Func<TObject, TResultItem> getValue,
            in int targetFrequency)
        {
            ValidParameters(duration, items, getTime, getValue, targetFrequency);

            var result = CreateEmptySamplingArray<TResultItem>(duration, targetFrequency);

            var lastIndex = 0L;
            var ticksPerSample = TimeSpan.TicksPerSecond / targetFrequency;
            for(var i = 0; i < itemsCount; i++)
            {
                var item = items[i];
                var currentDate = getTime(item);
                var index = CalculateIndex(startDate, currentDate, ticksPerSample);

                if (index == result.Length)
                    index = result.Length - 1;
                else if (index < 0 || index >= result.Length)
                    continue;

                result[index] = getValue(item);
                FillSampler(result, (int)lastIndex + 1, index, result[lastIndex]);
                lastIndex = index;
            }

            FillSampler(result, (int)lastIndex + 1, result.Length, result[lastIndex]);
            return result;
        }

       
        private static void ValidParameters<T1, T2>(
            in TimeSpan duration,
            IEnumerable<T1> items,
            Func<T1, DateTime> getTime,
            Func<T1, T2> getValue,
            in int targetFrequency)
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
        }

        public static T[] CreateEmptySamplingArray<T>(in TimeSpan duration, in int targetFrequency)
        {
            if (targetFrequency <= 0)
                throw new ArgumentException($"{nameof(targetFrequency)} должен быть больше 0");

            var totalSeconds = duration.TotalSeconds;
            var arrayLength = (long)Math.Floor(totalSeconds * targetFrequency);
            return new T[arrayLength];

        }
        public static long CalculateIndex(in DateTime startDate, in DateTime currentDate, in long ticksPerSample)
        {
            if (ticksPerSample <= 0)
                throw new ArgumentException($"{nameof(ticksPerSample)} должен быть больше 0");

            TimeSpan offset = currentDate - startDate;
            if (offset < TimeSpan.Zero)
                throw new ArgumentException($"Один из элементов измерился раньше, чем начало измерения. {nameof(startDate)}: {startDate}. {nameof(currentDate)}: {currentDate}");

            return offset.Ticks / ticksPerSample;
        }
        public static void FillSampler<T>(in T[] arr, in long startIndex, in long endIndex, in T value)
        {
            if (arr is null)
                throw new ArgumentNullException(nameof(arr));

            if(startIndex < 0)
                throw new IndexOutOfRangeException($"{nameof(startIndex)} must positive");

            if (endIndex < 0)
                throw new IndexOutOfRangeException($"{nameof(startIndex)} must positive");

            for (var i = startIndex; i < endIndex; i++)
                arr[i] = value;
        }

    }
}
