using System;

namespace task_3
{
    class Program
    {
        static void Main()
        {
            var a = new int[3, 4];
            var b = new int[4, 2];

            Matrix.Randomise(a);
            Matrix.Randomise(b);

            Matrix.Print(a);
            Console.WriteLine();
            Matrix.Print(b);
            
            var c = Matrix.Multiply(a, b);

            Matrix.Print(c);
        }
    }
}
