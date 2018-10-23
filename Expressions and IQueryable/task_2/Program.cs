namespace task_2
{
    internal class Program
    {
        private static void Main()
        {
            TestMethod();
        }

        public class Foo
        {
            public int A { get; set; }
            public string B { get; set; }
            public int C { get; set; }
        }

        public class Bar
        {
            public int A { get; set; }
            public string B { get; set; }
        }

        public static void TestMethod()
        {
            var map = new Mapper();
            var foo = new Foo { A = 5, B = "Alla", C = 64 };

            var result = map.Change<Foo, Bar>(foo);
        }
    }
}
