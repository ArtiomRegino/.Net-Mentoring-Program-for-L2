using System;
using System.Threading;
using System.Threading.Tasks;

namespace task_2._1
{
    class Program
    {  
        static void Main()
        {
            Run().Wait();
        }

        public static async Task Run()
        {
            int number;
            CancellationTokenSource source = null;
            CancellationToken token;

            Console.WriteLine("Enter number N: ");

            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out number))
                {
                    source?.Cancel();
                }
                source = new CancellationTokenSource();
                token = source.Token;
                Task.Run(async () => await Calculate(number, token));
            }
        }

        public static async Task Calculate(int number, CancellationToken token)
        {
            Console.WriteLine("Wait for calculation or change the N: ");
            try
            {
                int result = await SumAsync(number, token);

                Console.WriteLine($"Result: {result}");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Calculation canceled.");
            }
            catch (Exception)
            {
                Console.WriteLine("Calculation faulted.");
            }
        }

        public static async Task<int> SumAsync(int number, CancellationToken token)
        {
            await Task.Delay(1000);

            int sum = 0;

            for (int i = 0; i < number; i++)
            {
                sum += i;
            }
            token.ThrowIfCancellationRequested();

            return sum;
        }
    }
}
