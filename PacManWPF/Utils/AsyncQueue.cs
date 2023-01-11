using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PacManWPF.Utils
{
    public class AsyncQueue<T>
    {

        private Queue<T> _queue = new();


        private Queue<TaskCompletionSource<T>>? dequeuingWaiters;


        public bool IsEmpty
        {
            get => Count == 0;
        }


        public int Count
        {
            get
            {
                lock (this)
                {
                    return _queue.Count;
                }
            }
        }


        public void Enqueue(T value)
        {
            Debug.Assert(TryEnqueue(value));
        }


        public bool TryEnqueue(T value)
        {
            bool alreadyDispatched = false;
            lock (this)
            {
                // Is a dequeuer waiting for this?
                while (dequeuingWaiters?.Count > 0)
                {
                    TaskCompletionSource<T> waitingDequeuer = dequeuingWaiters.Dequeue();
                    if (waitingDequeuer.TrySetResult(value))
                    {
                        alreadyDispatched = true;
                        break;
                    }
                }

                FreeCanceledDequeuers();

                if (!alreadyDispatched)
                    _queue.Enqueue(value);
            }

            return true;
        }


        public Task<T> DequeueAsync(CancellationToken cancellationToken = default)
        {
            T result;

            lock (this)
            {
                if (_queue?.Count > 0)
                {
                    result = _queue.Dequeue();
                }
                else
                {
                    if (dequeuingWaiters is null)
                        dequeuingWaiters = new Queue<TaskCompletionSource<T>>(capacity: 2);
                    else
                        FreeCanceledDequeuers();

                    var waiterTcs = new TaskCompletionSource<T>();
                    dequeuingWaiters.Enqueue(waiterTcs);
                    return waiterTcs.Task;
                }
            }

            CompleteIfNecessary();
            return Task.FromResult(result);
        }


        private void CompleteIfNecessary()
        {
            Debug.Assert(Monitor.IsEntered(this) is false); // Check if in lock

            bool transitionTaskSource;
            lock (this)
            {
                transitionTaskSource = _queue is null || _queue.Count == 0;
                if (transitionTaskSource)
                {
                    while (dequeuingWaiters?.Count > 0)
                    {
                        dequeuingWaiters.Dequeue().TrySetCanceled();
                    }
                }
            }
        }


        private void FreeCanceledDequeuers()
        {
            lock (this)
                while (dequeuingWaiters?.Count > 0 && dequeuingWaiters.Peek().Task.IsCompleted)
                    dequeuingWaiters.Dequeue();
        }
    }
}