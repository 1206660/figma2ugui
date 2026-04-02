# figma2ugui

> 将 Figma 设计自动转换为 Unity UGUI 的开源工具

[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](LICENSE)
[![Unity](https://img.shields.io/badge/unity-2021.3%2B-green.svg)](https://unity.com/)

## 简介

figma2ugui 是一个开源工具，可以将 Figma 设计文件自动转换为 Unity UGUI 组件，大幅减少 UI 开发时间。

### 主要特性

- 🚀 一键导入 Figma 设计到 Unity
- 🎨 保持设计稿的像素级精确度
- 📦 自动处理图片资源和字体
- 🔄 支持 Auto Layout 转换为 Layout Groups
- 🎯 生成可直接使用的 Prefab

## 快速开始

### 安装

通过 Unity Package Manager 安装：

1. 打开 Unity 编辑器
2. 打开 Package Manager (Window > Package Manager)
3. 点击 "+" 按钮，选择 "Add package from git URL"
4. 输入：`https://github.com/yourusername/figma2ugui.git`

### 使用方法

1. 在 Figma 中获取 Personal Access Token
   - 访问 Figma Settings > Account > Personal Access Tokens
   - 创建新 Token

2. 在 Unity 中打开导入窗口
   - Tools > Figma Importer

3. 输入信息并导入
   - 粘贴 Access Token
   - 输入 Figma 文件 URL
   - 点击 Import

## 支持的组件

| Figma | Unity UGUI |
|-------|-----------|
| Frame | Panel (RectTransform) |
| Text | TextMeshProUGUI |
| Rectangle | Image |
| Image | Image (Sprite) |
| Auto Layout (H) | HorizontalLayoutGroup |
| Auto Layout (V) | VerticalLayoutGroup |

## 文档

- [产品需求文档](docs/PRD.md)
- [架构设计](docs/ARCHITECTURE.md)
- [技术实现](docs/TECHNICAL.md)
- [开发任务](docs/TASKS.md)

## 开发状态

🚧 项目正在开发中，预计 v1.0 将在 8 周内发布。

## 贡献

欢迎贡献！请查看 [CONTRIBUTING.md](CONTRIBUTING.md) 了解详情。

## 许可证

本项目采用 Apache 2.0 许可证 - 详见 [LICENSE](LICENSE) 文件。

## 参考资料

- [Figma REST API](https://www.figma.com/developers/api)
- [Unity UGUI Documentation](https://docs.unity3d.com/Manual/UISystem.html)
- [cdmvision/unity-figma-importer](https://github.com/cdmvision/unity-figma-importer)
