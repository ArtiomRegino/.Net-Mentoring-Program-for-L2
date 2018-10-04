using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace task_6
{    //6. Write a program which creates two threads and a shared collection: the first one 
     //   should add 10 elements into the collection and the second should print all elements 
     //   in the collection after each adding.
     //   Use Thread, ThreadPool or Task classes for thread creation and any kind of synchronization constructions.
    class Program
    {//?????? после доб 10 эл-ов печатать? или после каждого элемента?
        static void Main()
        {
            List<int> collection = new List<int>();


        }

        public static void Print(IEnumerable<int> collection)
        {
            Console.WriteLine(Environment.NewLine);

            foreach (var item in collection)
            {
                Console.WriteLine(item);
            }
        }

        public static void AddToCollection(int element, List<int> collection)
        {
            collection.Add(element);
        }
    }
}
