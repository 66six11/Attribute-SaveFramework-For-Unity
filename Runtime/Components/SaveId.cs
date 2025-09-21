using System;
using UnityEngine;

namespace SaveFramework.Components
{
    /// <summary>
    /// Component that provides a unique identifier for save/load operations
    /// </summary>
    public class SaveId : MonoBehaviour
    {
        [SerializeField, Tooltip("此对象保存数据的唯一标识符")]
        private string id = string.Empty;

        /// <summary>
        /// The unique identifier for this object
        /// </summary>
        public string Id 
        { 
            get 
            { 
                if (string.IsNullOrEmpty(id))
                {
                    GenerateNewId();
                }
                return id; 
            } 
            set 
            { 
                id = value; 
            } 
        }

        private void Awake()
        {
            // Ensure we have a valid ID
            if (string.IsNullOrEmpty(id))
            {
                GenerateNewId();
            }
        }

        /// <summary>
        /// Generate a new unique identifier
        /// </summary>
        [ContextMenu("生成新 ID")]
        public void GenerateNewId()
        {
            id = Guid.NewGuid().ToString();
            
#if UNITY_EDITOR
            // Mark the object as dirty in the editor so the change gets saved
            if (Application.isPlaying == false)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }

        /// <summary>
        /// Set a custom ID (useful for predictable IDs in some cases)
        /// </summary>
        public void SetCustomId(string customId)
        {
            if (string.IsNullOrEmpty(customId))
                throw new ArgumentException("Custom ID cannot be null or empty", nameof(customId));

            id = customId;

#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }

        /// <summary>
        /// Check if this SaveId has a valid identifier
        /// </summary>
        public bool HasValidId()
        {
            return !string.IsNullOrEmpty(id);
        }

        private void OnValidate()
        {
            // Auto-generate ID if it's missing when the component is added or modified
            if (string.IsNullOrEmpty(id))
            {
                GenerateNewId();
            }
        }

        /// <summary>
        /// Get a display name for this SaveId (useful for debugging)
        /// </summary>
        public string GetDisplayName()
        {
            var objectName = gameObject.name;
            var shortId = string.IsNullOrEmpty(id) ? "NO_ID" : id.Substring(0, Math.Min(8, id.Length));
            return $"{objectName} ({shortId}...)";
        }
    }
}