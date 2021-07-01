using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xunit;

namespace Serilog.Bowdlerizer.Tests {
    public class PathTest {
        private static bool TryGetValueForPropertyOrField(object objectThatContainsPropertyName, string path, out object value) {
            path = path.Replace("$..", "");
            var properties = path.Split('.');

            return TryGetValueForPropertyOrField(objectThatContainsPropertyName, properties, out value);
        }

        private static bool TryGetValueForPropertyOrField(object objectThatContainsPropertyName, IEnumerable<string> properties, out object value) {
            foreach (var property in properties) {
                Type typeOfCurrentObject = objectThatContainsPropertyName.GetType();

                var parameterExpression = Expression.Parameter(typeOfCurrentObject, "obj");
                var arrayIndex = property.IndexOf('[');
                if (arrayIndex > 0) {
                    var property1 = property.Substring(0, arrayIndex);
                    Expression memberExpression1 = Expression.PropertyOrField(parameterExpression, property1);
                    var expression1 = Expression.Lambda(Expression.GetDelegateType(typeOfCurrentObject, memberExpression1.Type), memberExpression1, parameterExpression).Compile();
                    objectThatContainsPropertyName = expression1.DynamicInvoke(objectThatContainsPropertyName);
                    var index = Int32.Parse(property.Substring(arrayIndex + 1, property.Length - arrayIndex - 2));
                    typeOfCurrentObject = objectThatContainsPropertyName.GetType();
                    parameterExpression = Expression.Parameter(typeOfCurrentObject, "list");
                    Expression memberExpression2 = Expression.Call(parameterExpression, typeOfCurrentObject.GetMethod("get_Item"), new Expression[] { Expression.Constant(index) });
                    var expression2 = Expression.Lambda(Expression.GetDelegateType(typeOfCurrentObject, memberExpression2.Type), memberExpression2, parameterExpression).Compile();
                    objectThatContainsPropertyName = expression2.DynamicInvoke(objectThatContainsPropertyName);
                } else {
                    try {
                        Expression memberExpression = Expression.PropertyOrField(parameterExpression, property);
                        var expression = Expression.Lambda(Expression.GetDelegateType(typeOfCurrentObject, memberExpression.Type), memberExpression, parameterExpression).Compile();
                        objectThatContainsPropertyName = expression.DynamicInvoke(objectThatContainsPropertyName);
                    } catch {
                        value = null;
                        return false;
                    }
                }
            }

            value = objectThatContainsPropertyName;
            return true;
        }

        [Fact]
        public void TestUnknownProperty() {
            var dateTime = new DateTime();

            var result = TryGetValueForPropertyOrField(dateTime, "$..Foo", out object _);
            Assert.False(result);
        }


        [Fact]
        public void TestOneProperty() {
            var dateTime = new DateTime();

            TryGetValueForPropertyOrField(dateTime, "$..Day", out object result);

            Assert.Equal(dateTime.Day, result);
        }

        [Fact]
        public void TestNestedProperties() {
            var dateTime = new DateTime();

            TryGetValueForPropertyOrField(dateTime, "$..Date.Day", out object result);

            Assert.Equal(dateTime.Date.Day, result);
        }

        [Fact]
        public void TestDifferentNestedProperties() {
            var dateTime = new DateTime();

            TryGetValueForPropertyOrField(dateTime, "$..Date.DayOfWeek", out object result);

            Assert.Equal(dateTime.Date.DayOfWeek, result);
        }
    }
}
