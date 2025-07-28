using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using GoC.Attributes;
using UnityEditor;
using UnityEngine;

public static class ScriptingApiDocumentationGenerator
{
    [MenuItem("GoC/Documentation/LogToUnityConsole")]
    public static void LogToUnityConsole() => Debug.Log(GenerateDocs());

    [MenuItem("GoC/Documentation/Dump to file")]
    public static void DumpToFile()
    {
        string path = GetDocsPath();
        File.WriteAllText(path, GenerateDocs());
        AssetDatabase.Refresh();
    }

    public static string GetDocsPath() => Path.Combine(Application.streamingAssetsPath, "Docs/ScriptingApi.md");
    
    public static string GenerateDocs()
    {
        StringBuilder sb = new StringBuilder();

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.GetCustomAttributes(typeof(ScriptingApiAttribute)).Any())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.Namespace != null && type.Namespace.Contains("GoC"))
                    {
                        GenerateTypeDocs(type, sb);
                        sb.AppendLine();
                    }
                }
            }
        }

        return sb.ToString();
    }

    private static void GenerateTypeDocs(Type type, StringBuilder sb)
    {
        switch (type)
        {
            case { IsEnum: true }: GenerateEnumDocs(type, sb); break;
            
            case { IsInterface: true }: GenerateInterfaceDocs(type, sb); break;
            
            case { IsClass: true }: GenerateClassDocs(type, sb); break;
        }
    }

    private static void GenerateEnumDocs(Type enumType, StringBuilder sb)
    {
        string typeName = FormatTypeName(enumType);
        sb.AppendLine($"Enum {typeName}");
        
        foreach (object value in Enum.GetValues(enumType))
            sb.AppendLine($"  {typeName}.{value} = {(int)value}");
    }

    private static void GenerateInterfaceDocs(Type interfaceType, StringBuilder sb)
    {
        sb.AppendLine($"interface {FormatTypeName(interfaceType)}");
        
        foreach (MemberInfo memberInfo in interfaceType.GetMembers())
            if (memberInfo.DeclaringType == interfaceType)
                GenerateMemberDocs(memberInfo, sb);
    }

    private static void GenerateClassDocs(Type classType, StringBuilder sb)
    {
        sb.AppendLine($"class {FormatTypeName(classType)}");
        
        foreach (MemberInfo memberInfo in classType.GetMembers())
            if (memberInfo.DeclaringType == classType)
                GenerateMemberDocs(memberInfo, sb);
    }

    private static void GenerateMemberDocs(MemberInfo memberInfo, StringBuilder sb)
    {
        Type declaringType = memberInfo.DeclaringType;

        if (declaringType == null)
            return;
        
        string declaringTypeName = FormatTypeName(declaringType);
        
        switch (memberInfo)
        {
            case FieldInfo fieldInfo:
                sb.AppendLine($"  {FormatTypeName(fieldInfo.FieldType)} {fieldInfo.Name}");
                break;

            case PropertyInfo propertyInfo:
                sb.AppendLine($"  {FormatTypeName(propertyInfo.PropertyType)} {propertyInfo.Name}");
                break;
                    
            case MethodInfo { IsSpecialName: false } methodInfo:
                sb.AppendLine($"  {FormatTypeName(methodInfo.ReturnType)} {methodInfo.Name}({FormatParameters(methodInfo)})");
                break;
        }
    }

    private static string FormatParameters(MethodInfo methodInfo)
    {
        StringBuilder sb = new StringBuilder();

        ParameterInfo[] parameters = methodInfo.GetParameters();
        
        for (int i = 0; i < methodInfo.GetParameters().Length; i++)
        {
            sb.Append(FormatTypeName(parameters[i].ParameterType));
            sb.Append(" ");
            sb.Append(parameters[i].Name);
            if (i != parameters.Length - 1)
                sb.Append(", ");
        }

        return sb.ToString();
    }
    
    private static string FormatTypeName(Type type)
    {
        StringBuilder sb = new StringBuilder();
        if (type.IsGenericType)
        {
            sb.Append(type.GetGenericTypeDefinition().Name.Substring(0, type.Name.IndexOf('`')));
            sb.Append('<');
            for (int i = 0; i < type.GenericTypeArguments.Length; i++)
            {
                sb.Append(FormatTypeName(type.GenericTypeArguments[i]));
                if (i < type.GenericTypeArguments.Length - 1)
                    sb.Append(',');
            }
            sb.Append('>');
        }
        else
            sb.Append(type.Name); // Start with the innermost type's name

        Type currentType = type.DeclaringType;
        while (currentType != null)
        {
            sb.Insert(0, currentType.Name + "."); // Prepend containing type's name
            currentType = currentType.DeclaringType;
        }

        return sb.ToString();
    }
}
