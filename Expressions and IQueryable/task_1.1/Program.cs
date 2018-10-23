using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using task_1._1.Visitors;

namespace task_1
{
    class Program
    {
        static void Main()
        {
            Expression<Func<int, int>> sourceAdd = i => i + (i + 1) * (i + 5) * (i + 1);
            var resultAdd = new AddToIncrementTrasform().VisitAndConvert(sourceAdd, "");

            Console.WriteLine(sourceAdd + " " + sourceAdd.Compile().Invoke(3));
            if (resultAdd != null) Console.WriteLine(resultAdd + " " + resultAdd.Compile().Invoke(3));


            Expression<Func<int, int>> sourceSub = i => i - (i - 1) * (i - 5) * (i - 1);
            var resultSub = new SubtractToDecrementTrasform().VisitAndConvert(sourceSub, "");

            Console.WriteLine(sourceSub + " " + sourceSub.Compile().Invoke(3));
            if (resultSub != null) Console.WriteLine(resultSub + " " + resultSub.Compile().Invoke(3));

            //---------------------------------------------
            Expression<Func<int, int, int, string>> add = (x, y, h) => ((2 * x) + y).ToString();
            Console.WriteLine(add);

            var args = new Dictionary<string, ConstantExpression>
            {
                ["x"] = Expression.Constant(4, typeof(int)),
                ["y"] = Expression.Constant(1, typeof(int)),
            };

            var visitor = new ParameterToConstantTransform(args);
            var result = visitor.Visit(add);
            Console.WriteLine(result);

        }
    }
}
