using System;
using System.Threading;

namespace task_4
{
    class Program
    {
        static void Main()
        {
            CreateTaskRecursive(10);
        }

        public static void CreateTaskRecursive(int number)
        {
            var thr = new Thread(num =>
            {
                if (number <= 0) return;

                var intNum = (int) num;
                intNum = intNum - 1;
                Console.WriteLine(intNum);

                CreateTaskRecursive(intNum);
            });

            thr.Start(number);
            thr.Join();
        }
    }
}
