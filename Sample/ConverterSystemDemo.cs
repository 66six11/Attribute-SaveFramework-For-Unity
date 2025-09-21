using UnityEngine;
using SaveFramework.Runtime.Core;
using SaveFramework.Runtime.Core.Conversion;
using Newtonsoft.Json.Linq;

namespace SaveFramework.Test
{
    /// <summary>
    /// 演示新型模块化转换器系统的改进
    /// </summary>
    public class ConverterSystemDemo : MonoBehaviour
    {
        [ContextMenu("Demo: Converter System Improvements")]
        public void DemoConverterSystemImprovements()
        {
            Debug.Log("=== SaveFramework Converter System Improvements Demo ===");
            
            DemoAutoRegistration();
            DemoDoubleConversionPrevention();
            DemoImprovedArrayParsing();
            DemoRobustNumericArrayReader();
            DemoEditorValidation();
            
            Debug.Log("=== Demo Complete ===");
        }

        private void DemoAutoRegistration()
        {
            Debug.Log("--- 1. Auto-Registration of Converters ---");
            
            ConverterRegistry.Initialize();
            var supportedTypes = ConverterRegistry.GetSupportedTypes();
            
            Debug.Log($"✓ Automatically discovered and registered {supportedTypes.Length} converters:");
            foreach (var type in supportedTypes)
            {
                Debug.Log($"  • {type.Name} converter ready");
            }
            
            Debug.Log("Benefits: No manual registration needed, just add [AutoRegisterConverter] to new converters!\n");
        }

        private void DemoDoubleConversionPrevention()
        {
            Debug.Log("--- 2. Double-Conversion Prevention ---");
            
            var originalVector = new Vector3(1.5f, 2.5f, 3.5f);
            Debug.Log($"Original Vector3: {originalVector}");
            
            // Simulate what happens during save/load
            var jsonValue = Converters.ToJsonValue(originalVector, typeof(Vector3));
            Debug.Log($"JSON Value: [{string.Join(",", (float[])jsonValue)}]");
            
            // Old system would convert again even if already correct type
            // New system checks type first
            var restored = Converters.FromJsonValue(jsonValue, typeof(Vector3));
            Debug.Log($"Restored Vector3: {restored}");
            
            // Test direct type check
            var alreadyCorrectType = Converters.FromJsonValue(originalVector, typeof(Vector3));
            Debug.Log($"Direct assignment (same type): {alreadyCorrectType}");
            
            Debug.Log("Benefits: Prevents unnecessary conversions when value is already correct type!\n");
        }

        private void DemoImprovedArrayParsing()
        {
            Debug.Log("--- 3. Improved Vector3[]/Color[] Array Parsing ---");
            
            var vectorArray = new Vector3[] 
            { 
                new Vector3(1, 2, 3), 
                new Vector3(4, 5, 6),
                Vector3.zero
            };
            
            var colorArray = new Color[] 
            { 
                Color.red, 
                new Color(0.5f, 0.7f, 0.9f, 0.8f),
                Color.clear
            };
            
            // Test array conversion
            var vectorJson = Converters.ToJsonValue(vectorArray, typeof(Vector3[]));
            var colorJson = Converters.ToJsonValue(colorArray, typeof(Color[]));
            
            var restoredVectors = (Vector3[])Converters.FromJsonValue(vectorJson, typeof(Vector3[]));
            var restoredColors = (Color[])Converters.FromJsonValue(colorJson, typeof(Color[]));
            
            Debug.Log($"Vector3[] conversion: {vectorArray.Length} -> {restoredVectors.Length} elements");
            Debug.Log($"Color[] conversion: {colorArray.Length} -> {restoredColors.Length} elements");
            
            for (int i = 0; i < vectorArray.Length; i++)
            {
                Debug.Log($"  Vector[{i}]: {vectorArray[i]} -> {restoredVectors[i]}");
            }
            
            for (int i = 0; i < colorArray.Length; i++)
            {
                Debug.Log($"  Color[{i}]: {colorArray[i]} -> {restoredColors[i]}");
            }
            
            Debug.Log("Benefits: Robust array handling with improved element-wise conversion!\n");
        }

        private void DemoRobustNumericArrayReader()
        {
            Debug.Log("--- 4. Robust NumericArrayReader ---");
            
            // Test various input formats
            var testCases = new (string name, object input)[]
            {
                ("Unity Vector3", new Vector3(1, 2, 3)),
                ("Unity Color", Color.red),
                ("Float Array", new float[] { 4.5f, 5.5f, 6.5f }),
                ("Int Array", new int[] { 7, 8, 9 }),
                ("Double Array", new double[] { 10.1, 11.2, 12.3 }),
                ("JArray (simulated)", CreateMockJArray(new float[] { 13, 14, 15 })),
                ("Single Value", 42.0f)
            };
            
            foreach (var (name, input) in testCases)
            {
                var result = NumericArrayReader.ReadFloatArray(input);
                Debug.Log($"  {name}: [{string.Join(",", result)}]");
            }
            
            Debug.Log("Benefits: Handles all JSON formats, Unity types, arrays, and edge cases!\n");
        }

        private void DemoEditorValidation()
        {
            Debug.Log("--- 5. Editor-Time Validation ---");
            
            Debug.Log("Editor validation features available:");
            Debug.Log("  • Inspector warnings for unsupported field types");
            Debug.Log("  • Menu: Tools/SaveFramework/Validate All Saveable Types");
            Debug.Log("  • Menu: Tools/SaveFramework/List Supported Types");
            Debug.Log("  • Build-time validation prevents shipping with unsupported types");
            Debug.Log("  • Custom property drawer shows red fields for unsupported types");
            
            Debug.Log("Benefits: Catch serialization issues early in development!\n");
        }

        private object CreateMockJArray(float[] values)
        {
            // Simulate JArray structure for demo purposes
            return values; // In real usage, this would be a JArray
        }

        [ContextMenu("Demo: Complex Array Scenarios")]
        public void DemoComplexArrayScenarios()
        {
            Debug.Log("=== Complex Array Scenarios Demo ===");
            
            // Test nested arrays and edge cases
            var quaternionArray = new Quaternion[]
            {
                Quaternion.identity,
                Quaternion.Euler(90, 0, 0),
                new Quaternion(0.1f, 0.2f, 0.3f, 0.4f),
                Quaternion.Euler(0, 180, 270)
            };
            
            Debug.Log("Testing Quaternion[] with complex rotations:");
            var jsonValue = Converters.ToJsonValue(quaternionArray, typeof(Quaternion[]));
            var restored = (Quaternion[])Converters.FromJsonValue(jsonValue, typeof(Quaternion[]));
            
            for (int i = 0; i < quaternionArray.Length; i++)
            {
                var original = quaternionArray[i];
                var restoredQ = restored[i];
                var angleDiff = Quaternion.Angle(original, restoredQ);
                Debug.Log($"  [{i}] Original: {original}, Restored: {restoredQ}, Angle Diff: {angleDiff:F3}°");
            }
            
            // Test empty arrays
            var emptyVectorArray = new Vector3[0];
            var emptyJson = Converters.ToJsonValue(emptyVectorArray, typeof(Vector3[]));
            var emptyRestored = (Vector3[])Converters.FromJsonValue(emptyJson, typeof(Vector3[]));
            Debug.Log($"Empty array handling: {emptyVectorArray.Length} -> {emptyRestored.Length}");
            
            Debug.Log("=== Complex Array Demo Complete ===");
        }
    }
}