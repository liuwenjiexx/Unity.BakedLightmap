# Example

## 单场景烘培 

### 步骤

1. 双击打开场景 `Assets/Example/Scenes/SingleScene`

2. 选择 `Main Camera`

3. 双击 `TestSingleScene/Baked Lightmap` 打开烘培设置

4. 烘培白天灯光，点击 light `Bake` 按钮，等待烘培完成 ...

5. 烘培夜晚灯光，点击 night `Bake` 按钮，等待烘培完成 ...

6. 点击 `Play` 按钮 运行

7. 打开 `Game` 窗口，点击 `light` 或 `night`  按钮切换灯光

8. `TestSingleScene` 脚本包含如何切换光照贴图

   

### 应用

- 制作白天和夜晚的光照场景



## 动态物体烘培

### 步骤

1. 双击打开场景`Assets/Example/Scenes/SplitScene`
2. 选择 `Main Camera`
3. 双击 `TestSplitScene/Baked Lightmap` 打开烘培设置
4. 烘培场景，依次点击 `SplitScene`和`SplitScene - bake` 的 `Bake` 按钮，等待烘培完成...
5. 切回基础场景，双击 `SplitScene` 
6. 如果显示为黑色，光照丢失
   1. 点击菜单`Window/Rendering/Lighting` 打开Lighting 窗口
   2. 切换到 Baked Lightmaps 选项卡
   3. `Lighting Data Asset` 重新指定为`Assets/Example/Scenes/SplitScene/LightingData`
7. 点击 `Play` 按钮 运行
8. `TestSplitScene` 脚本包含动态生成物体的代码



### 应用

- 物体需要动态生成，同时需要带烘培光影