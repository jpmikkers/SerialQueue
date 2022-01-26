using System;
using System.Threading.Tasks;

namespace Threading
{
    public class SerialQueue
    {
        readonly object _locker = new();
        readonly WeakReference<Task> _lastTask = new(default!);

        public Task Enqueue(Action action)
        {
            return Enqueue<bool>(() =>
            {
                action();
                return true;
            });
        }

        public Task<T> Enqueue<T>(Func<T> function)
        {
            lock (_locker)
            {
                Task<T> resultTask;
                
                if (_lastTask.TryGetTarget(out var lastTask))
                {
                    resultTask = lastTask.ContinueWith(_ => function(), TaskContinuationOptions.ExecuteSynchronously);
                }
                else
                {
                    resultTask = Task.Run(function);
                }

                _lastTask.SetTarget(resultTask);
                
                return resultTask;
            }
        }

        public Task Enqueue(Func<Task> asyncAction)
        {
            lock (_locker)
            {
                Task resultTask;
                
                if (_lastTask.TryGetTarget(out var lastTask))
                {
                    resultTask = lastTask.ContinueWith(_ => asyncAction(), TaskContinuationOptions.ExecuteSynchronously).Unwrap();
                }
                else
                {
                    resultTask = Task.Run(asyncAction);
                }

                _lastTask.SetTarget(resultTask);
                
                return resultTask;
            }
        }

        public Task<T> Enqueue<T>(Func<Task<T>> asyncFunction)
        {
            lock (_locker)
            {
                Task<T> resultTask;
                
                if (_lastTask.TryGetTarget(out var lastTask))
                {
                    resultTask = lastTask.ContinueWith(_ => asyncFunction(), TaskContinuationOptions.ExecuteSynchronously).Unwrap();
                }
                else
                {
                    resultTask = Task.Run(asyncFunction);
                }

                _lastTask.SetTarget(resultTask);
                
                return resultTask;
            }
        }
    }
}
