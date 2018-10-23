using System;

namespace task_2
{
    public class Mapper<TSource, TDestination>
    {
        readonly Func<TSource, TDestination> _mapFunction;

        internal Mapper(Func<TSource, TDestination> func)
        {
            _mapFunction = func;
        }

        public TDestination Map(TSource source)
        {
            return _mapFunction(source);
        }
    }
}