using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TP.ConcurrentProgramming.Data
{
    internal class Logger : IDisposable
    {
        private readonly BlockingCollection<string> _logQueue;
        
        private readonly CancellationTokenSource _cts;
        private readonly Task _writerTask;

        public Logger()
        {
            _logQueue = new BlockingCollection<string>();
            _cts = new CancellationTokenSource();
            
            _writerTask = Task.Run(WriteLogsAsync);
        }

        public void LogDiagnosticData(int ballId, double x, double y, double vX, double vY)
        {
            if (_logQueue.IsAddingCompleted) return;

            var logEntry = new
            {
                Timestamp = DateTime.Now.ToString("O"),
                BallId = ballId,
                TaskId = Task.CurrentId,
                PosX = x,
                PosY = y,
                VelX = vX,
                VelY = vY
            };

            string json = JsonSerializer.Serialize(logEntry);
            _logQueue.TryAdd(json);
        }

        private async Task WriteLogsAsync()
        {
            using var sw = new StreamWriter("./diagnostics.json", append: true);
            sw.AutoFlush = true;
            
            try
            {
                foreach (var log in _logQueue.GetConsumingEnumerable(_cts.Token))
                {
                    await sw.WriteLineAsync(log);
                }
            }
            catch (OperationCanceledException) {}
        }

        public void Dispose()
        {
            _logQueue.CompleteAdding();
            
            try
            {
                _writerTask.Wait(500); 
            }
            catch { }
            
            _cts.Cancel();
            _cts.Dispose();
            _logQueue.Dispose();
        }
    }
}