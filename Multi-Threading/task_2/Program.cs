using System;
using System.Linq;
using System.Threading.Tasks;

namespace task_2
{
    internal class Program
    {
        private static void Main()
        {
            Task.Run(() =>
                    {
                        var ints = new int[10];
                        var random = new Random(DateTime.Now.Millisecond);

                        for (var i = 0; i < ints.Length; i++)
                            ints[i] = random.Next();

                        return ints;
                    }
                )
                .ContinueWith(antecedent =>
                    {
                        var ints = antecedent.Result;
                        var random = new Random(DateTime.Now.Millisecond);
                        var randomNumber = random.Next();

                        for (var i = 0; i < ints.Length; i++)
                            ints[i] *= randomNumber;

                        return ints;
                    }
                )
                .ContinueWith(antecedent =>
                    {
                        var ints = antecedent.Result;

                        Array.Sort(ints, 0, 10);

                        return ints;
                    }
                )
                .ContinueWith(antecedent =>
                    {
                        var ints = antecedent.Result;

                        var avr = ints.Sum();
                        avr /= ints.Length;

                        return avr;
                    }
                );
        }
    }
}