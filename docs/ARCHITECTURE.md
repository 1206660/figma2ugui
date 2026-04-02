# 架构设计文档

## 项目概述

**项目**: figma2ugui  
**版本**: 1.0.0  
**日期**: 2026-04-02

## 1. 系统架构

### 1.1 整体架构

```
┌─────────────────┐
│  Figma Design   │
│     Files       │
└────────┬────────┘
         │
         │ Figma REST API
         ▼
┌─────────────────┐
│  Unity Package  │
│  (C# Editor)    │
├─────────────────┤
│ - API Client    │
│ - Parser        │
│ - Converter     │
│ - Asset Manager │
└────────┬────────┘
         │
         │ Unity API
         ▼
┌─────────────────┐
│  Unity Scene    │
│  (UGUI Prefab)  │
└─────────────────┘
```

### 1.2 核心组件

#### 组件 1: FigmaApiClient
**职责**: 与 Figma REST API 通信
- 认证管理（Personal Access Token）
- 获取文件数据
- 导出图片资源
- 错误处理和重试

#### 组件 2: FigmaParser
**职责**: 解析 Figma 数据结构
- 解析节点树
- 提取样式信息
- 识别组件类型
- 构建中间数据模型

#### 组件 3: UguiConverter
**职责**: 转换为 Unity UGUI
- 节点到 GameObject 映射
- 组件创建和配置
- 布局计算（RectTransform）
- 层级结构构建

#### 组件 4: AssetManager
**职责**: 资源管理
- 下载和缓存图片
- 创建 Sprite
- 字体导入
- 资源去重

#### 组件 5: EditorWindow
**职责**: Unity 编辑器界面
- 用户输入（Token, File URL）
- 导入进度显示
- 配置选项
- 预览功能

## 2. 数据流

### 2.1 导入流程

```
1. 用户输入 → EditorWindow
2. EditorWindow → FigmaApiClient (获取文件)
3. FigmaApiClient → FigmaParser (解析数据)
4. FigmaParser → UguiConverter (转换)
5. UguiConverter → AssetManager (处理资源)
6. UguiConverter → Unity Scene (创建对象)
7. 完成 → 保存 Prefab
```

### 2.2 数据模型

#### Figma 数据模型
```csharp
class FigmaNode {
    string id;
    string name;
    string type;  // FRAME, TEXT, RECTANGLE, etc.
    FigmaNode[] children;
    Dictionary<string, object> style;
    LayoutProperties layout;
}
```

#### 中间数据模型
```csharp
class UguiNode {
    string name;
    UguiComponentType componentType;
    RectTransformData rectTransform;
    ComponentData componentData;
    UguiNode[] children;
}
```

## 3. 技术选型

### 3.1 开发技术栈

| 层级 | 技术 | 说明 |
|-----|------|------|
| Unity 版本 | 2021.3 LTS+ | 长期支持版本 |
| 开发语言 | C# 9.0+ | Unity 脚本 |
| UI 框架 | UGUI | Unity 内置 |
| 文本组件 | TextMeshPro | Unity Package |
| HTTP 客户端 | UnityWebRequest | Unity 内置 |
| JSON 解析 | Unity JsonUtility / Newtonsoft.Json | 数据解析 |

### 3.2 外部依赖

- **Figma REST API v1**: 数据源
- **TextMeshPro**: 文本渲染（Unity 内置包）

## 4. 目录结构

```
figma2ugui/
├── Editor/
│   ├── Core/
│   │   ├── FigmaApiClient.cs
│   │   ├── FigmaParser.cs
│   │   ├── UguiConverter.cs
│   │   └── AssetManager.cs
│   ├── Models/
│   │   ├── FigmaModels.cs
│   │   └── UguiModels.cs
│   ├── UI/
│   │   └── FigmaImporterWindow.cs
│   └── Utils/
│       ├── LayoutCalculator.cs
│       └── ColorConverter.cs
├── Runtime/
│   └── (运行时组件，如需要)
├── Tests/
│   ├── Editor/
│   └── Runtime/
├── Documentation~/
├── package.json
└── README.md
```

## 5. 关键设计决策

### 5.1 为什么不使用 Figma Plugin？

**决策**: 仅使用 REST API，不开发 Figma Plugin

**原因**:
- 降低复杂度，减少维护成本
- REST API 功能已足够
- 避免 Plugin 审核和分发问题
- 用户无需安装额外工具

### 5.2 同步 vs 异步

**决策**: 使用异步操作

**原因**:
- 网络请求耗时
- 避免阻塞 Unity 编辑器
- 提供进度反馈
- 支持取消操作

### 5.3 资源缓存策略

**决策**: 本地文件系统缓存

**位置**: `Assets/FigmaCache/`
**策略**: 
- 按文件 ID 和版本号缓存
- 支持手动清理
- 自动过期（可配置）

## 6. 扩展性设计

### 6.1 组件映射扩展

使用策略模式支持自定义组件转换：

```csharp
interface IComponentConverter {
    bool CanConvert(FigmaNode node);
    void Convert(FigmaNode node, GameObject target);
}
```

### 6.2 插件系统

预留接口供第三方扩展：
- 自定义组件转换器
- 后处理钩子
- 资源处理器

## 7. 性能考虑

### 7.1 优化策略

- **批量处理**: 资源下载使用并发请求
- **增量更新**: 仅更新变化的节点
- **延迟加载**: 大文件分批处理
- **对象池**: 复用临时对象

### 7.2 性能目标

- 100 个节点 < 10 秒
- 500 个节点 < 30 秒
- 内存占用 < 500MB

## 参考资料

- [Figma REST API](https://www.figma.com/developers/api)
- [Unity UGUI Documentation](https://docs.unity3d.com/Manual/UISystem.html)
- [cdmvision/unity-figma-importer](https://github.com/cdmvision/unity-figma-importer)
