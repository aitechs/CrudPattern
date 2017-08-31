using System;
using System.Linq.Expressions;
using System.Reflection;

namespace AiTech.LiteOrm
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EntityMapper<T> where T : class
    {

        protected readonly T ItemData;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityOwner"></param>
        public EntityMapper(T entityOwner)
        {
            ItemData = entityOwner;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="outExpr"></param>
        /// <param name="input"></param>
        public void Map(Expression<Func<T, dynamic>> outExpr, dynamic input)
        {
            if (input == null) return;


            MemberExpression expr;

            var body = outExpr.Body as MemberExpression;
            if (body != null)
            {
                expr = body;
            }
            else
            {
                var op = ((UnaryExpression)outExpr.Body).Operand;
                expr = ((MemberExpression)op);
            }
            //outExpr.Compile();
            //var expr = (MemberExpression)outExpr.Body;

            var prop = (PropertyInfo)expr.Member;
            prop.SetValue(ItemData, input, null);

        }
    }
}
