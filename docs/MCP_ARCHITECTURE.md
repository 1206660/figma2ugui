# MCP 集成架构设计

## 核心概念

基于 MCP (Model Context Protocol)，让 Claude AI 实时读取 Figma 设计，智能生成导入规则。

## 架构升级

```
Figma Design
    ↓ (MCP Server)
Claude AI ← 读取设计上下文
    ↓ (生成规则)
规则引擎 → Unity Importer
    ↓
UGUI Prefab
```

## 关键组件

### 1. MCP Bridge
- 连接 Figma 和 Claude
- 实时读取设计结构
- 理解 Auto Layout 逻辑

### 2. AI 规则生成器
Claude 分析设计后自动生成：
- 命名规范映射
- 组件类型识别
- 布局约束转换
- 资源优化策略

### 3. 规则引擎
根据 AI 生成的规则执行：
- 智能组件映射
- 自动布局计算
- 资源去重优化

## 工作流

1. **设计阶段**：设计师在 Figma 使用 Auto Layout
2. **AI 分析**：Claude 通过 MCP 读取设计，理解结构
3. **规则生成**：AI 生成最优导入规则
4. **自动导入**：Unity 按规则生成 UGUI
5. **双向同步**：修改可回流到 Figma

## 参考资料
- [MCP Protocol](https://github.com/anthropics/mcp)
