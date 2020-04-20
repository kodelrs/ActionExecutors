using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TaskWorkerExecutor
{
    public class WorkExecutor : IDisposable
    { 
        private ConcurrentQueue<Action> queue = null;
        private CancellationTokenSource tokenSource = null;
        private CancellationToken token;
        private bool disposed = false;
        private Task executionTask = null;

        public WorkExecutor()
        {
            queue = new ConcurrentQueue<Action>();
            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;
            executionTask = Task.Factory.StartNew(this.process_queue, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
 

        public void QueueAction(Action action)
        {
             queue.Enqueue(action);
        }

        private async void process_queue()
        {
            bool readyForTheTask = true;
            while (true)
            {
               
                if (token.IsCancellationRequested)
                {
                    queue.Clear();

                    break;
                }

                if (readyForTheTask)
                {
                    readyForTheTask = false;
                    Action action;
                    await Task.Factory.StartNew(() =>
                    {
                        if (token.IsCancellationRequested)
                        {
                            return;
                        }
                        if (queue.TryDequeue(out action))
                        {
                             action.Invoke();
                        }

                    }, token).ContinueWith((next) =>
                    {
                        if (next.IsCanceled || next.IsFaulted || next.IsCompleted)
                        {
                            readyForTheTask = true;
                        }
                         
                        
                    });
                }else
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(5));
                }
                 
            }
        }

        void Dispose(bool disposing)
        {
            try
            {
                if (!disposed)
                {
                    if (disposing)
                    {
                        queue.Clear();

                        tokenSource.Cancel();
                        executionTask.Wait();
                        tokenSource.Dispose();
                        executionTask.Dispose();
                    }

                    disposed = true;
                }
            }
            catch(Exception exp)
            {
                Console.WriteLine("error while disposing>" + exp.Message);
            }
            finally
            {
                tokenSource = null;
                executionTask = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

    }
}
