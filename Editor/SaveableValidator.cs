#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using SaveFramework.Runtime.Core.Conversion;

namespace SaveFramework.Editor
{
    // 这个类不直接引用 SaveFieldAttribute 类型，避免命名空间不一致导致的编译问题
    public class SaveableValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }

        private const string TargetAttributeSimpleName = "SaveFieldAttribute";

        [MenuItem("Tools/SaveFramework/Validate Save Fields")]
        public static void ValidateMenu()
        {
            var issues = ValidateAll();
            if (issues.Count == 0)
            {
                Debug.Log("SaveFramework: 所有带 [SaveField] 的字段/属性类型均受支持。");
            }
            else
            {
                foreach (var i in issues) Debug.LogError(i);
            }
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            var issues = ValidateAll();
            if (issues.Count > 0)
            {
                var msg = "SaveFramework: 存在不支持的保存字段/属性类型，阻止打包：\n" + string.Join("\n", issues);
                throw new BuildFailedException(msg);
            }
        }

        private static List<string> ValidateAll()
        {
            var issues = new List<string>();

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = asm.GetTypes(); }
                catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t != null).ToArray(); }

                foreach (var t in types)
                {
                    if (t == null || t.IsGenericTypeDefinition) continue;

                    // 字段
                    var fields = t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    foreach (var f in fields)
                    {
                        if (!HasSaveFieldAttribute(f)) continue;
                        var ft = f.FieldType;
                        if (!ConverterRegistry.IsSupported(ft))
                            issues.Add(string.Format("{0}.{1} : {2}", t.FullName, f.Name, ft.FullName));
                    }

                    // 属性（只检查可读可写、非索引器）
                    var props = t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    foreach (var p in props)
                    {
                        if (!HasSaveFieldAttribute(p)) continue;
                        if (!p.CanRead || !p.CanWrite) continue;
                        if (p.GetIndexParameters().Length > 0) continue;

                        var pt = p.PropertyType;
                        if (!ConverterRegistry.IsSupported(pt))
                            issues.Add(string.Format("{0}.{1} : {2}", t.FullName, p.Name, pt.FullName));
                    }
                }
            }

            return issues;
        }

        private static bool HasSaveFieldAttribute(MemberInfo member)
        {
            // 按“类型名等于 SaveFieldAttribute”来判断，忽略命名空间差异
            var attrs = member.GetCustomAttributes(inherit: true);
            foreach (var a in attrs)
            {
                var at = a.GetType();
                if (string.Equals(at.Name, TargetAttributeSimpleName, StringComparison.Ordinal))
                    return true;
            }
            return false;
        }
    }
}
#endif