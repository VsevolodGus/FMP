using Bioss.Ultrasound.Services;

namespace Bioss.Ultrasound.Test;

public class SamplingTest
{
    // Тестовый класс для данных
    public class TestDataItem
    {
        public DateTime Timestamp { get; set; }
        public int Value { get; set; }
    }

    #region ТЕСТЫ ВАЛИДАЦИИ
    [Fact]
    public void Sampling_NullItems_ThrowsArgumentNullException()
    {
        // Arrange
        var duration = TimeSpan.FromSeconds(1);
        var startDate = DateTime.Now;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            SignalSampler.Sampling<TestDataItem, int>(
                duration, startDate, null,
                x => x.Timestamp, x => x.Value,
                targetFrequency: 16));
    }

    [Fact]
    public void Sampling_NullGetTime_ThrowsArgumentNullException()
    {
        // Arrange
        var duration = TimeSpan.FromSeconds(1);
        var startDate = DateTime.Now;
        var items = Array.Empty<TestDataItem>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            SignalSampler.Sampling(
                duration, startDate, items,
                null, // getTime is null
                x => x.Value,
                targetFrequency: 16));
    }

    [Fact]
    public void Sampling_NullGetValue_ThrowsArgumentNullException()
    {
        // Arrange
        var duration = TimeSpan.FromSeconds(1);
        var startDate = DateTime.Now;
        var items = Array.Empty<TestDataItem>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            SignalSampler.Sampling<TestDataItem, int>(
                duration, startDate, items,
                x => x.Timestamp,
                null, // getValue is null
                targetFrequency: 16));
    }

    [Fact]
    public void Sampling_NegativeDuration_ThrowsArgumentException()
    {
        // Arrange
        var duration = TimeSpan.FromSeconds(-1);
        var startDate = DateTime.Now;
        var items = Array.Empty<TestDataItem>();

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            SignalSampler.Sampling(
                duration, startDate, items,
                x => x.Timestamp, x => x.Value,
                targetFrequency: 16));

        Assert.Contains("positive", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Sampling_InvalidTargetFrequency_ThrowsArgumentException(int invalidFrequency)
    {
        // Arrange
        var duration = TimeSpan.FromSeconds(1);
        var startDate = DateTime.Now;
        var items = Array.Empty<TestDataItem>();

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            SignalSampler.Sampling(
                duration, startDate, items,
                x => x.Timestamp, x => x.Value,
                targetFrequency: invalidFrequency));

        Assert.Contains("must be greater than 0", ex.Message);
    }

    [Fact]
    public void Sampling_ItemsOutOfTimeRange_AreIgnored()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1, 10, 0, 0);
        var duration = TimeSpan.FromSeconds(1); // Только 1 секунда

        var items = new[]
        {
            new TestDataItem { Timestamp = startDate.AddSeconds(-0.5), Value = -1 }, // До начала
            new TestDataItem { Timestamp = startDate.AddMilliseconds(0), Value = 0 }, // Начало
            new TestDataItem { Timestamp = startDate.AddMilliseconds(500), Value = 1 }, // Внутри
            new TestDataItem { Timestamp = startDate.AddSeconds(1.5), Value = 2 }, // После конца
            new TestDataItem { Timestamp = startDate.AddSeconds(2), Value = 3 }   // Еще дальше
        };

        // Act
        Assert.Throws<ArgumentException>(() =>
            SignalSampler.Sampling(
                duration,
                startDate,
                items,
                x => x.Timestamp,
                x => x.Value,
                targetFrequency: 16)
            );
    }
    #endregion


    #region ТЕСТЫ ОСНОВНОЙ ЛОГИКИ 
    [Theory]
    [InlineData(4, 20)] // 5 сек × 4 Гц = 20
    [InlineData(16, 80)] //5 сек × 16 Гц = 80
    [InlineData(100, 500)] // 5 сек × 100 Гц = 500
    public void Sampling_EmptyItems_ReturnsArrayOfCorrectLength(int targetFrequency, int exceptedResult)
    {
        // Arrange
        var duration = TimeSpan.FromSeconds(5);
        var startDate = new DateTime(2024, 1, 1, 10, 0, 0);
        var items = new List<TestDataItem>();

        // Act
        var result = SignalSampler.Sampling(
            duration, startDate, items,
            x => x.Timestamp, x => x.Value,
            targetFrequency);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(exceptedResult, result.Length); // 5 сек × targetFrequency Гц = exceptedResult
        Assert.All(result, x => Assert.Equal(0, x)); // default(int) = 0
    }

    [Theory]
    [InlineData(5, 16, 80)]
    [InlineData(10, 8, 80)]
    [InlineData(20, 4, 80)]
    [InlineData(10, 1, 10)]
    [InlineData(100, 32, 3200)]
    public void Sampling_DifferentFrequencies_CorrectArrayLength(int countSeconds, int targetFrequency, int expectedLength)
    {
        // Arrange
        var duration = TimeSpan.FromSeconds(countSeconds);
        var startDate = new DateTime(2024, 1, 1, 10, 0, 0);
        var items = new List<TestDataItem>();

        // Act
        var result = SignalSampler.Sampling(
            duration, startDate, items,
            x => x.Timestamp, x => x.Value,
            targetFrequency: targetFrequency);

        // Assert
        Assert.Equal(expectedLength, result.Length);
    }

    [Theory]
    [InlineData(0, 0, 16)]    // 0 мс → индекс 0
    [InlineData(63, 1, 16)]   // 63 мс → индекс 1 (63/62.5 ≈ 1....)
    [InlineData(125, 2, 16)]  // 125 мс → индекс 2
    [InlineData(250, 4, 16)]  // 250 мс → индекс 4
    [InlineData(500, 8, 16)]  // 500 мс → индекс 8
    [InlineData(100, 1, 10)]  // 100 мс при 10 Гц → индекс 1 (100/100 = 1)
    [InlineData(150, 0, 4)]   // 150 мс при 4 Гц → индекс 0 (150/250 = 0.6 → 0)
    public void Sampling_TimeToIndexMapping_Correct(int milliseconds, int expectedIndex, int targetFrequency)
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1, 10, 0, 0);
        var itemTime = startDate.AddMilliseconds(milliseconds);
        var items = new[] { new TestDataItem { Timestamp = itemTime, Value = 999 } };
        var duration = TimeSpan.FromSeconds(1);

        // Act
        var result = SignalSampler.Sampling(
            duration, startDate, items,
            x => x.Timestamp, x => x.Value,
            targetFrequency: targetFrequency);

        // Assert
        Assert.Equal(999, result[expectedIndex]);
    }

    [Fact]
    public void Sampling_MultipleItems_4HzTo16HzMapping()
    {
        // Arrange: 4 измерения в секунду (4 Гц) → 16 позиций в секунду
        var startDate = new DateTime(2024, 1, 1, 10, 0, 0);
        var items = new[]
        {
            new TestDataItem { Timestamp = startDate.AddMilliseconds(0), Value = 1 },
            new TestDataItem { Timestamp = startDate.AddMilliseconds(250), Value = 2 },
            new TestDataItem { Timestamp = startDate.AddMilliseconds(500), Value = 3 },
            new TestDataItem { Timestamp = startDate.AddMilliseconds(750), Value = 4 }
        };
        var duration = TimeSpan.FromSeconds(1);

        // Act
        var result = SignalSampler.Sampling(
            duration,
            startDate,
            items,
            x => x.Timestamp,
            x => x.Value,
            targetFrequency: 16);

        // Assert
        Assert.Equal(16, result.Length);
        Assert.Equal(1, result[0]);   // 0 мс → индекс 0
        Assert.Equal(2, result[4]);   // 250 мс → индекс 4 (250/62.5 = 4)
        Assert.Equal(3, result[8]);   // 500 мс → индекс 8
        Assert.Equal(4, result[12]);  // 750 мс → индекс 12

        // Между значениями должны быть 0
        for (int i = 0; i < 16; i++)
        {
            if (i != 0 && i != 4 && i != 8 && i != 12)
            {
                Assert.Equal(0, result[i]);
            }
        }
    }

    [Fact]
    public void Sampling_ItemsAtBoundary_CorrectlyHandled()
    {
        // Arrange: измерение точно на границе последнего интервала
        var startDate = new DateTime(2024, 1, 1, 10, 0, 0);
        var duration = TimeSpan.FromSeconds(1);
        var targetFrequency = 16;

        // 1000 мс при 16 Гц: интервал = 62.5 мс, 1000/62.5 = 16
        var boundaryTime = startDate.AddMilliseconds(1000);
        var items = new[] { new TestDataItem { Timestamp = boundaryTime, Value = 99 } };

        // Act
        var result = SignalSampler.Sampling(
            duration, startDate, items,
            x => x.Timestamp, x => x.Value,
            targetFrequency: targetFrequency);

        // Assert: index == arrayLength (16) → index = arrayLength - 1 (15)
        Assert.Equal(99, result[15]); // Последний элемент массива
    }
    #endregion


    #region ТЕСТЫ РЕЖИМА SAMPLER 
    /// <summary>
    /// [ 10 20 30 ]
    /// [ 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 ]
    /// </summary>
    [Fact]
    public void Sampling_WithSamplerTrue_FillsIntermediateValues()
    {
        const int value1 = 10;
        const int value2 = 20;
        const int value3 = 30;
        const int value4 = 40;

        // Arrange
        var startDate = new DateTime(2024, 1, 1, 10, 0, 0);
        var items = new[]
        {
            new TestDataItem { Timestamp = startDate.AddMilliseconds(0), Value = value1 },
            new TestDataItem { Timestamp = startDate.AddMilliseconds(250), Value = value2 },
            new TestDataItem { Timestamp = startDate.AddMilliseconds(500), Value = value3 },
            new TestDataItem { Timestamp = startDate.AddMilliseconds(750), Value = value4 }
        };
        var duration = TimeSpan.FromSeconds(1);

        // Act
        var result = SignalSampler.FullSampling(
            duration, startDate, items,
            x => x.Timestamp, x => x.Value,
            targetFrequency: 16
            );

        // Assert
        Assert.Equal(16, result.Length);

        // Проверяем заполнение
        // Индексы 0-3: значение 10 (до первого измерения)
        for (int i = 0; i < 4; i++) // 0-250 мс / 62.5 = 0-3
            Assert.Equal(value1, result[i]);

        // Индексы 4-7: значение 20 (250-500 мс)
        for (int i = 4; i < 8; i++) // 250-500 мс / 62.5 = 4-7
            Assert.Equal(value2, result[i]);

        // Индексы 8-15: значение 30 (500-1000 мс)
        for (int i = 8; i < 12; i++) // 500-750мс / 62.5 = 8-12
            Assert.Equal(value3, result[i]);

        for (int i = 12; i < 16; i++) // 750-1000 мс / 62.5 = 12-15
            Assert.Equal(value4, result[i]);
    }

    [Fact]
    public void Sampling_WithSamplerAndSingleItem_FillsAll()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1, 10, 0, 0);
        var items = new[] { new TestDataItem { Timestamp = startDate, Value = 5 } };
        var duration = TimeSpan.FromSeconds(0.5); // 8 элементов при 16 Гц

        // Act
        var result = SignalSampler.FullSampling(
            duration, startDate, items,
            x => x.Timestamp, x => x.Value,
            targetFrequency: 16);

        // Assert
        Assert.Equal(8, result.Length); // 0.5 сек × 16 Гц = 8
        Assert.All(result, x => Assert.Equal(5, x)); // Все заполнены значением 5
    }
    #endregion


    #region  ТЕСТЫ КОЛЛИЗИЙ И ПЕРЕЗАПИСИ
    [Fact]
    public void Sampling_OverlappingTimes_LastValueWins()
    {
        // Arrange: два измерения попадают в один и тот же интервал
        var startDate = new DateTime(2024, 1, 1, 10, 0, 0);
        var items = new[]
        {
            new TestDataItem { Timestamp = startDate.AddMilliseconds(100), Value = 1 },
            new TestDataItem { Timestamp = startDate.AddMilliseconds(110), Value = 2 }, // Почти то же время
            new TestDataItem { Timestamp = startDate.AddMilliseconds(120), Value = 3 }  // И еще одно
        };
        var duration = TimeSpan.FromSeconds(1);

        // Act
        var result = SignalSampler.Sampling(
            duration, startDate, items,
            x => x.Timestamp, x => x.Value,
            targetFrequency: 16);

        // Assert: все попадают в индекс 1 (100/62.5=1.6→1, 110/62.5=1.76→1, 120/62.5=1.92→1)
        // Последнее значение (3) должно остаться
        Assert.Equal(3, result[1]);
        Assert.Equal(0, result[0]);
    }
    #endregion


    #region ТЕСТЫ С РАЗНЫМИ ТИПАМИ ДАННЫХ 
    [Fact]
    public void Sampling_WithStringValues_WorksCorrectly()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1, 10, 0, 0);
        var items = new[]
        {
            new { Time = startDate, Text = "Hello" },
            new { Time = startDate.AddSeconds(0.5), Text = "World" }
        };
        var duration = TimeSpan.FromSeconds(1);

        // Act
        var result = SignalSampler.Sampling(
            duration, startDate, items,
            x => x.Time, x => x.Text,
            targetFrequency: 8);

        // Assert
        Assert.Equal(8, result.Length);
        Assert.Equal("Hello", result[0]);
        Assert.Equal("World", result[4]); // 500 мс при 8 Гц → индекс 4
        Assert.Null(result[1]); // default(string) = null
    }

    [Fact]
    public void Sampling_WithNullableInt_WorksCorrectly()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1, 10, 0, 0);
        var items = new[]
        {
            new { Time = startDate, Number = (int?)10 },
            new { Time = startDate.AddSeconds(0.5), Number = (int?)null }
        };
        var duration = TimeSpan.FromSeconds(1);

        // Act
        var result = SignalSampler.Sampling(
            duration, startDate, items,
            x => x.Time, x => x.Number,
            targetFrequency: 4);

        // Assert
        Assert.Equal(4, result.Length);
        Assert.Equal(10, result[0]);
        Assert.Null(result[2]); // 500 мс при 4 Гц → индекс 2
        Assert.Null(result[1]); // default(int?) = null
    }
    #endregion


    #region ТЕСТЫ РЕАЛЬНЫХ СЦЕНАРИЕВ
    [Fact]
    public void Sampling_RealWorldScenario_30MinuteRecording()
    {
        // Arrange: реалистичный сценарий - 30 минут записи при 16 Гц
        var startDate = new DateTime(2024, 1, 1, 10, 0, 0);
        var duration = TimeSpan.FromMinutes(30);
        var targetFrequency = 16;

        // Создаем 4 измерения в секунду (каждые 250 мс) в течение 30 секунд (для теста)
        var items = new List<TestDataItem>();
        for (int second = 0; second < 30; second++)
        {
            for (int measurement = 0; measurement < 4; measurement++)
            {
                items.Add(new TestDataItem
                {
                    Timestamp = startDate
                        .AddSeconds(second)
                        .AddMilliseconds(measurement * 250),
                    Value = second * 10 + measurement
                });
            }
        }

        // Act
        var result = SignalSampler.FullSampling(
            duration, startDate, items,
            x => x.Timestamp, x => x.Value,
            targetFrequency: targetFrequency
            );

        // Assert
        long expectedLength = (long)Math.Ceiling(duration.TotalSeconds * targetFrequency);
        Assert.Equal(expectedLength, result.Length);

        // Проверяем несколько точек
        // Первое измерение должно быть в индексе 0
        Assert.Equal(0, result[0]); // second=0, measurement=0

        // Измерение в 1 секунду 250 мс должно быть в индексе 20
        // 1.25 сек × 16 Гц = 20
        var itemAt1250ms = items.First(x =>
            (x.Timestamp - startDate).TotalMilliseconds == 1250);
        Assert.Equal(itemAt1250ms.Value, result[20]);
    }

    [Fact]
    public void Sampling_WithVeryHighFrequency_DoesNotOverflow()
    {
        // Arrange: высокая частота, но короткое время
        var startDate = new DateTime(2024, 1, 1, 10, 0, 0);
        var duration = TimeSpan.FromSeconds(0.1); // 100 мс
        var highFrequency = 10000; // 10 кГц
        var items = new[] { new TestDataItem { Timestamp = startDate, Value = 1 } };

        // Act
        var result = SignalSampler.Sampling(
            duration, startDate, items,
            x => x.Timestamp, x => x.Value,
            targetFrequency: highFrequency);

        // Assert
        long expectedLength = (long)Math.Ceiling(0.1 * 10000); // 1000
        Assert.Equal(expectedLength, result.Length);
        Assert.Equal(1, result[0]);
    }
    #endregion


    #region ТЕСТЫ НА ОКРУГЛЕНИЕ И ТОЧНОСТЬ 
    [Theory]
    [InlineData(31, 16)]  // 31 мс → 31/62.5=0.496 → 0
    [InlineData(32, 16)]  // 32 мс → 32/62.5=0.512 → 0? (целочисленное деление)
    [InlineData(47, 16)]  // 47 мс → 47/62.5=0.752 → 0
    [InlineData(48, 16)]  // 48 мс → 48/62.5=0.768 → 0
    public void Sampling_IntegerDivision_Correct(int milliseconds, int targetFrequency)
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1, 10, 0, 0);
        var itemTime = startDate.AddMilliseconds(milliseconds);
        var items = new[] { new TestDataItem { Timestamp = itemTime, Value = 100 } };
        var duration = TimeSpan.FromSeconds(1);

        // Act
        var result = SignalSampler.Sampling(
            duration, startDate, items,
            x => x.Timestamp, x => x.Value,
            targetFrequency: targetFrequency);

        // Assert: целочисленное деление отбрасывает дробную часть
        double intervalMs = 1000.0 / targetFrequency;
        int expectedIndex = (int)(milliseconds / intervalMs);

        Assert.Equal(100, result[expectedIndex]);
    }
    #endregion
}