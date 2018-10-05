using System;
using System.Collections.Generic;
using System.Threading;

namespace task_6
{
    class Program
    {
        static EventWaitHandle _waitHandle1 = new EventWaitHandle(false, EventResetMode.AutoReset);
        static EventWaitHandle _waitHandle2 = new EventWaitHandle(false, EventResetMode.AutoReset);

        static void Main()
        {
            var collection = new List<int>();

            var addToCollectionThread = new Thread(number =>
            {
                for (int i = 0; i < (int)number; i++)
                {
                     collection.Add(i);

                    _waitHandle1.Set();
                    _waitHandle2.WaitOne();
                }
            });

            var printCollectionThread = new Thread(number =>
            {
                int i = (int)number;
                while (i > 0)
                {
                    _waitHandle1.WaitOne();
                    
                    foreach (var item in collection)
                    {
                        Console.Write($"{item} ");
                    }
                    
                    Console.WriteLine(Environment.NewLine);
                    _waitHandle2.Set();
                    i--;
                }
            });

            addToCollectionThread.Start(10);
            printCollectionThread.Start(10);

        }
    }
}
