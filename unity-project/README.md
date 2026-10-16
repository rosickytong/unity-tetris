# Unity Tetris (minimal 2D)
Unity 6000.0.81f1 (6.0 LTS). Whole scene is created in code (Assets/Scripts/Tetris.cs, bootstrapped via RuntimeInitializeOnLoadMethod).
Controls: Left/Right move, Up rotate, Down soft drop, Space hard drop, Space/Enter/R restart after game over.

Build WebGL (needs an activated Unity license on this machine):
  ~/Unity/Hub/Editor/6000.0.81f1/Editor/Unity -batchmode -nographics -quit -upmNoDefaultPackages \
    -projectPath /workspace/unity-tetris -buildTarget WebGL \
    -executeMethod BuildScript.BuildWebGL -logFile /workspace/unity-tetris/build.log
Output: /workspace/unity-tetris/Build/WebGL (compression disabled, static-host friendly).
