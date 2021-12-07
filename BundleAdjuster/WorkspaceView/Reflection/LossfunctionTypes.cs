using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ceresdotnet;

namespace BundleAdjuster.WorkspaceView.Reflection
{
    internal delegate T ObjectActivator<out T>(params object[] args);
    internal static class LossfunctionTypes {
        public static readonly List<Tuple<Type, ObjectActivator<ceresdotnet.LossFunction>, ParameterInfo[]>> _losstypes
            = new List<Tuple<Type, ObjectActivator<ceresdotnet.LossFunction>, ParameterInfo[]>>();
        
        static LossfunctionTypes() {
            var type = typeof(ceresdotnet.LossFunction);
            foreach (var LossfunctionType in AppDomain.CurrentDomain.GetAssemblies().SelectMany(s=>s.GetTypes()).Where(p=> type.IsAssignableFrom(p) && !p.IsAbstract && p.IsClass)) {
                var ctor = LossfunctionType.GetConstructors().First();
                var activator = GetActivator<ceresdotnet.LossFunction>(ctor);
                _losstypes.Add(new Tuple<Type, ObjectActivator<LossFunction>, ParameterInfo[]>(type,activator,ctor.GetParameters()));
            }
            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
        }

        private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            var type = typeof(ceresdotnet.LossFunction);
            foreach (var LossfunctionType in args.LoadedAssembly.GetTypes().Where(p => type.IsAssignableFrom(p) && !p.IsAbstract && p.IsClass))
            {
                var ctor = LossfunctionType.GetConstructors().First();
                var activator = GetActivator<ceresdotnet.LossFunction>(ctor);
                _losstypes.Add(new Tuple<Type, ObjectActivator<LossFunction>, ParameterInfo[]>(type, activator, ctor.GetParameters()));
            }
        }

        private static ObjectActivator<T> GetActivator<T>(ConstructorInfo ctor)
        {
            Type type = ctor.DeclaringType;
            ParameterInfo[] paramsInfo = ctor.GetParameters();

            //create a single param of type object[]
            ParameterExpression param =
                Expression.Parameter(typeof(object[]), "args");

            Expression[] argsExp =
                new Expression[paramsInfo.Length];

            //pick each arg from the params array 
            //and create a typed expression of them
            for (int i = 0; i < paramsInfo.Length; i++)
            {
                Expression index = Expression.Constant(i);
                Type paramType = paramsInfo[i].ParameterType;

                Expression paramAccessorExp =
                    Expression.ArrayIndex(param, index);

                Expression paramCastExp =
                    Expression.Convert(paramAccessorExp, paramType);

                argsExp[i] = paramCastExp;
            }

            //make a NewExpression that calls the
            //ctor with the args we just created
            NewExpression newExp = Expression.New(ctor, argsExp);

            //create a lambda with the New
            //Expression as body and our param object[] as arg
            LambdaExpression lambda =
                Expression.Lambda(typeof(ObjectActivator<T>), newExp, param);

            //compile it
            ObjectActivator<T> compiled = (ObjectActivator<T>)lambda.Compile();
            return compiled;
        }
    }
    
}
