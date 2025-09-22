using System;
using System.Collections.Generic;

namespace SaveFramework.Runtime.Core
{
    /// <summary>
    /// 保存数据存储后端的接口
    /// </summary>
    public interface ISaveBackend
    {
        /// <summary>
        /// 将数据保存到指定插槽
        /// </summary>
        void Save(string slotName, Dictionary<string, object> data);

        /// <summary>
        /// 从指定插槽加载数据
        /// </summary>
        Dictionary<string, object> Load(string slotName);

        /// <summary>
        /// 检查是否存在保存槽
        /// </summary>
        bool HasSave(string slotName);

        /// <summary>
        /// 删除保存槽
        /// </summary>
        void DeleteSave(string slotName);

        /// <summary>
        /// 获取所有可用的保存槽名称
        /// </summary>
        string[] GetSaveSlots();
    }
}