# AI 组件分析功能实现

## 已实现功能

### 1. 自动组件识别
- ✅ ScrollView 自动识别（名称包含 scroll/list）
- ✅ Button 自动识别（名称以 btn 开头或包含 button）
- ✅ 基础组件（Text, Image, Panel）

### 2. 字体分析
- ✅ 自动提取字体族（fontFamily）
- ✅ 自动提取字体大小（fontSize）
- ✅ 自动提取字重（fontWeight）

### 3. ScrollView 生成
自动创建完整的 ScrollView 结构：
```
ScrollView
├── Viewport (Mask)
└── Content (动态内容容器)
```

## 新增文件

```
Editor/AI/
├── ComponentAnalyzer.cs  # 组件类型分析
└── FontAnalyzer.cs       # 字体信息分析
```

## 使用示例

Figma 设计中：
- 命名为 `scroll_list` → 自动生成 ScrollView
- 命名为 `btn_submit` → 自动生成 Button
- Text 节点 → 自动提取字体信息

## 下一步扩展

- [ ] 支持更多组件（Slider, Toggle, Dropdown）
- [ ] 字体资源自动下载
- [ ] 布局组件自动识别
