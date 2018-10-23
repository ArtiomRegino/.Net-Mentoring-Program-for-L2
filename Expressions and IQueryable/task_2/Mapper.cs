using System.Linq;

namespace task_2
{
    public class Mapper
    {
        public TDestination Change<TSource, TDestination>(TSource source) where TDestination : new()
        {
            var destination = new TDestination();
            var propsDest = destination.GetType().GetProperties();
            var propsSource = source.GetType().GetProperties();

            foreach (var propSource in propsSource)
            {
                var propDest = propsDest.FirstOrDefault(p => p.Name == propSource.Name && p.PropertyType == propSource.PropertyType);
                var a = propSource.GetValue(source);

                if (propDest != null) propDest.SetValue(destination, a);
            }

            return destination;
        }
    }
}