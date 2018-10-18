using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace task_1._1.Visitors
{
    class ParameterToConstantTransform: ExpressionVisitor
    {
        private readonly Dictionary<string, ConstantExpression> _parameters;

        public ParameterToConstantTransform(Dictionary<string, ConstantExpression> parameters)
        {
            this._parameters = parameters;
        }

        protected override Expression VisitParameter(ParameterExpression node)
            => _parameters.TryGetValue(node.Name, out var ce) ? (Expression)ce : node;

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
           var parameters = node.Parameters;
           IEnumerable<ParameterExpression> b = parameters;

           foreach (var item in _parameters)
           {
               b = b.Where(x => x.Name != item.Key);
           }

            return Expression.Lambda(Visit(node.Body), b);
        }
    }
}
