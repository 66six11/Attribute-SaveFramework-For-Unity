using UnityEngine;
using SaveFramework.Runtime.Core;
using SaveFramework.Runtime.Core.Conversion;

namespace SaveFramework.Test
{
    /// <summary>
    /// Final validation script to verify the modular converter system works correctly
    /// </summary>
    public class FinalValidation : MonoBehaviour
    {
        [ContextMenu("Run Complete Validation")]
        public void RunCompleteValidation()
        {
            Debug.Log("=== SaveFramework Modular Converter System - Final Validation ===");
            
            bool allTestsPassed = true;
            
            allTestsPassed &= ValidateConverterRegistration();
            allTestsPassed &= ValidateDoubleConversionPrevention();
            allTestsPassed &= ValidateArrayConversions();
            allTestsPassed &= ValidateNumericArrayReader();
            allTestsPassed &= ValidateConverterFacade();
            
            if (allTestsPassed)
            {
                Debug.Log("🎉 ALL TESTS PASSED! The modular converter system is working correctly.");
                Debug.Log("✅ Features implemented:");
                Debug.Log("  • Pluggable converter registry with auto-registration");
                Debug.Log("  • Modular Vector/Color/Quaternion converters");
                Debug.Log("  • Double-conversion prevention");
                Debug.Log("  • Improved Vector3[]/Color[] array deserialization");
                Debug.Log("  • Robust NumericArrayReader for various JSON formats");
                Debug.Log("  • Editor-time validation system");
                Debug.Log("  • Type safety guards");
            }
            else
            {
                Debug.LogError("❌ Some tests failed. Please check the implementation.");
            }
            
            Debug.Log("=== Final Validation Complete ===");
        }

        private bool ValidateConverterRegistration()
        {
            Debug.Log("--- Validating Converter Registration ---");
            
            try
            {
                ConverterRegistry.Initialize();
                
                var requiredTypes = new System.Type[]
                {
                    typeof(Vector2), typeof(Vector3), typeof(Vector4),
                    typeof(Quaternion), typeof(Color)
                };
                
                foreach (var type in requiredTypes)
                {
                    if (!ConverterRegistry.HasConverter(type))
                    {
                        Debug.LogError($"❌ Missing converter for {type.Name}");
                        return false;
                    }
                    Debug.Log($"✓ {type.Name} converter registered");
                }
                
                Debug.Log("✅ Converter registration validation passed");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Converter registration failed: {ex.Message}");
                return false;
            }
        }

        private bool ValidateDoubleConversionPrevention()
        {
            Debug.Log("--- Validating Double-Conversion Prevention ---");
            
            try
            {
                var vector = new Vector3(1.5f, 2.5f, 3.5f);
                
                // Test that passing an already-correct-type value doesn't get double-converted
                var result = Converters.FromJsonValue(vector, typeof(Vector3));
                
                if (result is Vector3 resultVector && Vector3.Distance(vector, resultVector) < 0.001f)
                {
                    Debug.Log("✓ Double-conversion prevention works");
                    Debug.Log("✅ Double-conversion prevention validation passed");
                    return true;
                }
                else
                {
                    Debug.LogError("❌ Double-conversion prevention failed");
                    return false;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Double-conversion prevention test failed: {ex.Message}");
                return false;
            }
        }

        private bool ValidateArrayConversions()
        {
            Debug.Log("--- Validating Array Conversions ---");
            
            try
            {
                // Test Vector3[]
                var originalVectors = new Vector3[]
                {
                    new Vector3(1, 2, 3),
                    new Vector3(4, 5, 6),
                    Vector3.zero
                };
                
                var vectorJson = Converters.ToJsonValue(originalVectors, typeof(Vector3[]));
                var restoredVectors = (Vector3[])Converters.FromJsonValue(vectorJson, typeof(Vector3[]));
                
                if (restoredVectors.Length != originalVectors.Length)
                {
                    Debug.LogError($"❌ Vector3[] length mismatch: {originalVectors.Length} -> {restoredVectors.Length}");
                    return false;
                }
                
                for (int i = 0; i < originalVectors.Length; i++)
                {
                    if (Vector3.Distance(originalVectors[i], restoredVectors[i]) > 0.001f)
                    {
                        Debug.LogError($"❌ Vector3[{i}] mismatch: {originalVectors[i]} -> {restoredVectors[i]}");
                        return false;
                    }
                }
                
                // Test Color[]
                var originalColors = new Color[]
                {
                    Color.red,
                    new Color(0.5f, 0.7f, 0.9f, 0.8f),
                    Color.clear
                };
                
                var colorJson = Converters.ToJsonValue(originalColors, typeof(Color[]));
                var restoredColors = (Color[])Converters.FromJsonValue(colorJson, typeof(Color[]));
                
                if (restoredColors.Length != originalColors.Length)
                {
                    Debug.LogError($"❌ Color[] length mismatch: {originalColors.Length} -> {restoredColors.Length}");
                    return false;
                }
                
                for (int i = 0; i < originalColors.Length; i++)
                {
                    var colorDiff = Mathf.Abs(originalColors[i].r - restoredColors[i].r) +
                                   Mathf.Abs(originalColors[i].g - restoredColors[i].g) +
                                   Mathf.Abs(originalColors[i].b - restoredColors[i].b) +
                                   Mathf.Abs(originalColors[i].a - restoredColors[i].a);
                    
                    if (colorDiff > 0.001f)
                    {
                        Debug.LogError($"❌ Color[{i}] mismatch: {originalColors[i]} -> {restoredColors[i]}");
                        return false;
                    }
                }
                
                Debug.Log("✓ Vector3[] conversion accurate");
                Debug.Log("✓ Color[] conversion accurate");
                Debug.Log("✅ Array conversions validation passed");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Array conversions test failed: {ex.Message}");
                return false;
            }
        }

        private bool ValidateNumericArrayReader()
        {
            Debug.Log("--- Validating NumericArrayReader ---");
            
            try
            {
                // Test with Unity types
                var vectorResult = NumericArrayReader.ReadFloatArray(new Vector3(1, 2, 3));
                if (vectorResult.Length != 3 || vectorResult[0] != 1f || vectorResult[1] != 2f || vectorResult[2] != 3f)
                {
                    Debug.LogError("❌ NumericArrayReader Vector3 parsing failed");
                    return false;
                }
                
                var colorResult = NumericArrayReader.ReadFloatArray(Color.red);
                if (colorResult.Length != 4 || colorResult[0] != 1f || colorResult[1] != 0f || colorResult[2] != 0f || colorResult[3] != 1f)
                {
                    Debug.LogError("❌ NumericArrayReader Color parsing failed");
                    return false;
                }
                
                // Test with arrays
                var arrayResult = NumericArrayReader.ReadFloatArray(new float[] { 4.5f, 5.5f, 6.5f });
                if (arrayResult.Length != 3 || arrayResult[0] != 4.5f || arrayResult[1] != 5.5f || arrayResult[2] != 6.5f)
                {
                    Debug.LogError("❌ NumericArrayReader float[] parsing failed");
                    return false;
                }
                
                Debug.Log("✓ NumericArrayReader Unity types parsing");
                Debug.Log("✓ NumericArrayReader array parsing");
                Debug.Log("✅ NumericArrayReader validation passed");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ NumericArrayReader test failed: {ex.Message}");
                return false;
            }
        }

        private bool ValidateConverterFacade()
        {
            Debug.Log("--- Validating Converter Facade ---");
            
            try
            {
                // Test that Converters.cs facade properly routes to registry
                var vector = new Vector3(7, 8, 9);
                
                // Test IsSupported
                if (!Converters.IsSupported(typeof(Vector3)))
                {
                    Debug.LogError("❌ Converters.IsSupported(Vector3) should return true");
                    return false;
                }
                
                if (!Converters.IsSupported(typeof(Vector3[])))
                {
                    Debug.LogError("❌ Converters.IsSupported(Vector3[]) should return true");
                    return false;
                }
                
                // Test conversion through facade
                var jsonValue = Converters.ToJsonValue(vector, typeof(Vector3));
                var restored = (Vector3)Converters.FromJsonValue(jsonValue, typeof(Vector3));
                
                if (Vector3.Distance(vector, restored) > 0.001f)
                {
                    Debug.LogError($"❌ Converter facade conversion failed: {vector} -> {restored}");
                    return false;
                }
                
                Debug.Log("✓ Converters.IsSupported() works");
                Debug.Log("✓ Converter facade routing works");
                Debug.Log("✅ Converter facade validation passed");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Converter facade test failed: {ex.Message}");
                return false;
            }
        }
    }
}