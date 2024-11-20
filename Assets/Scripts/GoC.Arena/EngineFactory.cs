using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using GoC.Attributes;
using GoC.ScriptingApi;
using Jint;
using Jint.Runtime.Interop;
using Unity.Mathematics;
using UnityEngine;

namespace GoC.Arena
{
    public static class EngineFactory
    {
        public static Engine Create()
        {
            Engine engine = new Engine(Configure);

            engine.SetValue("log", new Action<object>(Debug.Log));
            engine.SetValue("Result", TypeReference.CreateTypeReference<Result>(engine));
            engine.SetValue(nameof(Substance), TypeReference.CreateTypeReference<Substance>(engine));
            return engine;
        }
        
        private static void Configure(Options options)
        {
            options.Culture = CultureInfo.InvariantCulture;
            options.Interop.TrackObjectWrapperIdentity = true;
            options.SetTypeResolver(
                new TypeResolver
                {
                    MemberFilter = MEMBER_FILTER
                }
            );
        }

        private static readonly HashSet<Type> ENABLED_TYPES = new()
        {
            typeof(int2),
            typeof(IEnumerable<>),
            typeof(IReadOnlyCollection<>),
            typeof(IReadOnlyDictionary<,>),
        };
        
        // member filter decides which apis should be exposed to javascript by jint engine.
        // We expose apis of ENABLED_TYPES plus apis from assemblies and types marked with ScriptingApiAttribute
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
    }
}
