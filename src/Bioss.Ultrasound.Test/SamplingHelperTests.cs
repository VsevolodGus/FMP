using Bioss.Ultrasound.Services;

namespace Bioss.Ultrasound.Test;

public class SignalSamplerTests
{
    #region CreateEmptySamplingArray
    [Theory]
    [InlineData(1, 16, 16)]    // 1 сек × 16 Гц = 16
    [InlineData(10, 16, 160)]  // 10 сек × 16 Гц = 160
    [InlineData(0.5, 16, 8)]   // 0.5 сек × 16 Гц = 8
    [InlineData(1, 4, 4)]      // 1 сек × 4 Гц = 4
    [InlineData(1, 1, 1)]      // 1 сек × 1 Гц = 1
    [InlineData(2.3, 10, 23)]  // 2.3 сек × 10 Гц = 23 (Math.Ceiling)
    public void CreateEmptySamplingArray_ValidParameters_ReturnsCorrectLength(
        double seconds, int targetFrequency, int expectedLength)
    {
        // Arrange
        var duration = TimeSpan.FromSeconds(seconds);

        // Act
        var result = SignalSampler.CreateEmptySamplingArray<int>(duration, targetFrequency);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedLength, result.Length);
        Assert.All(result, x => Assert.Equal(0, x)); // default(int) = 0
    }

    [Theory]
    [InlineData(0, 16, 0)]     // 0 сек × 16 Гц = 0
    [InlineData(0, 1, 0)]      // 0 сек × 1 Гц = 0
    public void CreateEmptySamplingArray_ZeroDuration_ReturnsEmptyArray(
        double seconds, int targetFrequency, int expectedLength)
    {
        // Arrange
        var duration = TimeSpan.FromSeconds(seconds);

        // Act
        var result = SignalSampler.CreateEmptySamplingArray<string>(duration, targetFrequency);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedLength, result.Length);
        Assert.All(result, x => Assert.Null(x)); // default(string) = null
    }

    [Fact]
    public void CreateEmptySamplingArray_LongDuration_ReturnsCorrectLength()
    {
        // Arrange: 30 минут при 16 Гц
        var duration = TimeSpan.FromMinutes(30); // 1800 секунд
        int targetFrequency = 16;
        int expectedLength = 1800 * 16; // 28,800

        // Act
        var result = SignalSampler.CreateEmptySamplingArray<double>(duration, targetFrequency);

        // Assert
        Assert.Equal(expectedLength, result.Length);
        Assert.All(result, x => Assert.Equal(0.0, x)); // default(double) = 0.0
    }

    [Fact]
    public void CreateEmptySamplingArray_WithMaxValues_DoesNotThrow()
    {
        // Arrange: максимальные значения, которые не вызывают переполнение
        var duration = TimeSpan.FromHours(1); // 3600 секунд
        int targetFrequency = 1000; // 1 кГц
        int expectedLength = 3_600_000; // 3600 × 1000

        // Act
        var result = SignalSampler.CreateEmptySamplingArray<byte>(duration, targetFrequency);

        // Assert
        Assert.Equal(expectedLength, result.Length);
    }

    [Theory]
    [InlineData(-1)]   // отрицательная частота
    [InlineData(0)]    // нулевая частота
    [InlineData(-100)] // сильно отрицательная
    public void CreateEmptySamplingArray_InvalidTargetFrequency_ThrowsArgumentException(int invalidFrequency)
    {
        // Arrange
        var duration = TimeSpan.FromSeconds(1);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            SignalSampler.CreateEmptySamplingArray<int>(duration, invalidFrequency));

        Assert.Contains("targetFrequency", ex.Message);
    }

    [Fact]
    public void CreateEmptySamplingArray_VeryLargeDuration_CausesOverflow()
    {
        // Arrange: значения, которые вызовут переполнение при вычислении
        var duration = TimeSpan.FromDays(365); // 1 год
        int targetFrequency = 100_000; // 100 кГц

        // Act & Assert: ожидаем OverflowException или ArgumentException
        Assert.ThrowsAny<Exception>(() =>
            SignalSampler.CreateEmptySamplingArray<int>(duration, targetFrequency));
    }
    #endregion

    #region CalculateIndex
    [Theory]
    [InlineData("2024-01-01 10:00:00", "2024-01-01 10:00:00", 1_000_000, 0)]      // 0 тиков разницы
    [InlineData("2024-01-01 10:00:00", "2024-01-01 10:00:01", 1_000_000, 10)]   // 1 сек = 10 млн тиков, интервал 1 млн
    [InlineData("2024-01-01 10:00:00", "2024-01-01 10:00:00.500", 5_000_000, 1)] // 0.5 сек = 5 млн тиков, интервал 5 млн
    [InlineData("2024-01-01 10:00:00", "2024-01-01 10:00:02.300", 1_000_000, 23)] // 2.3 сек = 23 млн тиков
    public void CalculateIndex_ValidTimes_ReturnsCorrectIndex(
        string startDateStr, string currentDateStr, long ticksPerSample, long expectedIndex)
    {
        // Arrange
        var startDate = DateTime.Parse(startDateStr);
        var currentDate = DateTime.Parse(currentDateStr);

        // Act
        long index = SignalSampler.CalculateIndex(startDate, currentDate, ticksPerSample);

        // Assert
        Assert.Equal(expectedIndex, index);
    }

    [Fact]
    public void CalculateIndex_CurrentDateBeforeStart_ThrowsArgumentException()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1, 10, 0, 0);
        var currentDate = startDate.AddSeconds(-1); // 1 секунда раньше
        long ticksPerSample = 1_000_000; // 0.1 секунды

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            SignalSampler.CalculateIndex(startDate, currentDate, ticksPerSample));

        Assert.Contains("раньше", ex.Message);
        Assert.Contains(nameof(startDate), ex.Message);
        Assert.Contains(nameof(currentDate), ex.Message);
    }

    [Theory]
    [InlineData(0)]      // нулевой интервал
    [InlineData(-1)]     // отрицательный интервал
    [InlineData(-1000)]  // сильно отрицательный
    public void CalculateIndex_InvalidTicksPerSample_ThrowsException(long invalidTicksPerSample)
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1, 10, 0, 0);
        var currentDate = startDate.AddSeconds(1);

        // Act & Assert
        if (invalidTicksPerSample <= 0)
        {
            // DivisionByZeroException или OverflowException
            Assert.ThrowsAny<Exception>(() =>
                SignalSampler.CalculateIndex(startDate, currentDate, invalidTicksPerSample));
        }
    }

    [Fact]
    public void CalculateIndex_MaxDateTimeValues_WorksCorrectly()
    {
        // Arrange
        var startDate = DateTime.MinValue;
        var currentDate = DateTime.MaxValue;
        long ticksPerSample = TimeSpan.TicksPerDay; // 1 день в тиках

        // Act
        long index = SignalSampler.CalculateIndex(startDate, currentDate, ticksPerSample);

        // Assert: должно работать без переполнения
        Assert.True(index > 0);
    }

    [Fact]
    public void CalculateIndex_SameDateDifferentKind_WorksCorrectly()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var currentDate = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Local);
        long ticksPerSample = 1_000_000;

        // Act & Assert: разные Kind не должны влиять на вычисление
        long index = SignalSampler.CalculateIndex(startDate, currentDate, ticksPerSample);
        Assert.Equal(0, index);
    }
    #endregion

    #region FillSampler
    [Fact]
    public void FillSampler_ValidRange_FillsCorrectly()
    {
        // Arrange
        var arr = new int[10];
        long startIndex = 2;
        long endIndex = 7;
        int value = 42;

        // Act
        SignalSampler.FillSampler(arr, startIndex, endIndex, value);

        // Assert
        // Индексы 0-1 должны быть 0
        Assert.Equal(0, arr[0]);
        Assert.Equal(0, arr[1]);

        // Индексы 2-6 должны быть 42
        for (int i = 2; i < 7; i++)
            Assert.Equal(42, arr[i]);

        // Индексы 7-9 должны быть 0
        for (int i = 7; i < 10; i++)
            Assert.Equal(0, arr[i]);
    }

    [Fact]
    public void FillSampler_StartEqualsEnd_DoesNothing()
    {
        // Arrange
        var arr = new string[5] { "a", "b", "c", "d", "e" };
        long startIndex = 3;
        long endIndex = 3; // пустой диапазон
        string value = "X";

        // Act
        SignalSampler.FillSampler(arr, startIndex, endIndex, value);

        // Assert: массив не изменился
        Assert.Equal(new[] { "a", "b", "c", "d", "e" }, arr);
    }

    [Fact]
    public void FillSampler_FullArray_FillsEverything()
    {
        // Arrange
        var arr = new double[5];
        long startIndex = 0;
        long endIndex = arr.Length;
        double value = 3.14;

        // Act
        SignalSampler.FillSampler(arr, startIndex, endIndex, value);

        // Assert
        Assert.All(arr, x => Assert.Equal(3.14, x));
    }

    [Theory]
    [InlineData(-1, 3)]     // startIndex < 0
    [InlineData(0, 10)]     // endIndex > длина массива
    [InlineData(-2, -1)]    // оба отрицательные
    public void FillSampler_InvalidIndices_ThrowsIndexOutOfRangeException(
        long startIndex, long endIndex)
    {
        // Arrange
        var arr = new int[5];
        int value = 1;

        // Act & Assert
        Assert.Throws<IndexOutOfRangeException>(() =>
            SignalSampler.FillSampler(arr, startIndex, endIndex, value));
    }

    [Fact]
    public void FillSampler_NullArray_ThrowsArgumentNullException()
    {
        // Arrange
        int[] arr = null;
        long startIndex = 0;
        long endIndex = 5;
        int value = 1;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            SignalSampler.FillSampler(arr, startIndex, endIndex, value));
    }

    [Fact]
    public void FillSampler_WithReferenceTypes_FillsReferences()
    {
        // Arrange
        var obj = new object();
        var arr = new object[5];
        long startIndex = 1;
        long endIndex = 4;

        // Act
        SignalSampler.FillSampler(arr, startIndex, endIndex, obj);

        // Assert: все заполненные элементы ссылаются на один объект
        Assert.Same(obj, arr[1]);
        Assert.Same(obj, arr[2]);
        Assert.Same(obj, arr[3]);
        Assert.Null(arr[0]);
        Assert.Null(arr[4]);
    }

    [Fact]
    public void FillSampler_LargeArray_PerformanceTest()
    {
        // Arrange: большой массив для проверки производительности
        var arr = new int[1_000_000];
        long startIndex = 100_000;
        long endIndex = 900_000;
        int value = 777;

        // Act (измеряем время)
        var startTime = DateTime.Now;
        SignalSampler.FillSampler(arr, startIndex, endIndex, value);
        var elapsed = DateTime.Now - startTime;

        // Assert
        // Проверяем правильность заполнения
        for (int i = 0; i < 100_000; i++)
            Assert.Equal(0, arr[i]);

        for (int i = 100_000; i < 900_000; i++)
            Assert.Equal(777, arr[i]);

        for (int i = 900_000; i < 1_000_000; i++)
            Assert.Equal(0, arr[i]);

        // Проверяем, что заполнение заняло разумное время (< 100 мс)
        Assert.True(elapsed.TotalMilliseconds < 100,
            $"FillSampler занял {elapsed.TotalMilliseconds} мс, ожидалось < 100 мс");
    }
    #endregion
    // ========== ИНТЕГРАЦИОННЫЕ ТЕСТЫ ==========


    [Fact]
    public void IntegrationTest_AllMethodsWorkTogether()
    {
        // Arrange: полный сценарий использования всех методов
        var duration = TimeSpan.FromSeconds(2);
        int targetFrequency = 10; // 10 Гц
        var startDate = new DateTime(2024, 1, 1, 10, 0, 0);

        // 1. Создаем пустой массив
        var arr = SignalSampler.CreateEmptySamplingArray<int>(duration, targetFrequency);
        Assert.Equal(20, arr.Length); // 2 сек × 10 Гц

        // 2. Вычисляем индексы для нескольких измерений
        long ticksPerSample = TimeSpan.TicksPerSecond / targetFrequency; // 0.1 сек = 1_000_000 тиков

        var measurement1 = startDate.AddSeconds(0.3); // 0.3 сек
        var measurement2 = startDate.AddSeconds(1.5); // 1.5 сек

        long index1 = SignalSampler.CalculateIndex(startDate, measurement1, ticksPerSample);
        long index2 = SignalSampler.CalculateIndex(startDate, measurement2, ticksPerSample);

        Assert.Equal(3, index1);  // 0.3 сек / 0.1 сек = 3
        Assert.Equal(15, index2); // 1.5 сек / 0.1 сек = 15

        // 3. Записываем значения
        arr[index1] = 100;
        arr[index2] = 200;

        // 4. Заполняем промежутки (семплирование)
        if (index1 > 0)
            SignalSampler.FillSampler(arr, 0, index1, 100);

        if (index2 > index1 + 1)
            SignalSampler.FillSampler(arr, index1 + 1, index2, 100);

        // Проверяем результат
        for (int i = 0; i <= index1; i++)
            Assert.Equal(100, arr[i]);

        for (var i = index1 + 1; i < index2; i++)
            Assert.Equal(100, arr[i]);

        Assert.Equal(200, arr[index2]);
    }
}