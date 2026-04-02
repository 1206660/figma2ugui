# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**figma2ugui** is an open-source tool for converting Figma designs to Unity UI (UGUI). The project automates importing Figma layouts, components, and assets into Unity as native UGUI elements.

## Project Structure

```
figma2ugui/
├── Editor/                 # Unity Editor scripts
│   ├── Core/              # Core functionality
│   ├── Models/            # Data models
│   ├── UI/                # Editor windows
│   └── Utils/             # Utilities
├── Runtime/               # Runtime components (if needed)
├── Tests/                 # Unit and integration tests
├── docs/                  # Documentation
│   ├── PRD.md            # Product requirements
│   ├── ARCHITECTURE.md   # System architecture
│   ├── TECHNICAL.md      # Technical implementation
│   └── TASKS.md          # Development tasks
├── package.json          # Unity package manifest
└── README.md             # Project readme
```

## Development Commands

### Unity Package Development
- Open Unity 2021.3 LTS or later
- Add package via Package Manager: `Window > Package Manager > + > Add package from disk`
- Select `package.json` from this repository

### Testing
- Run tests via Unity Test Runner: `Window > General > Test Runner`
- Editor tests are in `Tests/Editor/`

## Core Architecture

### Main Components

1. **FigmaApiClient**: Communicates with Figma REST API
2. **FigmaParser**: Parses Figma node tree and extracts data
3. **UguiConverter**: Converts Figma nodes to Unity GameObjects
4. **AssetManager**: Downloads and manages image/font assets
5. **FigmaImporterWindow**: Unity Editor UI for import workflow

### Data Flow

```
Figma API → FigmaApiClient → FigmaParser → UguiConverter → Unity Scene
                                    ↓
                              AssetManager → Assets/FigmaCache/
```

## Key Technical Details

- **Unity Version**: 2021.3 LTS+
- **Language**: C# 9.0+
- **Dependencies**: TextMeshPro (Unity built-in)
- **API**: Figma REST API v1
- **Authentication**: Personal Access Token

## Component Mapping

| Figma | Unity UGUI |
|-------|-----------|
| Frame | RectTransform + Panel |
| Text | TextMeshProUGUI |
| Rectangle | Image (Solid) |
| Image | Image (Sprite) |
| Auto Layout (H) | HorizontalLayoutGroup |
| Auto Layout (V) | VerticalLayoutGroup |

## Development Guidelines

- Follow Unity C# coding conventions
- Keep classes focused and under 300 lines
- Use async/await for API calls
- Cache downloaded assets in `Assets/FigmaCache/`
- Provide progress feedback for long operations
- Handle errors gracefully with user-friendly messages

## Documentation

See `docs/` folder for detailed documentation:
- **PRD.md**: Product requirements and goals
- **ARCHITECTURE.md**: System design and components
- **TECHNICAL.md**: Implementation details and code examples
- **TASKS.md**: Development roadmap and task breakdown
