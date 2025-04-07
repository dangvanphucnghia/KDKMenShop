using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace KDKMenShop.Others
{
    public class QueryExecutionTimeListener : IObserver<DiagnosticListener>, IObserver<KeyValuePair<string, object>>
    {
        private double _totalExecutionTimeMs = 0;
        private string _lastQueryText = string.Empty;
        private double _lastQueryDuration = 0;

        public void OnNext(DiagnosticListener listener)
        {
            if (listener.Name == DbLoggerCategory.Name)
            {
                listener.Subscribe(this);
            }
        }

        public void OnNext(KeyValuePair<string, object> eventPair)
        {
            if (eventPair.Key == RelationalEventId.CommandExecuted.Name && eventPair.Value is CommandExecutedEventData eventData)
            {
                string currentQueryText = eventData.Command.CommandText;
                double currentQueryDuration = eventData.Duration.TotalMilliseconds;

                // Check if this query matches the last one logged and if it was executed in a similar duration
                if (currentQueryText == _lastQueryText && Math.Abs(currentQueryDuration - _lastQueryDuration) < 1)
                {
                    // Skip logging this duplicate query
                    return;
                }

                _totalExecutionTimeMs += currentQueryDuration;
                Console.WriteLine("---------------------------------------------------------------");
                Console.WriteLine($"Tong thoi gian query: {currentQueryDuration} ms");
                Console.WriteLine("---------------------------------------------------------------");

                // Update last query details
                _lastQueryText = currentQueryText;
                _lastQueryDuration = currentQueryDuration;
            }
        }

        public double GetTotalExecutionTime()
        {
            return _totalExecutionTimeMs;
        }

        public void ResetExecutionTime()
        {
            _totalExecutionTimeMs = 0;
        }

        public void OnError(Exception error)
        {
            Console.Error.WriteLine($"Error in QueryExecutionTimeListener: {error.Message}");
        }

        public void OnCompleted() { }
    }
}
