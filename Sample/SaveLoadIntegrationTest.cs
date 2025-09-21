using UnityEngine;
using SaveFramework.Runtime.Core;
using SaveFramework.Components;
using SaveFramework.Runtime.Core.Attributes;

namespace SaveFramework.Test
{
    /// <summary>
    /// Integration test to verify the complete save/load workflow with the new converter system
    /// </summary>
    public class SaveLoadIntegrationTest : MonoBehaviour
    {
        [SaveFramework.Runtime.Core.Attributes.SaveField(" Test Vector3 Array ")] 
        public Vector3[] testVector3Array = new Vector3[] 
        { 
            new Vector3(1.1f, 2.2f, 3.3f), 
            new Vector3(4.4f, 5.5f, 6.6f),
            Vector3.zero,
            Vector3.one
        };
        
        [SaveFramework.Runtime.Core.Attributes.SaveField(" Test Color Array ")] 
        public Color[] testColorArray = new Color[] 
        { 
            Color.red, 
            Color.green, 
            new Color(0.5f, 0.7f, 0.9f, 0.8f),
            Color.clear
        };
        
        [SaveFramework.Runtime.Core.Attributes.SaveField(" Test Quaternion Array ")] 
        public Quaternion[] testQuaternionArray = new Quaternion[] 
        { 
            Quaternion.identity,
            new Quaternion(0.1f, 0.2f, 0.3f, 0.4f),
            Quaternion.Euler(45, 90, 180)
        };
        
        [SaveFramework.Runtime.Core.Attributes.SaveField(" Test Single Vector3 ")] 
        public Vector3 testVector3 = new Vector3(7.7f, 8.8f, 9.9f);
        
        [SaveFramework.Runtime.Core.Attributes.SaveField(" Test Single Color ")] 
        public Color testColor = new Color(0.25f, 0.5f, 0.75f, 1.0f);

        private void Start()
        {
            // Ensure this GameObject has a SaveId component
            if (GetComponent<SaveId>() == null)
            {
                var saveId = gameObject.AddComponent<SaveId>();
                saveId.Id = "SaveLoadIntegrationTest";
            }
        }

        [ContextMenu("Save Test Data")]
        public void SaveTestData()
        {
            try
            {
                Debug.Log("=== Saving Test Data ===");
                LogCurrentValues("Before Save");
                
                SaveManager.Instance.Save("test-slot");
                Debug.Log("✓ Save completed successfully");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"✗ Save failed: {ex.Message}");
            }
        }

        [ContextMenu("Load Test Data")]
        public void LoadTestData()
        {
            try
            {
                Debug.Log("=== Loading Test Data ===");
                LogCurrentValues("Before Load");
                
                SaveManager.Instance.Load("test-slot");
                Debug.Log("✓ Load completed successfully");
                
                LogCurrentValues("After Load");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"✗ Load failed: {ex.Message}");
            }
        }

        [ContextMenu("Modify and Test Save/Load")]
        public void ModifyAndTestSaveLoad()
        {
            Debug.Log("=== Modify and Test Save/Load ===");
            
            // Store original values for comparison
            var originalVector3Array = (Vector3[])testVector3Array.Clone();
            var originalColorArray = (Color[])testColorArray.Clone();
            var originalQuaternionArray = (Quaternion[])testQuaternionArray.Clone();
            var originalVector3 = testVector3;
            var originalColor = testColor;
            
            // Modify values
            testVector3Array[0] = new Vector3(10.1f, 20.2f, 30.3f);
            testColorArray[0] = Color.magenta;
            testQuaternionArray[0] = Quaternion.Euler(15, 30, 45);
            testVector3 = new Vector3(100f, 200f, 300f);
            testColor = Color.cyan;
            
            LogCurrentValues("After Modification");
            
            // Save modified values
            SaveTestData();
            
            // Restore original values
            testVector3Array = originalVector3Array;
            testColorArray = originalColorArray;
            testQuaternionArray = originalQuaternionArray;
            testVector3 = originalVector3;
            testColor = originalColor;
            
            LogCurrentValues("After Restoration (should be original values)");
            
            // Load saved values
            LoadTestData();
            
            LogCurrentValues("After Load (should be modified values)");
        }

        private void LogCurrentValues(string phase)
        {
            Debug.Log($"--- Current Values ({phase}) ---");
            
            Debug.Log($"Vector3 Array: [");
            for (int i = 0; i < testVector3Array.Length; i++)
            {
                Debug.Log($"  [{i}] {testVector3Array[i]}");
            }
            Debug.Log("]");
            
            Debug.Log($"Color Array: [");
            for (int i = 0; i < testColorArray.Length; i++)
            {
                Debug.Log($"  [{i}] {testColorArray[i]}");
            }
            Debug.Log("]");
            
            Debug.Log($"Quaternion Array: [");
            for (int i = 0; i < testQuaternionArray.Length; i++)
            {
                Debug.Log($"  [{i}] {testQuaternionArray[i]}");
            }
            Debug.Log("]");
            
            Debug.Log($"Single Vector3: {testVector3}");
            Debug.Log($"Single Color: {testColor}");
        }

        [ContextMenu("Test Converter Registry")]
        public void TestConverterRegistry()
        {
            var test = GetComponent<ConverterRegistryTest>();
            if (test == null)
            {
                test = gameObject.AddComponent<ConverterRegistryTest>();
            }
            test.TestConverterRegistry();
        }
    }
}