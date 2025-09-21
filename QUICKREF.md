# SaveFramework 快速参考卡

## 基本使用步骤

1. **添加 SaveId 组件** 到需要保存数据的 GameObject
2. **标记字段** 使用 `[Save]` 属性
3. **生成注册代码** - Tools → Save Framework → 生成注册
4. **保存/加载** 使用 `SaveManager.Instance.Save("slot")` 和 `SaveManager.Instance.Load("slot")`

## 代码模板

### 基础保存组件
```csharp
using UnityEngine;
using SaveFramework.Runtime.Attributes;
using SaveFramework.Runtime.Core;

public class MyData : MonoBehaviour
{
    [Save("health")] public int Health = 100;
    [Save] public Vector3 Position;
    
    public void SaveData()
    {
        SaveManager.Instance.Save("my_slot");
    }
    
    public void LoadData()
    {
        SaveManager.Instance.Load("my_slot");
    }
}
```

### 支持的类型
| 类型 | 示例 |
|------|------|
| 基础类型 | `int`, `float`, `string`, `bool` |
| Unity类型 | `Vector3`, `Quaternion`, `Color` |
| 数组 | `int[]`, `Vector3[]` |
| 集合 | `List<string>` |
| 枚举 | `enum MyEnum { A, B }` |

### 常用 Save 属性用法
```csharp
[Save] public int Value;                    // 使用字段名
[Save("custom_key")] public int Value;      // 自定义键名
[Save("key", "old_key")] public int Value;  // 支持别名
```

### 快捷键
- **F5**: 保存数据（在示例中）
- **F9**: 加载数据（在示例中）

### 编辑器菜单
- **Tools → Save Framework → 生成注册**: 扫描并生成保存代码
- **Tools → Save Framework → 清除生成的注册**: 清理生成的文件
- **Tools → Save Framework → 显示生成的文件**: 打开生成的文件位置

### 常用 API
```csharp
// 保存管理器
SaveManager.Instance.Save("slot_name");
SaveManager.Instance.Load("slot_name");
SaveManager.Instance.HasSave("slot_name");
SaveManager.Instance.DeleteSave("slot_name");
SaveManager.Instance.GetSaveSlots();

// SaveId 组件
var saveId = GetComponent<SaveId>();
saveId.GenerateNewId();
saveId.SetCustomId("custom_id");
saveId.HasValidId();
```

### 故障排除快速检查
- [ ] GameObject 有 SaveId 组件吗？
- [ ] 字段有 [Save] 属性吗？
- [ ] 执行了"生成注册"吗？
- [ ] 存档文件存在吗？检查：`Application.persistentDataPath/Saves/`

---

*这是 SaveFramework 的快速参考指南，完整文档请参阅 README.md*