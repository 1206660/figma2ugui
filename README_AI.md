# figma2ugui - AI 驱动的 Figma 到 Unity 转换工具

## 核心特性

🤖 **AI 智能规则生成** - Claude 分析设计自动生成最优导入规则  
🔄 **MCP 实时连接** - 通过 Model Context Protocol 直接读取 Figma  
⚡ **自动化工作流** - 从设计到 Prefab 全自动  
🎯 **智能组件识别** - 基于命名和结构自动映射组件类型  

## 架构

```
Figma (MCP Server) → Claude AI → 规则引擎 → Unity UGUI
```

## 快速开始

### 1. 配置 MCP
```bash
# 在 .kiro/settings/mcp.json 中配置 Figma MCP Server
```

### 2. 使用 AI 生成规则
Claude 会分析你的 Figma 设计，自动生成：
- 命名规范映射（如 `btn_*` → Button）
- 布局约束转换
- 资源优化策略

### 3. 一键导入
```
Tools > Figma Importer > Import with AI Rules
```

## 设计规范

为获得最佳效果，请在 Figma 中：
- ✅ 使用 Auto Layout
- ✅ 遵循命名规范（btn_, txt_, img_）
- ✅ 组件化设计

## 参考资料

- [MCP Protocol](https://github.com/anthropics/mcp)
- [Figma REST API](https://www.figma.com/developers/api)
