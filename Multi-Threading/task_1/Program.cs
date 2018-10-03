using System;
using System.Threading.Tasks;

namespace task_1
{
    class Program
    {
        static void Main()
        {
            var arrTasks = new Task[100];

            for (var i = 0; i < 100; i++)
            {
                int taskNum = i + 1;

                arrTasks[i] = new Task(() =>
                {
                    for (int j = 1; j <= 1000; j++)
                    {
                        Console.WriteLine($"Task #{taskNum} – {j}.");
                    }
                });
            }

            foreach (var task in arrTasks)
            {
                task.Start();
            }

            Task.WaitAll(arrTasks);
        }
    }
}
