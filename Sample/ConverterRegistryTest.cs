using System;
using UnityEngine;
using SaveFramework.Runtime.Core.Conversion;
using SaveFramework.Runtime.Core.Conversion.BuiltIn;

namespace SaveFramework.Test
{
    /// <summary>
    /// 用于验证转换器注册表系统正常工作的简单测试组件
    /// </summary>
    public class ConverterRegistryTest : MonoBehaviour
    {
        [ContextMenu("Test Converter Registry")]
        public void TestConverterRegistry()
        {
            Debug.Log("=== Testing Converter Registry System ===");
            
            // Initialize registry
            ConverterRegistry.Initialize();
            
            // Test 1: Check if converters are registered
            TestConverterRegistration();
            
            // Test 2: Test Vector3 conversion
            TestVector3Conversion();
            
            // Test 3: Test Vector3[] array conversion
            TestVector3ArrayConversion();
            
            // Test 4: Test Color conversion
            TestColorConversion();
            
            // Test 5: Test Color[] array conversion
            TestColorArrayConversion();
            
            // Test 6: Test NumericArrayReader with various formats
            TestNumericArrayReader();
            
            Debug.Log("=== Converter Registry Tests Complete ===");
        }
        
        private void TestConverterRegistration()
        {
            Debug.Log("--- Testing Converter Registration ---");
            
            var supportedTypes = ConverterRegistry.GetSupportedTypes();
            Debug.Log($"Found {supportedTypes.Length} registered converters:");
            
            foreach (var type in supportedTypes)
            {
                Debug.Log($"  - {type.Name}");
            }
            
            // Check specific types
            Assert(ConverterRegistry.HasConverter(typeof(Vector2)), "Vector2 converter should be registered");
            Assert(ConverterRegistry.HasConverter(typeof(Vector3)), "Vector3 converter should be registered");
            Assert(ConverterRegistry.HasConverter(typeof(Vector4)), "Vector4 converter should be registered");
            Assert(ConverterRegistry.HasConverter(typeof(Quaternion)), "Quaternion converter should be registered");
            Assert(ConverterRegistry.HasConverter(typeof(Color)), "Color converter should be registered");
        }
        
        private void TestVector3Conversion()
        {
            Debug.Log("--- Testing Vector3 Conversion ---");
            
            var converter = ConverterRegistry.GetConverter(typeof(Vector3));
            Assert(converter != null, "Vector3 converter should exist");
            
            var original = new Vector3(1.5f, 2.5f, 3.5f);
            var jsonValue = converter.ToJsonValue(original);
            var restored = (Vector3)converter.FromJsonValue(jsonValue);
            
            Debug.Log($"Original: {original}, JSON: {string.Join(",", (float[])jsonValue)}, Restored: {restored}");
            Assert(Vector3.Distance(original, restored) < 0.001f, "Vector3 conversion should be accurate");
        }
        
        private void TestVector3ArrayConversion()
        {
            Debug.Log("--- Testing Vector3[] Array Conversion ---");
            
            var original = new Vector3[] { 
                new Vector3(1, 2, 3), 
                new Vector3(4, 5, 6), 
                Vector3.zero 
            };
            
            var jsonValue = SaveFramework.Runtime.Core.Converters.ToJsonValue(original, typeof(Vector3[]));
            var restored = (Vector3[])SaveFramework.Runtime.Core.Converters.FromJsonValue(jsonValue, typeof(Vector3[]));
            
            Debug.Log($"Original array length: {original.Length}, Restored array length: {restored.Length}");
            Assert(original.Length == restored.Length, "Array lengths should match");
            
            for (int i = 0; i < original.Length; i++)
            {
                Debug.Log($"  [{i}] Original: {original[i]}, Restored: {restored[i]}");
                Assert(Vector3.Distance(original[i], restored[i]) < 0.001f, $"Vector3[{i}] should match");
            }
        }
        
        private void TestColorConversion()
        {
            Debug.Log("--- Testing Color Conversion ---");
            
            var converter = ConverterRegistry.GetConverter(typeof(Color));
            Assert(converter != null, "Color converter should exist");
            
            var original = new Color(0.8f, 0.2f, 0.6f, 0.9f);
            var jsonValue = converter.ToJsonValue(original);
            var restored = (Color)converter.FromJsonValue(jsonValue);
            
            Debug.Log($"Original: {original}, JSON: {string.Join(",", (float[])jsonValue)}, Restored: {restored}");
            Assert(Mathf.Abs(original.r - restored.r) < 0.001f, "Color R should match");
            Assert(Mathf.Abs(original.g - restored.g) < 0.001f, "Color G should match");
            Assert(Mathf.Abs(original.b - restored.b) < 0.001f, "Color B should match");
            Assert(Mathf.Abs(original.a - restored.a) < 0.001f, "Color A should match");
        }
        
        private void TestColorArrayConversion()
        {
            Debug.Log("--- Testing Color[] Array Conversion ---");
            
            var original = new Color[] { 
                Color.red, 
                Color.green, 
                Color.blue,
                new Color(0.5f, 0.5f, 0.5f, 0.5f)
            };
            
            var jsonValue = SaveFramework.Runtime.Core.Converters.ToJsonValue(original, typeof(Color[]));
            var restored = (Color[])SaveFramework.Runtime.Core.Converters.FromJsonValue(jsonValue, typeof(Color[]));
            
            Debug.Log($"Original array length: {original.Length}, Restored array length: {restored.Length}");
            Assert(original.Length == restored.Length, "Array lengths should match");
            
            for (int i = 0; i < original.Length; i++)
            {
                Debug.Log($"  [{i}] Original: {original[i]}, Restored: {restored[i]}");
                Assert(Mathf.Abs(original[i].r - restored[i].r) < 0.001f, $"Color[{i}].r should match");
                Assert(Mathf.Abs(original[i].g - restored[i].g) < 0.001f, $"Color[{i}].g should match");
                Assert(Mathf.Abs(original[i].b - restored[i].b) < 0.001f, $"Color[{i}].b should match");
                Assert(Mathf.Abs(original[i].a - restored[i].a) < 0.001f, $"Color[{i}].a should match");
            }
        }
        
        private void TestNumericArrayReader()
        {
            Debug.Log("--- Testing NumericArrayReader ---");
            
            // Test with Unity types
            var result1 = NumericArrayReader.ReadFloatArray(new Vector3(1, 2, 3));
            Debug.Log($"Vector3(1,2,3) -> [{string.Join(",", result1)}]");
            Assert(result1.Length == 3 && result1[0] == 1f && result1[1] == 2f && result1[2] == 3f, "Vector3 parsing should work");
            
            var result2 = NumericArrayReader.ReadFloatArray(Color.red);
            Debug.Log($"Color.red -> [{string.Join(",", result2)}]");
            Assert(result2.Length == 4 && result2[0] == 1f && result2[1] == 0f && result2[2] == 0f && result2[3] == 1f, "Color parsing should work");
            
            // Test with arrays
            var result3 = NumericArrayReader.ReadFloatArray(new float[] { 4.5f, 5.5f, 6.5f });
            Debug.Log($"float[4.5,5.5,6.5] -> [{string.Join(",", result3)}]");
            Assert(result3.Length == 3 && result3[0] == 4.5f && result3[1] == 5.5f && result3[2] == 6.5f, "Float array parsing should work");
        }
        
        private void Assert(bool condition, string message)
        {
            if (condition)
            {
                Debug.Log($"✓ PASS: {message}");
            }
            else
            {
                Debug.LogError($"✗ FAIL: {message}");
            }
        }
    }
}