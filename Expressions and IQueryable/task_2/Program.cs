namespace task_2
{
    class Program
    {
        static void Main()
        {
            var mapGenerator = new MappingGenerator();
            var foo = new Foo { A = 5, B = "Alla", C = 64 };

            var mapper = mapGenerator.Generate<Foo, Bar>();

            var res = mapper.Map(foo);
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
    }
}
