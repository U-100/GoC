using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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

            engine.SetValue(nameof(Result), TypeReference.CreateTypeReference<Result>(engine));
            
            engine.SetValue(nameof(Substance), TypeReference.CreateTypeReference<Substance>(engine));
            
            engine.SetValue(nameof(int2), TypeReference.CreateTypeReference<int2>(engine));
            engine.SetValue("distance", new Func<int2, int2, float>((lhs, rhs) => math.distance(lhs, rhs)));
            engine.SetValue("dot", new Func<int2, int2, float>((lhs, rhs) => math.dot(lhs, rhs)));
            engine.SetValue("length", new Func<int2, float>(lhs => math.length(lhs)));

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
            options.EnableModules(Path.Combine(Directory.GetCurrentDirectory(), "JavaScript"));
            options.AllowClr(typeof(math).Assembly);
            options.Interop.AllowOperatorOverloading = true;
        }

        private static readonly HashSet<Type> ENABLED_TYPES = new()
        {
            typeof(int2),
            typeof(IEnumerable<>),
            typeof(IReadOnlyCollection<>),
            typeof(IReadOnlyList<>),
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
                    declaringType = declaringType.GetGenericTypeDefinition();

                if (ENABLED_TYPES.Contains(declaringType))
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
