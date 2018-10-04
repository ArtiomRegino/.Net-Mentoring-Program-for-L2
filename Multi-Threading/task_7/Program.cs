using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace task_7
{
    class Program
    {
        static void Main(string[] args)
        {

            var task = Task.Run(() =>
            {
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
                throw null;
            });

            //a
            //task.ContinueWith((antecedent) => { Console.WriteLine(Thread.CurrentThread.ManagedThreadId); });

            //b
            //task.ContinueWith((antecedent) =>
            //{
            //    Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            //}, TaskContinuationOptions.OnlyOnFaulted);

            //c
            //var task = Task.Run(() =>
            //{
            //    Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            //    throw null;
            //});
            //task.ContinueWith((antecedent) =>
            //{
            //    Console.WriteLine(Thread.CurrentThread.ManagedThreadId);

            //}, TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted);

            Console.ReadKey();
        }
    }
}
