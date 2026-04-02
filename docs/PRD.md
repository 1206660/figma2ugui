# 产品需求文档 (PRD)

## 项目概述

**项目名称**: figma2ugui  
**版本**: 1.0.0  
**日期**: 2026-04-02  
**状态**: 规划阶段

## 1. 产品目标

创建一个开源工具，自动将 Figma 设计转换为 Unity UGUI，减少 UI 开发时间，提高设计到实现的一致性。

### 核心价值主张
- **提高效率**: 将手动 UI 重建时间从数小时减少到数分钟
- **保持一致性**: 确保设计稿与实现的像素级一致
- **降低门槛**: 让非程序员也能参与 UI 实现
- **开源免费**: 相比商业工具，零成本使用

## 2. 目标用户

### 主要用户
1. **Unity UI 开发者**: 需要快速实现设计稿的程序员
2. **独立游戏开发者**: 资源有限，需要高效工具
3. **UI/UX 设计师**: 希望直接参与实现的设计师

### 用户痛点
- 手动重建 UI 耗时且容易出错
- 设计与实现不一致，需要反复调整
- 商业工具价格昂贵
- 现有开源工具功能不完整或维护不足

## 3. 核心功能

### MVP (最小可行产品)

#### 3.1 Figma 数据读取
- 通过 Figma REST API 读取设计文件
- 支持个人访问令牌认证
- 解析节点树结构
- 提取样式信息（颜色、字体、尺寸等）

#### 3.2 组件映射
| Figma 组件 | Unity UGUI 组件 |
|-----------|----------------|
| Frame | RectTransform + Canvas/Panel |
| Text | TextMeshProUGUI |
| Rectangle | Image (Solid Color) |
| Image | Image (Sprite) |
| Auto Layout (Horizontal) | HorizontalLayoutGroup |
| Auto Layout (Vertical) | VerticalLayoutGroup |

#### 3.3 资源管理
- 导出并下载图片资源（PNG/SVG）
- 字体文件下载和导入
- 资源去重和缓存
- 自动生成 Sprite 和 Material

#### 3.4 Unity 集成
- Unity Package Manager 安装
- 编辑器窗口界面
- 一键导入功能
- 生成预制体（Prefab）

## 4. 功能优先级

### P0 (必须有)
- Figma API 集成
- 基础组件映射（Frame, Text, Image）
- 资源导出
- Unity 编辑器窗口

### P1 (应该有)
- Auto Layout 支持
- 锚点和适配
- 命名规范保持
- 错误处理和日志

### P2 (可以有)
- 增量更新
- 组件绑定
- 本地化支持
- 动画转换

## 5. 技术约束

- Unity 版本: 2021.3 LTS 及以上
- Figma API 版本: v1
- 依赖: TextMeshPro (Unity 内置)
- 开发语言: C# (Unity), TypeScript (可选 Figma Plugin)

## 6. 成功指标

- 基础 UI 转换准确率 > 90%
- 单个页面导入时间 < 30 秒
- GitHub Stars > 100 (6个月内)
- 社区贡献者 > 5 人

## 7. 风险与挑战

| 风险 | 影响 | 缓解策略 |
|-----|------|---------|
| Figma API 变更 | 高 | 版本锁定，及时更新 |
| 复杂布局转换不准确 | 中 | 提供手动调整选项 |
| 性能问题（大文件） | 中 | 分批处理，异步加载 |
| 用户学习成本 | 低 | 详细文档和示例 |

## 8. 发布计划

- **Alpha (v0.1)**: 基础功能，内部测试
- **Beta (v0.5)**: 核心功能完整，公开测试
- **v1.0**: 稳定版本，完整文档

## 参考资料

- [Unity Figma Importer (cdmvision)](https://github.com/cdmvision/unity-figma-importer)
- [Figma REST API Documentation](https://www.figma.com/developers/api)
