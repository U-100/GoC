using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using GoC.Attributes;
using Jint;
using Jint.Runtime.Interop;
using Unity.Mathematics;
using UnityEngine;

namespace GoC
{
    public static class EngineFactory
    {
        public static Engine Create()
        {
            Engine engine = new Engine(Configure);

            engine.SetValue("log", new Action<object>(Debug.Log));
            engine.SetValue("setTimeout", new Action<Action, int>(SetTimeout));
            engine.SetValue("delay", new Func<int, Task>(Delay));
            
            return engine;
        }
        
        private static void Configure(Options options)
        {
            options.Culture = CultureInfo.InvariantCulture;
            options.Interop.TrackObjectWrapperIdentity = true;
            
            // options.Interop.WrapObjectHandler =
            //     static (engine, target, type) =>
            //     {
            //         //UnityEngine.Debug.Log($"Wrapping {target} as {type}");
            //         return ObjectWrapper.Create(engine, target, type);
            //     };
            
            options.SetTypeResolver(new TypeResolver { MemberFilter = MEMBER_FILTER });
        }

        private static readonly HashSet<Type> ENABLED_TYPES = new()
        {
            typeof(int2),
            typeof(IEnumerable<>),
            typeof(IReadOnlyCollection<>),
            typeof(IReadOnlyDictionary<,>),
        };
        
        private static readonly Predicate<MemberInfo> MEMBER_FILTER = delegate(MemberInfo memberInfo)
        {
            Type declaringType = memberInfo.DeclaringType;
            if(declaringType != null)
            {
                if (declaringType.IsGenericType)
                {
                    if (ENABLED_TYPES.Contains(declaringType.GetGenericTypeDefinition()))
                        return true;
                }
                else if (ENABLED_TYPES.Contains(declaringType))
                    return true;
                
                if (declaringType.GetCustomAttribute<ScriptingApiAttribute>() != null)
                    return true;

                Assembly assembly = declaringType.Assembly;
                if (assembly.GetCustomAttribute<ScriptingApiAttribute>() != null)
                    return true;
            }

            return false;
        };
        
        private static async void SetTimeout(Action callback, int milliseconds)
        {
            await Task.Delay(milliseconds);
            callback();
        }

        private static async Task Delay(int milliseconds)
        {
            await Task.Delay(milliseconds).ConfigureAwait(false);
        }
    }
}
