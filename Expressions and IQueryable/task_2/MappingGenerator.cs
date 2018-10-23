using System;
using System.Linq;
using System.Linq.Expressions;

namespace task_2
{
    public class MappingGenerator
    {
        public Mapper<TSource, TDestination> Generate<TSource, TDestination>()
        {
            var sourceParam = Expression.Parameter(typeof(TSource));

            var mapFunction = Expression.MemberInit(Expression.New(typeof(TDestination)), sourceParam.Type.GetProperties().Select(p =>
            {
                var destinationProp = typeof(TDestination).GetProperty(p.Name);

                if (destinationProp != null)
                {
                    return Expression.Bind(typeof(TDestination).GetProperty(p.Name), Expression.Property(sourceParam, p));
                }
                return null;
                    
            }).Where(m => m != null));
                
            var expr = Expression.Lambda<Func<TSource, TDestination>> (mapFunction, sourceParam);

            return new Mapper<TSource, TDestination>(expr.Compile());
        }
    }
}
