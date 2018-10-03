using System;
using System.Threading.Tasks;

namespace task_3
{
    internal static class Matrix
    {
        /// <summary>
        /// Fulfil a matrix with random numbers.
        /// </summary>
        /// <param name="matrix">Array for being fulfiled.</param>
        /// <returns>Fulfiled matrix.</returns>
        public static int[,] Randomise(int[,] matrix)
        {
            var random = new Random(DateTime.Now.Millisecond);
            var rows = matrix.GetUpperBound(0) + 1;
            var col = matrix.GetUpperBound(1) + 1;

            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < col; j++)
                {
                    matrix[i, j] = random.Next(-20, 30);
                }
            }

            return matrix;
        }

        /// <summary>
        /// Multiply two matrixes.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>Result of multipling.</returns>
        public static int[,] Multiply(int[,] a, int[,] b)
        {
            var rowsA = a.GetUpperBound(0) + 1;
            var colA = a.GetUpperBound(1) + 1;
            var rowsB = b.GetUpperBound(0) + 1;
            var colB = b.GetUpperBound(1) + 1;

            if (colA != rowsB)
            {
                Console.WriteLine("Check sizes of matrixes.");
            }

            var c = new int[rowsA, colB];

            Parallel.For(0, rowsA, i =>
            {
                for (var j = 0; j < colB; ++j)
                {
                    for (var n = 0; n < colA; ++n)
                        c[i, j] += a[i, n] * b[n, j];
                }
            });

            return c;
        }

        /// <summary>
        /// Print matrix.
        /// </summary>
        /// <param name="a">Matrix for printing.</param>
        public static void Print(int[,] a)
        {
            var rows = a.GetUpperBound(0) + 1;
            var col = a.GetUpperBound(1) + 1;

            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < col; j++)
                {
                    Console.Write($"{a[i, j]} ");
                }
                Console.Write(Environment.NewLine);
            }
        }
    }
}
