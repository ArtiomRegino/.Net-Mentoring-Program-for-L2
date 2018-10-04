using System;
using System.Threading;
using System.Threading.Tasks;

namespace task_7
{
    class Program
    {
        static void Main()
        {
            //a
            CreateAndRunTask().ContinueWith((antecedent) =>
            {
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            });

            Console.ReadKey();

            //b
            CreateAndRunTask().ContinueWith((antecedent) =>
            {
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            }, TaskContinuationOptions.OnlyOnFaulted);

            Console.ReadKey();

            //c
            CreateAndRunTask().ContinueWith((antecedent) =>
            {
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId);

            }, TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted);

            Console.ReadKey();

            //d
            CreateAndRunTask().ContinueWith((antecedent) =>
            {
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId);

            }, TaskContinuationOptions.OnlyOnCanceled);

            Console.ReadKey();
        }

        public static Task CreateAndRunTask()
        {
            return Task.Run(() =>
            {
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
                throw null;
            });
        }
    }
}
