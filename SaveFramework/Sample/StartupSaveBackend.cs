using System.IO;
using UnityEngine;
using SaveFramework.Runtime.Core;
using UnityEditor;

public class StartupSaveBackend : MonoBehaviour
{
    [Tooltip("留空则使用默认 persistentDataPath/Saves")]
    [SerializeField] private string customDir = "";

    private void Awake()
    {
        // 如果没填就回落到默认位置
        var dir = string.IsNullOrEmpty(customDir)
            ? Path.Combine(Application.persistentDataPath, "Saves")
            : customDir;

        // 设置自定义后端（构造函数内部会自动创建目录）
        SaveManager.Instance.SetBackend(new FileBackend(dir));
    }
}