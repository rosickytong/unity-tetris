# Unity Tetris

最简单的 Unity 2D 俄罗斯方块，已发布为 WebGL。

**打开游戏：** https://rosickytong.github.io/unity-tetris/

## 操作

- ← / →：移动
- ↑：旋转
- ↓：加速下落
- Space：硬降
- Space / Enter / R：游戏结束后重新开始

## 仓库内容

- 根目录：GitHub Pages 用的 WebGL 构建（`index.html`、`Build/`、`TemplateData/`）
- `unity-project/`：Unity 源码（场景在运行时由 `Assets/Scripts/Tetris.cs` 生成）

## 本地重新打包 WebGL

需要已激活的 Unity 许可证：

```bash
~/Unity/Hub/Editor/6000.0.81f1/Editor/Unity -batchmode -nographics -quit -upmNoDefaultPackages \
  -projectPath /path/to/unity-project -buildTarget WebGL \
  -executeMethod BuildScript.BuildWebGL -logFile build.log
```
