CreasePatternEditor文件夹，折痕图编辑器
	CreaseAttributeEditor
	CreaseAttributePanel 属性编辑面板
	CreasePatternEditor
	GeometryUtils
	HalfEdgeMesh
	OrigamiFaceGenerator

暂时只有全部等比例移动，且存在穿模现象，准备单独设置角度的展示方式，操作方式

---

# scripts文件夹功能总结（不含 `Abandoned Script`）

## 一、整体结构

`Assets/scripts` 目前可以分成两块：

1. 折纸模拟运行时
   `OrigamiData`、`OrigamiLoader`、`OrigamiController`、`SliderController`、`CameraController`、`DesignModeManager`、`GridCameraLink`、`FaceFinder`、`Triangulator`

2. 折痕图编辑器
   `CreasePatternEditor` 文件夹下的编辑、属性面板、几何工具、半边结构、自动生成功能

整体流程大致是：

1. 在折痕编辑模式下创建顶点和折痕
2. 保存为 JSON 折纸模型
3. `OrigamiLoader` 读取 JSON，生成面片、折痕线和铰链
4. `OrigamiController` 统一控制折叠进度与旋转
5. `SliderController`、UI、相机脚本提供交互

## 二、主流程脚本

### 1. `OrigamiData.cs`

数据定义文件，负责提供 JSON 序列化/反序列化使用的数据结构：

- `OrigamiVertex`：顶点坐标
- `OrigamiFace`：面片顶点列表、刚体属性、颜色
- `OrigamiCrease`：折痕两端点、类型、静止角、最小/最大角度、刚度、显示宽度
- `OrigamiConnection`：预留面与折痕连接信息
- `OrigamiMaterial`：材质参数
- `OrigamiModel`：完整模型容器

它本身不做逻辑，主要是全工程的数据基础。

### 2. `OrigamiLoader.cs`

运行时模型加载核心：

- 读取 `Assets/Models` 下的 JSON 文件
- 解析为 `OrigamiModel`
- 生成每个面对应的 `GameObject`、`Mesh`、`MeshRenderer`、`Rigidbody`、`MeshCollider`
- 根据折痕类型绘制折痕线（山折/谷折/边界使用不同材质颜色）
- 根据相邻两个面创建 `HingeJoint`
- 将生成的铰链注册到 `OrigamiController`
- 同步 UI 上的模型名称、描述、折叠滑条
- 支持重新加载模型、按名称加载模型、显示/隐藏折痕

这是“从 JSON 到可模拟折纸对象”的关键桥梁。

### 3. `OrigamiController.cs`

折纸模拟控制核心：

- 启动时搜集场景中的 `HingeJoint`
- 维护当前有效铰链列表，并持续清理失效引用
- 用 `foldProgress` 表示整体折叠进度
- 支持按上下方向键调节折叠角
- 支持按 A/D 旋转整个折纸系统
- 按折叠进度统一设置所有铰链目标角度
- 根据当前铰链角度反算整体折叠进度

当前实现特点是“所有折痕联动”，也就是整体按统一进度折叠，不是单条折痕独立控制。

### 4. `SliderController.cs`

滑条与折叠进度之间的转换层：

- 将 UI `Slider` 的值换算为 `OrigamiController.foldProgress`
- 将当前折叠进度反推回滑条位置
- 目前默认假设所有铰链角度范围一致，并取第一个铰链作为计算参考

适合当前统一折叠模式，但如果后续支持每条折痕独立角度，这里也需要一起改。

### 5. `CameraController.cs`

场景相机控制：

- WASD/QE 平移
- 鼠标右键旋转
- 鼠标中键平移视角
- 滚轮缩放
- 支持通过按钮重置相机位置与旋转

用于浏览折痕图和三维折纸模型。

### 6. `DesignModeManager.cs`

模式切换管理器：

- 在“折痕设计模式”和“折纸模拟模式”之间切换
- 控制两套 UI 的显示隐藏
- 控制 `CreasePatternEditor` 与 `OrigamiController` 的启用/停用
- 控制 `CreasePattern` 与 `OrigamiSystem` 场景对象显示
- 向控制台文本输出当前模式日志

它负责串联“编辑”和“模拟”两条工作流。

### 7. `GridCameraLink.cs`

把相机矩阵传给网格材质：

- 每帧将相机投影矩阵与世界矩阵写入 Shader 参数
- 用于驱动场景中的网格背景或参考网格显示

### 8. `FaceFinder.cs`

基于半边结构做面查找：

- 根据顶点和折痕重建半边拓扑
- 从点击位置寻找最近折痕
- 计算该折痕所属面
- 返回该面的顶点列表

这个脚本更偏“拓扑分析工具”，适合后续做选面、给面赋属性、局部折叠等功能。

### 9. `Triangulator.cs`

多边形三角剖分工具：

- 使用 Ear Clipping 思路把多边形转成三角面索引
- 便于把任意 polygon 生成为 Unity 可渲染 mesh

当前 `OrigamiLoader.CreateFaces()` 里实际上用了简单扇形三角化，因此这个类更像备用工具或早期工具类。

## 三、CreasePatternEditor 子模块

### 1. `CreasePatternEditor.cs`

折痕图编辑器主脚本，也是编辑模式下的核心：

- 支持编辑模式切换：
  - 添加顶点
  - 添加折痕
  - 移动顶点
  - 删除顶点
  - 删除折痕
  - 编辑折痕属性
- 鼠标点击地面平面创建/选择对象
- 顶点吸附到网格与临近点
- 创建折痕时可指定山折、谷折、边界、平折
- 移动顶点时自动更新相关折痕线段
- 删除顶点后自动重建顶点编号和折痕引用
- 支持保存为 JSON
- 支持从 JSON 重新加载折痕图
- 保存时调用 `OrigamiFaceGenerator` 自动生成面数据

这部分已经具备“点-边式折痕图编辑器”的基本能力。

### 2. `CreaseAttributePanel.cs`

折痕属性弹窗 UI：

- 选中折痕后打开面板
- 修改折痕类型
- 修改最小角度与最大角度
- 点击应用后回调给编辑器主脚本

### 3. `CreaseAttributeEditor.cs`

折痕属性修改逻辑封装：

- 修改折痕类型
- 修改单一角度
- 修改最小/最大角度
- 同步更新 `LineRenderer` 的材质和宽度

这个类偏“属性编辑服务层”，用于把数据修改和显示刷新解耦。

### 4. `GeometryUtils.cs`

几何工具类，提供基础判断：

- 点到线段最近点
- 点是否在多边形内
- 点是否在线段上
- 线段是否相交
- 转角计算
- 顺/逆时针转向判断
- 回路是否为简单多边形

主要给面查找、拓扑分析、自动生成面等功能提供支撑。

### 5. `HalfEdgeMesh.cs`

半边网格结构封装：

- 根据顶点和折痕生成半边数据
- 按顶点出边角度排序
- 建立 `next` / `twin` 关系
- 走访并提取闭合面
- 支持寻找离点击位置最近的半边

这是更通用的拓扑层实现，和 `FaceFinder` 的思路相近，但封装更独立。

### 6. `OrigamiFaceGenerator.cs`

自动面生成器：

- 根据顶点和折痕构造半边
- 按角度排序出边
- 通过 face-walk 方式遍历闭合回路
- 自动输出 `OrigamiFace` 列表
- 会尝试剔除由边界顶点构成的外部面

该脚本是编辑器保存 JSON 时补全 `faces` 数据的关键。

## 四、当前代码体现出的能力与限制

### 已实现能力

- 可以在编辑模式下手工画点、连线、删点、删线
- 可以给折痕设置类型与角度范围
- 可以把折痕图保存成 JSON
- 可以从 JSON 自动生成面和三维折纸对象
- 可以给相邻面创建物理铰链并整体折叠
- 可以通过滑条和键盘控制整体折叠程度
- 可以在“设计模式 / 模拟模式”之间切换

### 当前限制

- `OrigamiController` 仍然是“所有折痕统一进度控制”，还不是单条折痕独立控制
- `SliderController` 依赖“所有铰链角度范围一致”的假设
- `OrigamiLoader.CreateFaces()` 仍然使用简单扇形三角化，对复杂凹多边形可能不够稳
- `SaveToJson()` 中虽然面板支持 `minAngle/maxAngle`，但保存时仍然按 `angle ± 180` 写出，没有直接使用面板修改后的范围
- 物理模拟采用多个刚体 + `HingeJoint`，所以目前仍容易出现穿模/碰撞不稳定

## 五、结论

当前 `scripts` 目录已经形成一套比较完整的“折痕图编辑 -> JSON 模型 -> 运行时加载 -> 整体折叠模拟”链路。

如果后续要继续完善，最关键的方向大致有三项：

1. 从“整体统一折叠”升级为“单折痕独立角度控制”
2. 优化面生成与三角化，减少复杂图形下的错误
3. 处理物理穿模与碰撞稳定性问题
