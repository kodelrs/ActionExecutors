using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BackgroundWorkerExecutor
{

    public class WorkExecutor :IDisposable
    {
        private ConcurrentQueue<Action> queue;
        private BackgroundWorker worker;
        private AutoResetEvent resetEvent = new AutoResetEvent(true);
        private bool disposed = false;

        public WorkExecutor()
        {
            queue = new ConcurrentQueue<Action>();
            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += process_queue;
            Start();
        }

        public void QueueAction(Action  action)
        {
            queue.Enqueue(action);
            resetEvent.Set();
           
        }


        private void process_queue(object sender, DoWorkEventArgs e)
        {
            try
            {
                while (!worker.CancellationPending)
                {
                    resetEvent.WaitOne();
                    if (queue.TryDequeue(out Action action))
                    {
                        if (action != null) action.Invoke();
                    }

                }
            }
            catch (Exception exp)
            {
                Console.WriteLine("error >" + exp.Message);
            }
            finally
            {
                e.Cancel = true;
            }


        }


        public void Start()
        {
            if (!worker.IsBusy)
                worker.RunWorkerAsync();
        }

        public void Stop()
        {
            worker.CancelAsync();
        }

        void Dispose(bool disposing)
        {

            try
            {
                if (!disposed)
                {
                    if (disposing)
                    {
                        Stop();
                        queue.Clear();
                        resetEvent.Close();
                    }


                    disposed = true;
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine("error while disposing >" + exp.Message);
            }
            finally
            {
                resetEvent = null;
            }
            
        }

        public void Dispose()
        {
            Dispose(true);

        }
    }
}
