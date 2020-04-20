using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using TaskWorkerExecutor;

namespace UnitTestProject1
{

    [TestClass]
    public class TaskWorker
    {


        [TestMethod]      
        public void ActionInvokedTest()
        {
            //arrange
            using (var we = new WorkExecutor())
            {
                var actionWasInvoked = false;
                Action action = () => actionWasInvoked = true;

                //act
                we.QueueAction(action);
                Thread.Sleep(TimeSpan.FromMilliseconds(10));//time to execute task


                //assert
                Assert.IsTrue(actionWasInvoked);

            }

        }

        [TestMethod]
        public void ActionInvokedOnceTest()
        {
            
            using (var we = new WorkExecutor())
            {
                //arrange
                var counts = 0;
                var expected = 1;

                //act
                we.QueueAction(() => counts++);
                Thread.Sleep(TimeSpan.FromMilliseconds(10));//time to execute task

                //assert
                Assert.AreEqual(expected, counts);
            }
               

        }

        [TestMethod]
        public void ActionExecutedSequentiallyTest()
        {
             
            using (WorkExecutor we = new WorkExecutor())
            {

                //arrange
                var counted = 0;
                var message = "";
                var executed = false;
                var status = "";

                List<Action> actions = new List<Action>();

                actions.AddRange(new List<Action>{
                    () => counted++,
                    () => message = "Hi there",
                    () => executed = true,
                    () => status = "worked",
                });


                
                for (int i = 0; i < actions.Count; i++)
                {
                    //act
                    we.QueueAction(actions[i]);
                    Thread.Sleep(TimeSpan.FromMilliseconds(10));//time to execute task


                    //assert
                    if (i == 0)
                    {
                        Assert.AreEqual(1, counted);
                    }
                    else
                    if (i == 1)
                    {
                        Assert.AreEqual("Hi there", message);
                    }
                    else
                    if (i == 2)
                    {
                        Assert.AreEqual(true, executed);
                    }
                    else
                    if (i == 3)
                    {
                        Assert.AreEqual("worked", status);
                    }
                }




            }


        }


    }
}
