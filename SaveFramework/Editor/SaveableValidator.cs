#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using SaveFramework.Runtime.Core.Conversion;

namespace SaveFramework.Editor
{
    // 这个类不直接引用具体属性类型，避免命名空间不一致导致的编译问题
    public class SaveableValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }

        // 运行时反射时使用的属性简单名（C#里 [Save] 实际类型名是 SaveAttribute）
        private const string TargetAttributeSimpleName = "SaveAttribute";

        [MenuItem("Tools/Save Framework/验证保存字段")]
        public static void ValidateMenu()
        {
            var issues = ValidateAllDetailed();

            if (issues.Count == 0)
            {
                Debug.Log("SaveFramework: 所有带 [Save]/[SaveAttribute] 的字段/属性类型均受支持。");
                return;
            }

            SaveableIssuesWindow.Show(issues);

            foreach (var i in issues)
            {
                var msg = i.ToLogMessage();
                if (i.Script != null)
                    Debug.LogError(msg, i.Script);
                else
                    Debug.LogError(msg);
            }
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            var issues = ValidateAllDetailed();
            if (issues.Count > 0)
            {
                var lines = issues.Select(i => i.ToBuildMessage());
                var msg = "SaveFramework: 存在不支持的保存字段/属性类型，阻止打包：\n" + string.Join("\n", lines);
                throw new BuildFailedException(msg);
            }
        }

        private static List<Issue> ValidateAllDetailed()
        {
            var issues = new List<Issue>();

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
                        if (!HasTargetAttribute(f)) continue;

                        var ft = f.FieldType;
                        if (!ConverterRegistry.IsSupported(ft))
                        {
                            var issue = CreateIssue(t, MemberKind.Field, f.Name, ft);
                            issues.Add(issue);
                        }
                    }

                    // 属性（只检查可读可写、非索引器）
                    var props = t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    foreach (var p in props)
                    {
                        if (!HasTargetAttribute(p)) continue;
                        if (!p.CanRead || !p.CanWrite) continue;
                        if (p.GetIndexParameters().Length > 0) continue;

                        var pt = p.PropertyType;
                        if (!ConverterRegistry.IsSupported(pt))
                        {
                            var issue = CreateIssue(t, MemberKind.Property, p.Name, pt);
                            issues.Add(issue);
                        }
                    }
                }
            }

            return issues;
        }

        private static Issue CreateIssue(Type declaringType, MemberKind kind, string memberName, Type dataType)
        {
            TryGetScriptAndLine(declaringType, memberName, kind, out var script, out var assetPath, out var line);
            return new Issue
            {
                DeclaringTypeFullName = declaringType.FullName,
                MemberName = memberName,
                Kind = kind,
                DataTypeFullName = dataType.FullName,
                AssetPath = assetPath,
                Line = line,
                Script = script
            };
        }

        private static bool HasTargetAttribute(MemberInfo member)
        {
            // 按“类型名等于 SaveAttribute”来判断，忽略命名空间差异
            var attrs = member.GetCustomAttributes(inherit: true);
            foreach (var a in attrs)
            {
                var at = a.GetType();
                if (string.Equals(at.Name, TargetAttributeSimpleName, StringComparison.Ordinal))
                    return true;
            }
            return false;
        }

        // 通过 MonoScript 找到定义该类型的脚本资产，并尝试用正则定位成员声明的行号
        private static bool TryGetScriptAndLine(Type declaringType, string memberName, MemberKind kind, out MonoScript script, out string assetPath, out int line)
        {
            script = FindMonoScriptForType(declaringType);
            if (script == null)
            {
                assetPath = string.Empty;
                line = -1;
                return false;
            }

            assetPath = AssetDatabase.GetAssetPath(script);
            line = GuessMemberLine(assetPath, memberName, kind);
            return true;
        }

        private static MonoScript FindMonoScriptForType(Type t)
        {
            // 仅对项目内脚本有效，对外部程序集（包、DLL）可能返回不到
            try
            {
                foreach (var ms in MonoImporter.GetAllRuntimeMonoScripts())
                {
                    if (ms == null) continue;
                    var cls = ms.GetClass();
                    if (cls == t) return ms;
                }
            }
            catch { /* 某些 Unity 版本/环境可能抛异常，忽略 */ }
            return null;
        }

        private static int GuessMemberLine(string path, string memberName, MemberKind kind)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return -1;

            // 简单启发式：先定位带属性声明块的成员；否则退化为按成员名的首次出现
            // 支持 [Save] 或 [SaveAttribute]，允许参数与空白
            var attrPattern = @"\[\s*Save(Attribute)?\s*(\([^\]]*\))?\s*\]";
            // 字段声明大致：<type> <name>[ = ...]?;
            var fieldPattern = $@"(?m)^{attrPattern}\s*(?:\[.*\]\s*)*.*\b{Regex.Escape(memberName)}\b\s*(?:[=;]|{{)";
            // 属性声明大致：<type> <name>\s*{ ... }
            var propPattern = $@"(?m)^{attrPattern}\s*(?:\[.*\]\s*)*.*\b{Regex.Escape(memberName)}\b\s*{{";

            var text = File.ReadAllText(path);

            // 先尝试按带属性的匹配
            var pattern = kind == MemberKind.Field ? fieldPattern : propPattern;
            var m = Regex.Match(text, pattern);
            if (m.Success) return GetLineFromIndex(text, m.Index);

            // 退化：找第一个整词匹配的行（避免匹配方法调用等，尽量排除括号后的情形）
            var lines = File.ReadAllLines(path);
            var word = new Regex(@"\b" + Regex.Escape(memberName) + @"\b");
            for (int i = 0; i < lines.Length; i++)
            {
                if (!word.IsMatch(lines[i])) continue;

                var l = lines[i];
                // 排除典型的方法/调用行
                if (l.Contains(memberName + "(")) continue;
                return i + 1; // Unity 是 1-based 行号
            }

            return -1;
        }

        private static int GetLineFromIndex(string text, int index)
        {
            // 统计 index 之前的换行符数量
            int line = 1; // 1-based
            for (int i = 0; i < index && i < text.Length; i++)
            {
                if (text[i] == '\n') line++;
            }
            return line;
        }

        private enum MemberKind { Field, Property }

        private class Issue
        {
            public string DeclaringTypeFullName;
            public string MemberName;
            public MemberKind Kind;
            public string DataTypeFullName;

            public string AssetPath;   // 可为空
            public int Line;           // -1 表示未知
            public MonoScript Script;  // 可为空

            public string ToLogMessage()
            {
                var where = string.IsNullOrEmpty(AssetPath) ? "(无法定位脚本文件)" :
                    (Line > 0 ? $"{AssetPath}:{Line}" : AssetPath);
                return $"[SaveFramework] 不受支持的{(Kind == MemberKind.Field ? "字段" : "属性")}类型: {DeclaringTypeFullName}.{MemberName} : {DataTypeFullName} @ {where}";
            }

            public string ToBuildMessage()
            {
                var where = string.IsNullOrEmpty(AssetPath) ? "" :
                    (Line > 0 ? $" ({AssetPath}:{Line})" : $" ({AssetPath})");
                return $"{DeclaringTypeFullName}.{MemberName} : {DataTypeFullName}{where}";
            }
        }
    }

    public class SaveableIssuesWindow : EditorWindow
    {
        private Vector2 _scroll;
        private List<object> _raw; // 仅为持有引用
        private List<Item> _items;

        private class Item
        {
            public string Title;
            public string Path;
            public int Line;
            public MonoScript Script;
        }

        public static void Show(IEnumerable<object> issues)
        {
            // 将匿名 Issue 转换为窗口条目
            var w = GetWindow<SaveableIssuesWindow>(utility: true, title: "Save Issues");
            w.minSize = new Vector2(680, 240);

            w._raw = issues.ToList();
            w._items = new List<Item>();

            var type = typeof(SaveableValidator);
            var issueType = type.GetNestedType("Issue", BindingFlags.NonPublic);
            var fDeclType = issueType.GetField("DeclaringTypeFullName");
            var fMember = issueType.GetField("MemberName");
            var fKind = issueType.GetField("Kind");
            var fDataType = issueType.GetField("DataTypeFullName");
            var fPath = issueType.GetField("AssetPath");
            var fLine = issueType.GetField("Line");
            var fScript = issueType.GetField("Script");

            foreach (var i in w._raw)
            {
                string title = $"{fDeclType.GetValue(i)}.{fMember.GetValue(i)} : {fDataType.GetValue(i)}";
                var item = new Item
                {
                    Title = title,
                    Path = (string)fPath.GetValue(i) ?? "",
                    Line = (int)fLine.GetValue(i),
                    Script = (MonoScript)fScript.GetValue(i),
                };
                w._items.Add(item);
            }

            w.Show();
            w.Focus();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("检测到以下不受支持的保存字段/属性：", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            using (var scroll = new EditorGUILayout.ScrollViewScope(_scroll))
            {
                _scroll = scroll.scrollPosition;

                if (_items == null || _items.Count == 0)
                {
                    EditorGUILayout.HelpBox("没有发现问题。", MessageType.Info);
                    return;
                }

                foreach (var it in _items)
                {
                    using (new EditorGUILayout.VerticalScope("box"))
                    {
                        EditorGUILayout.LabelField(it.Title, EditorStyles.wordWrappedLabel);

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            if (!string.IsNullOrEmpty(it.Path))
                            {
                                // 让 ObjectField 占满剩余宽度，并把路径放到 tooltip 中，避免显示被截断
                                float oldLabelWidth = EditorGUIUtility.labelWidth;
                                EditorGUIUtility.labelWidth = 48f; // 更短的标签宽度
                                EditorGUILayout.ObjectField(
                                    new GUIContent("脚本", it.Path),
                                    it.Script,
                                    typeof(MonoScript),
                                    false,
                                    GUILayout.ExpandWidth(true)
                                );
                                EditorGUIUtility.labelWidth = oldLabelWidth;
                                GUI.enabled = it.Script != null;
                                if (GUILayout.Button(it.Line > 0 ? $"打开脚本 (行 {it.Line})" : "打开脚本", GUILayout.Width(140)))
                                {
                                    if (it.Script != null)
                                    {
                                        if (it.Line > 0) AssetDatabase.OpenAsset(it.Script, it.Line);
                                        else AssetDatabase.OpenAsset(it.Script);
                                    }
                                }
                                GUI.enabled = true;
                            }
                            else
                            {
                                EditorGUILayout.HelpBox("无法定位到脚本文件（可能来自外部程序集）。", MessageType.None);
                            }
                        }
                    }
                }
            }

            EditorGUILayout.Space(6);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("刷新"))
                {
                    // 重新跑一遍校验
                    var mi = typeof(SaveableValidator).GetMethod("ValidateAllDetailed", BindingFlags.NonPublic | BindingFlags.Static);
                    var issues = (System.Collections.IEnumerable)mi.Invoke(null, null);
                    Show(issues.Cast<object>());
                    // Close(); // Show 会打开新窗口，这里关闭旧窗口
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("关闭", GUILayout.Width(80))) Close();
            }
        }
    }
}
#endif