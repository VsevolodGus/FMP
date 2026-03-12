using Bioss.Ultrasound.Services.Logging.Abstracts;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Bioss.Ultrasound.Services
{
    public sealed class QueuePump<T>
    {
        private readonly object _stateLock = new object();

        private readonly ILogger _logger;
        private readonly ConcurrentQueue<T> _queue = new();
        private readonly Func<T, Task> _consumer;
        private readonly int _emptyDelayMs;

        private CancellationTokenSource _cts;
        private Task _loopTask = Task.CompletedTask;

        public QueuePump(Func<T, Task> consumer, ILogger logger = null,  int emptyDelayMs = 25)
        {
            _consumer = consumer;
            _logger = logger;
            _emptyDelayMs = emptyDelayMs;
        }

        public void Start()
        {
            lock (_stateLock)
            {
                if (_loopTask != null && !_loopTask.IsCompleted)
                    return;

                _cts?.Dispose();
                _cts = new CancellationTokenSource();

                var token = _cts.Token;
                _loopTask = Task.Run(() => LoopAsync(token), token);
            }
        }

        public void Enqueue(T item)
        {
            _queue.Enqueue(item);
        }
       
        public void Reset()
        {
            lock (_stateLock)
            {
                _queue.Clear();
            }
        }

        public async Task StopAsync()
        {
            CancellationTokenSource cts;
            Task task;
            lock (_stateLock)
            {
                cts = _cts;
                task = _loopTask;

                _cts = null;
                _loopTask = Task.CompletedTask;
            }

            if (cts == null)
                return;

            try
            {
                cts.Cancel();
                if (task != null)
                    await task.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                cts.Dispose();
                Reset();
            }
        }
       
        private async Task LoopAsync(CancellationToken token)
        {
            var iterationNumber = 0;
            while (!token.IsCancellationRequested)
            {
                if (!_queue.TryDequeue(out var item))
                {
                    await Task.Delay(_emptyDelayMs, token).ConfigureAwait(false);
                    continue;
                }

                await _consumer(item).ConfigureAwait(false);
                DebugText(ref iterationNumber);

                while (_queue.TryDequeue(out item))
                {
                    await _consumer(item).ConfigureAwait(false);
                    DebugText(ref iterationNumber);
                }


            }
        }

        private void DebugText(ref int iterationNumber)
        {
            iterationNumber++;
            if (iterationNumber >= 500)
            {
                iterationNumber = 0;
                _logger?.Log($"Blu queue size: {_queue.Count}");
            }
        }
    }
}
