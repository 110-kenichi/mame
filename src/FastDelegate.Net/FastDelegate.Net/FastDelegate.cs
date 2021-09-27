#define NET_FX_4 //If you are using .Net 4.0 or greater, you can un-comment this define

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace FastDelegate.Net {
    public static class MethodInfoExtensions {
        private static Func<Object, Object[], Object> CreateForNonVoidInstanceMethod(MethodInfo method) {
            ParameterExpression instanceParameter = Expression.Parameter(typeof(Object), "target");
            ParameterExpression argumentsParameter = Expression.Parameter(typeof(Object[]), "arguments");

            MethodCallExpression call = Expression.Call(
                Expression.Convert(instanceParameter, method.DeclaringType),
                method,
                CreateParameterExpressions(method, argumentsParameter));

            Expression<Func<Object, Object[], Object>> lambda = Expression.Lambda<Func<Object, Object[], Object>>(
                Expression.Convert(call, typeof(Object)),
                instanceParameter,
                argumentsParameter);

            return lambda.Compile();
        }

        private static Func<Object[], Object> CreateForNonVoidStaticMethod(MethodInfo method) {
            ParameterExpression argumentsParameter = Expression.Parameter(typeof(Object[]), "arguments");

            MethodCallExpression call = Expression.Call(
                method,
                CreateParameterExpressions(method, argumentsParameter));

            Expression<Func<Object[], Object>> lambda = Expression.Lambda<Func<Object[], Object>>(
                Expression.Convert(call, typeof(Object)),
                argumentsParameter);

            return lambda.Compile();
        }

        private static Action<Object, Object[]> CreateForVoidInstanceMethod(MethodInfo method) {
            ParameterExpression instanceParameter = Expression.Parameter(typeof(Object), "target");
            ParameterExpression argumentsParameter = Expression.Parameter(typeof(Object[]), "arguments");

            MethodCallExpression call = Expression.Call(
                Expression.Convert(instanceParameter, method.DeclaringType),
                method,
                CreateParameterExpressions(method, argumentsParameter));

            Expression<Action<Object, Object[]>> lambda = Expression.Lambda<Action<Object, Object[]>>(
                call,
                instanceParameter,
                argumentsParameter);

            return lambda.Compile();
        }

        private static Action<Object[]> CreateForVoidStaticMethod(MethodInfo method) {
            ParameterExpression argumentsParameter = Expression.Parameter(typeof(Object[]), "arguments");

            MethodCallExpression call = Expression.Call(
                method,
                CreateParameterExpressions(method, argumentsParameter));

            Expression<Action<Object[]>> lambda = Expression.Lambda<Action<Object[]>>(
                call,
                argumentsParameter);

            return lambda.Compile();
        }

        private static Expression[] CreateParameterExpressions(MethodInfo method, Expression argumentsParameter) {
            return method.GetParameters().Select((parameter, index) =>
                Expression.Convert(
                    Expression.ArrayIndex(argumentsParameter, Expression.Constant(index)), parameter.ParameterType)).Cast<Expression>().ToArray();
        }

        public static Func<Object, Object[], Object> Bind(this MethodInfo method) {
            if (method.IsStatic) {
                if (method.ReturnType == typeof(void)) {
                    Action<object[]> wrapped = CreateForVoidStaticMethod(method);
                    return (target, parameters) => {
                        wrapped(parameters);
                        return (Object)null;
                    };
                } else {
                    Func<object[], object> wrapped = CreateForNonVoidStaticMethod(method);
                    return (target, parameters) => wrapped(parameters);
                }
            }
            if (method.ReturnType == typeof(void)) {
                Action<object, object[]> wrapped = CreateForVoidInstanceMethod(method);
                return (target, parameters) => {
                    wrapped(target, parameters);
                    return (Object)null;
                };
            } else {
                Func<object, object[], object> wrapped = CreateForNonVoidInstanceMethod(method);
                return wrapped;
            }
        }

        public static Type LambdaType(this MethodInfo method) {
            if (method.ReturnType == typeof(void)) {
                Type actionGenericType;
                switch (method.GetParameters().Length) {
                    case 0:
                        return typeof(Action);
                    case 1:
                        actionGenericType = typeof(Action<>);
                        break;
                    case 2:
                        actionGenericType = typeof(Action<,>);
                        break;
                    case 3:
                        actionGenericType = typeof(Action<,,>);
                        break;
                    case 4:
                        actionGenericType = typeof(Action<,,,>);
                        break;
#if NET_FX_4 //See #define NET_FX_4 as the head of this file
                    case 5:
                        actionGenericType = typeof(Action<,,,,>);
                        break;
                    case 6:
                        actionGenericType = typeof(Action<,,,,,>);
                        break;
                    case 7:
                        actionGenericType = typeof(Action<,,,,,,>);
                        break;
                    case 8:
                        actionGenericType = typeof(Action<,,,,,,,>);
                        break;
                    case 9:
                        actionGenericType = typeof(Action<,,,,,,,,>);
                        break;
                    case 10:
                        actionGenericType = typeof(Action<,,,,,,,,,>);
                        break;
                    case 11:
                        actionGenericType = typeof(Action<,,,,,,,,,,>);
                        break;
                    case 12:
                        actionGenericType = typeof(Action<,,,,,,,,,,,>);
                        break;
                    case 13:
                        actionGenericType = typeof(Action<,,,,,,,,,,,,>);
                        break;
                    case 14:
                        actionGenericType = typeof(Action<,,,,,,,,,,,,,>);
                        break;
                    case 15:
                        actionGenericType = typeof(Action<,,,,,,,,,,,,,,>);
                        break;
                    case 16:
                        actionGenericType = typeof(Action<,,,,,,,,,,,,,,,>);
                        break;
#endif
                    default:
                        throw new NotSupportedException("Lambdas may only have up to 16 parameters.");
                }
                return actionGenericType.MakeGenericType(method.GetParameters().Select(_ => _.ParameterType).ToArray());
            }
            Type functionGenericType;
            switch (method.GetParameters().Length) {
                case 0:
                    return typeof(Func<>);
                case 1:
                    functionGenericType = typeof(Func<,>);
                    break;
                case 2:
                    functionGenericType = typeof(Func<,,>);
                    break;
                case 3:
                    functionGenericType = typeof(Func<,,,>);
                    break;
                case 4:
                    functionGenericType = typeof(Func<,,,,>);
                    break;
#if NET_FX_4 //See #define NET_FX_4 as the head of this file
                case 5:
                    functionGenericType = typeof(Func<,,,,,>);
                    break;
                case 6:
                    functionGenericType = typeof(Func<,,,,,,>);
                    break;
                case 7:
                    functionGenericType = typeof(Func<,,,,,,,>);
                    break;
                case 8:
                    functionGenericType = typeof(Func<,,,,,,,,>);
                    break;
                case 9:
                    functionGenericType = typeof(Func<,,,,,,,,,>);
                    break;
                case 10:
                    functionGenericType = typeof(Func<,,,,,,,,,,>);
                    break;
                case 11:
                    functionGenericType = typeof(Func<,,,,,,,,,,,>);
                    break;
                case 12:
                    functionGenericType = typeof(Func<,,,,,,,,,,,,>);
                    break;
                case 13:
                    functionGenericType = typeof(Func<,,,,,,,,,,,,,>);
                    break;
                case 14:
                    functionGenericType = typeof(Func<,,,,,,,,,,,,,,>);
                    break;
                case 15:
                    functionGenericType = typeof(Func<,,,,,,,,,,,,,,,>);
                    break;
                case 16:
                    functionGenericType = typeof(Func<,,,,,,,,,,,,,,,,>);
                    break;
#endif
                default:
                    throw new NotSupportedException("Lambdas may only have up to 16 parameters.");
            }
            var parametersAndReturnType = new Type[method.GetParameters().Length + 1];
            method.GetParameters().Select(_ => _.ParameterType).ToArray().CopyTo(parametersAndReturnType, 0);
            parametersAndReturnType[parametersAndReturnType.Length - 1] = method.ReturnType;
            return functionGenericType.MakeGenericType(parametersAndReturnType);
        }
    }
}