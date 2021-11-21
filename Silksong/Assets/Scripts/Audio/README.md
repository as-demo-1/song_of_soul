## 音效管理类说明文档

### 解决方案一：单例模式
**名字空间：** AS_2D.Audio

| 脚本                  | 主要功能 | 使用方法 |
| :--------------- | ------------- | ------------- |
| BackgroundMusicPlayer | 播放、切换、暂停BGM等 | 单例模式 |
| RandomAudioPlayer     | 随机播放音效 | 挂载到子物体上，分类管理一系列音效（如若干个脚步声），由父物体引用该脚本并触发 |



### 解决方案二：Scriptable Object

**名字空间：** 无

**文件夹**：AudioSO

**测试场景**：AudioTest，包含BGM和跳跃音效

#### Scriptable Object 解决方案

来自：Unity官方OpenProject

优点：发起音效请求的物体和音效管理器可以存在于不同的场景中。使用对象池，音效不需要全部放在场景里。

AudioCueEventChanelSO 一个负责管理声音的中间通道，有一个UnityAction和一个RaiseEvent，这个用于触发音效，包含三个参数：音效、音效设置、播放位置。用这个脚本创建两个Scriptable Object，分别管背景音乐和音效。

AudioManager 一个音效管理器，继承Monobehavior，挂在到场景物体中。引用两个通道来接收事件，并且通过工厂-对象池系统（Scriptable Object）来产生音效。

AudioCue 在场景中负责请求播放音效。里面挂在一个音效的so，一个设置的so。需要触发音效的时候，就调用其中的函数即可。可以把这个脚本放在角色身上，或子物体。

AudioCueSO 负责保存音效的so，可以设置顺序播放、随机播放等模式，可以将一组音效保存到同一个so上，例如：人物走路地随机脚步声。

AudioConfigurationSO 音效设置so，本项目中仅需使用2DSFX_Config（2d音效设置）和Music_Config（BGM设置）

#### 使用方法步骤

参考测试场景：Asset/Sences/AudioTest，包含bgm循环播放以及跳跃时触发的音效。

1. 在场景中存在AudioManager，可以存在于另一个场景，例如：有关卡场景，负责场景切换的控制器存在于另一个上层场景，游戏开始时两个场景都会加载，音效管理器可以存在于这个上层场景。
2. 创建一个新的AudioCueSO，保存到合适的文件夹，并挂载上需要的音效，设定需要的模式，对于每一个或一组音效，都需要建立一个AudioCueSO，多个音效可以设置随机播放，或顺序播放。
3. 在场景中创建GameObject来挂载AudioCue脚本，并且向该脚本挂载音效so、设置so、channel以及游戏开始时播放设置。在控制音效触发的脚本中引用该脚本。
4. 触发音效 GetComponent<AudioCue>().PlayAudioCue();



### 对象池

工厂 namespace UOP1.Factory

对象池 namespace UOP1.Pool

引用自OpenProject，使用ScriptObject，具体使用方法待补充。
