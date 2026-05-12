# Origami Simulator

Unity 折纸模拟与折痕图编辑项目。项目目标是把二维折痕图转换为可加载、可编辑、可模拟的三维折纸结构，并通过 Unity 的物理系统和 UI 控件展示折叠过程。

## 项目状态

当前版本已经具备一条基本工作流：

1. 在折痕图编辑模式下绘制顶点和折痕。
2. 将折痕图保存为 JSON 模型文件。
3. 在模拟模式下读取 JSON。
4. 生成面片、折痕线和相邻面之间的 `HingeJoint`。
5. 使用滑条、键盘和相机控制查看整体折叠效果。

目前折叠控制仍以“整体统一进度”为主，还没有完成单条折痕独立角度控制。物理模拟使用多个刚体和铰链，复杂模型下可能出现穿模或碰撞不稳定。

## 环境

- Unity：`2022.3.62f2c1`
- 主要包：
  - TextMesh Pro `3.0.9`
  - Unity UI `1.0.0`
  - Visual Scripting `1.9.4`
  - Timeline `1.7.7`

## 目录结构

```text
Assets/
  Models/                 折纸 JSON、OBJ 等模型数据
  Scenes/                 Unity 场景
  Materials/              面片、折痕、顶点和网格材质
  Shader/                 自定义网格和双面渲染 Shader
  Font/                   字体与字体资源
  TextMesh Pro/           TextMesh Pro 资源
  scripts/                核心运行时和编辑器脚本
Packages/                 Unity 包依赖
ProjectSettings/          Unity 项目配置
```

Git 仓库中不应提交 `Library/`、`Logs/`、`obj/`、`.vs/`、`UserSettings/`、`*.csproj`、`*.sln` 等本地生成文件；这些已经由 `.gitignore` 排除。

## 主要功能

- 折痕图编辑
  - 添加顶点
  - 添加折痕
  - 移动顶点
  - 删除顶点
  - 删除折痕
  - 编辑折痕属性
  - 保存和加载 JSON 折纸模型

- 折痕属性
  - 支持山折、谷折、边界、平折
  - 支持最小角度和最大角度输入
  - 根据折痕类型切换线条材质和显示宽度

- 折纸模拟
  - 从 `Assets/Models` 加载 JSON 模型
  - 自动生成 Unity `Mesh`、`MeshRenderer`、`Rigidbody`、`MeshCollider`
  - 根据相邻面创建 `HingeJoint`
  - 使用整体折叠进度控制所有有效铰链
  - 支持显示或隐藏折痕线

- 场景交互
  - WASD 平移相机
  - Q/E 上下移动
  - 鼠标右键旋转视角
  - 鼠标中键平移视角
  - 鼠标滚轮缩放
  - Home 或按钮重置相机
  - F1 切换到折痕图设计模式
  - F2 切换到折纸模拟模式

## 核心脚本

### 运行时模拟

- `Assets/scripts/OrigamiData.cs`
  - 定义 JSON 序列化和反序列化使用的数据结构，包括顶点、面、折痕、连接关系、材质和完整模型。

- `Assets/scripts/OrigamiLoader.cs`
  - 读取 JSON 模型，生成面片、折痕线、刚体、碰撞体和铰链，并把有效铰链注册到控制器。

- `Assets/scripts/OrigamiController.cs`
  - 管理场景中的 `HingeJoint`，通过 `foldProgress` 统一控制折叠进度，并支持键盘控制整体折叠和旋转。

- `Assets/scripts/SliderController.cs`
  - 在 UI 滑条和 `OrigamiController.foldProgress` 之间做换算。

- `Assets/scripts/CameraController.cs`
  - 负责相机移动、旋转、缩放、平移和重置。

- `Assets/scripts/DesignModeManager.cs`
  - 管理“折痕图设计模式”和“折纸模拟模式”的切换，同时控制对应 UI 和场景对象的显示。

- `Assets/scripts/GridCameraLink.cs`
  - 将相机矩阵传入网格材质，用于驱动场景网格显示。

- `Assets/scripts/FaceFinder.cs`
  - 基于半边结构查找折痕所在面，可用于后续选面、面属性编辑和局部折叠功能。

- `Assets/scripts/Triangulator.cs`
  - 使用 Ear Clipping 思路将多边形拆分为三角面，供 Mesh 生成使用。

### 折痕图编辑器

- `Assets/scripts/CreasePatternEditor/CreasePatternEditor.cs`
  - 折痕图编辑器主脚本，负责顶点、折痕、编辑模式、删除操作、属性编辑、保存和加载。

- `Assets/scripts/CreasePatternEditor/CreaseAttributePanel.cs`
  - 折痕属性面板 UI，负责展示和提交折痕类型、最小角度、最大角度。

- `Assets/scripts/CreasePatternEditor/CreaseAttributeEditor.cs`
  - 折痕属性修改服务层，封装折痕类型和角度更新逻辑。

- `Assets/scripts/CreasePatternEditor/GeometryUtils.cs`
  - 几何工具类，提供点线距离、点在多边形内、线段相交、转角和简单回路判断。

- `Assets/scripts/CreasePatternEditor/HalfEdgeMesh.cs`
  - 半边网格结构封装，用于从顶点和折痕重建拓扑关系并提取闭合面。

- `Assets/scripts/CreasePatternEditor/OrigamiFaceGenerator.cs`
  - 自动面生成器，在保存 JSON 时根据顶点和折痕补全面数据。

## 使用方式

1. 使用 Unity Hub 打开项目根目录。
2. 确认 Unity 版本为 `2022.3.62f2c1` 或兼容的 2022.3 LTS 版本。
3. 打开 `Assets/Scenes/SampleScene.unity`。
4. 进入 Play 模式。
5. 使用 F1/F2 或界面按钮在设计模式和模拟模式之间切换。
6. 在设计模式中绘制折痕图并保存为 JSON。
7. 在模拟模式中加载 JSON，使用滑条或方向键查看折叠效果。

## 模型数据

示例模型位于：

```text
Assets/Models/
```

当前包含：

- `CustomPattern.json`
- `Miura0.json`
- `miura_J.json`
- `miura.obj`
- `miura - 副本.txt`

JSON 模型通常包含：

- `vertices`：顶点坐标
- `faces`：面片顶点列表
- `creases`：折痕连接、类型、角度范围和宽度
- `connections`：折痕与相邻面的连接关系
- `material`：材质参数

## 当前限制

- `OrigamiController` 仍然使用所有折痕统一折叠进度，还不是单折痕独立控制。
- `SliderController` 默认所有铰链角度范围一致，并使用第一个铰链作为参考。
- 复杂凹多边形或异常折痕图下，面生成和三角化仍可能不稳定。
- `SaveToJson()` 对折痕角度范围的保存逻辑仍需要继续校准。
- 多刚体加 `HingeJoint` 的物理方案容易受到碰撞、质量、约束参数影响，复杂模型下可能穿模。

## 后续开发方向

1. 支持单条折痕独立角度控制。
2. 优化自动面生成和复杂多边形三角化。
3. 改进物理稳定性，减少穿模和抖动。
4. 完善 JSON 模型格式校验和错误提示。
5. 增加更清晰的编辑器操作提示和示例模型。

## GitHub 使用建议

日常修改后可以按下面流程提交：

```powershell
git status
git add .
git commit -m "Update origami simulator"
git push
```

如果是第一次推送到远端仓库：

```powershell
git remote add origin https://github.com/Glenn0115/My-project-origami.git
git push -u origin main
```
