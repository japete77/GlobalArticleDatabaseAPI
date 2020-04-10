using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GlobalArticleDatabase.Helpers
{
    public class BsonPropertyHelper
    {
        public static string GetBsonElementName<T>(Expression<Func<T, object>> property)
        {
            LambdaExpression lambda = (LambdaExpression)property;
            MemberExpression memberExpression;

            if (lambda.Body is UnaryExpression)
            {
                UnaryExpression unaryExpression = (lambda.Body as UnaryExpression);
                memberExpression = (unaryExpression.Operand as MemberExpression);
            }
            else
            {
                memberExpression = (lambda.Body as MemberExpression);
            }

            if (memberExpression != null)
            {
                var propertyMember = typeof(T).GetProperties().Where(w => w.Name == memberExpression.Member.Name).FirstOrDefault();

                if (propertyMember != null)
                {
                    var attribute = (propertyMember.GetCustomAttributes(typeof(BsonElementAttribute), true).FirstOrDefault() as BsonElementAttribute);

                    if (attribute != null)
                    {
                        return attribute.ElementName;
                    }
                }
            }

            return memberExpression.Member.Name;
        }

        public static string GetPropertyName<T>(Expression<Func<T, object>> property)
        {
            LambdaExpression lambda = (LambdaExpression)property;
            MemberExpression memberExpression;

            if (lambda.Body is UnaryExpression)
            {
                UnaryExpression unaryExpression = (lambda.Body as UnaryExpression);
                memberExpression = (unaryExpression.Operand as MemberExpression);
            }
            else
            {
                memberExpression = (lambda.Body as MemberExpression);
            }

            return memberExpression.Member.Name;
        }
    }
}
