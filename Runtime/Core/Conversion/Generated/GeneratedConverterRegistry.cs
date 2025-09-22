

// 此文件由 SaveFramework.Editor.ConverterRegistryCodeGen 自动生成.
// 请勿手动修改此文件.


using UnityEngine;

namespace SaveFramework.Runtime.Core.Conversion.Generated
{
    // 在编辑器与运行时两侧都确保已注册
    [UnityEditor.InitializeOnLoad]
    public static class GeneratedConverterRegistry
    {
        static GeneratedConverterRegistry()
        {
#if UNITY_EDITOR
            // 编辑器脚本重载后立即注册
            RegisterAll();
#endif
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeInit()
        {
            RegisterAll();
        }

        public static void RegisterAll()
        {
            // 若已注册任一常见类型，则跳过（避免重复）
            if (SaveFramework.Runtime.Core.Conversion.ConverterRegistry.HasConverter(typeof(UnityEngine.Vector3)))
                return;

            ConverterRegistry.RegisterConverter(new global::SaveFramework.Runtime.Core.Conversion.BuiltIn.BoundsConverter());
            ConverterRegistry.RegisterConverter(new global::SaveFramework.Runtime.Core.Conversion.BuiltIn.ColorConverter());
            ConverterRegistry.RegisterConverter(new global::SaveFramework.Runtime.Core.Conversion.BuiltIn.QuaternionConverter());
            ConverterRegistry.RegisterConverter(new global::SaveFramework.Runtime.Core.Conversion.BuiltIn.Vector2Converter());
            ConverterRegistry.RegisterConverter(new global::SaveFramework.Runtime.Core.Conversion.BuiltIn.Vector3Converter());
            ConverterRegistry.RegisterConverter(new global::SaveFramework.Runtime.Core.Conversion.BuiltIn.Vector4Converter());
        }
    }
}