using System;
using System.Threading;

namespace task_5
{
    class Program
    { 
        static void Main()
        {
            CreateTaskRecursive(10);
            Console.ReadLine();
        }

        private static Semaphore _sph = new Semaphore(1,1);

        public static void CreateTaskRecursive(int number)
        {
            ThreadPool.QueueUserWorkItem(num =>
            {
                _sph.WaitOne();
                if (number <= 0) return;

                var intNum = (int)num;
                intNum = intNum - 1;
                Console.WriteLine(intNum);

                _sph.Release();
                CreateTaskRecursive(intNum);
            }, number);
        }
    }
}
