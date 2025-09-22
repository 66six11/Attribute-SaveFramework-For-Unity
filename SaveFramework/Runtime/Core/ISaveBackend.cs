using System;
using System.Collections.Generic;

namespace SaveFramework.Runtime.Core
{
    /// <summary>
    /// Interface for save data storage backends
    /// </summary>
    public interface ISaveBackend
    {
        /// <summary>
        /// Save data to the specified slot
        /// </summary>
        void Save(string slotName, Dictionary<string, object> data);

        /// <summary>
        /// Load data from the specified slot
        /// </summary>
        Dictionary<string, object> Load(string slotName);

        /// <summary>
        /// Check if a save slot exists
        /// </summary>
        bool HasSave(string slotName);

        /// <summary>
        /// Delete a save slot
        /// </summary>
        void DeleteSave(string slotName);

        /// <summary>
        /// Get all available save slot names
        /// </summary>
        string[] GetSaveSlots();
    }
}