using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace task_5
{
    class Program
    { 
        static void Main()
        {
            CreateTaskRecursive(10);
            Console.ReadLine();
        }

        private static Semaphore sph = new Semaphore(1,1);

        public static void CreateTaskRecursive(int number)
        {
            ThreadPool.QueueUserWorkItem(num =>
            {
                sph.WaitOne();
                if (number <= 0) return;

                var intNum = (int)num;
                intNum = intNum - 1;
                Console.WriteLine(intNum);

                sph.Release();
                CreateTaskRecursive(intNum);
            }, number);
        }
    }
}
