# 适配
1. 项目引用了navMesh库，2022版本已经内置到引擎，因此升级后要删掉库引用
2. auth库的2.4.0有bug，编辑器环境下profile检查会抛异常，导致编辑器环境不能使用lobby，仿照官方，降到2.3.1版本
3. 不清楚为什么打出来的包不能跟编辑器联机，貌似编出来的是release版，没找到改参数的地方，多人调试只能用parrelSync
# 参考文档
> 角色网络对象和显示分离讲的高大上，官方已经废了，现在不分离了

[知乎其他人经验 这个人的代码老旧，部分已经对不上](https://zhuanlan.zhihu.com/p/426092367)

[官方项目代码介绍，更复杂，看起来累，但避免了货不对板](https://docs-multiplayer.unity3d.com/netcode/current/learn/bossroom/bossroom)

> 同步方式很重要，先看这个才能看懂代码

[同步方式](https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/ways-synchronize)
# 语法
## inject
> 一知半解：网上看到的描述说这是种注入，实际使用效果更接近跨类共享数据

[Inject注入介绍](https://blog.csdn.net/qq_35531227/article/details/84839483)
## VContainer
> 看不懂
[VContainer](https://vcontainer.hadashikick.jp/resolving/gameobject-injection)
## readonly
> 声明后，可以在构造函数做一次赋值，之后就不能改变
## event 事件处理
```c#
//注册事件
/// <summary>
/// Server notification when a client requests a different lobby-seat, or locks in theiseat choice
/// </summary>
public event Action<ulong, int, bool> OnClientChangedSeat;
//响应事件
public void OnNetworkSpawn()
{
    if (!NetworkManager.Singleton.IsServer)
    {
        enabled = false;
    }
    else
    {
        NetworkManager.Singleton.OnClientDisconnectCallback +OnClientDisconnectCallback;
        networkCharSelection.OnClientChangedSeat += OnClientChangedSeat
        NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneEvent;
    }
}
public void OnNetworkDespawn()
{
    if (NetworkManager.Singleton)
    {
        NetworkManager.Singleton.OnClientDisconnectCallback -OnClientDisconnectCallback;
        NetworkManager.Singleton.SceneManager.OnSceneEvent -= OnSceneEvent;
    }
    if (networkCharSelection)
    {
        networkCharSelection.OnClientChangedSeat -= OnClientChangedSeat;
    }
}
//触发事件 这个函数声明了ServerRpc，并且函数名格式是xxxServerRpc
//也就是客户端调用，到服务器触发，然后服务器再通过事件到具体执行的函数上
/// <summary>
/// RPC to notify the server that a client has chosen a seat.
/// </summary>
[ServerRpc(RequireOwnership = false)]
public void ChangeSeatServerRpc(ulong clientId, int seatIdx, bool lockedIn)
{
    OnClientChangedSeat?.Invoke(clientId, seatIdx, lockedIn);
}
```
# 初始化
```c#
// 在StartUp做游戏初始化，然后再到开始场景
// 切换场景不销毁的对象，都用了DontDestroyOnLoad
// NetworkManager是NetCode的控制器，挂在全局对象上
// ConnectionManager是BossRoom自己写的组件，声明并且注入了游戏状态机，游戏状态改变时调ChangeState
ClientConnectingState 进入这个状态会起一个异步任务，连接服务器ConnectClientAsync
// 玩家连接，断开服务器，都通过这个管理

// GameDataSource 里存行为列表
using Action = Unity.BossRoom.Gameplay.Actions.Action;
[Tooltip("All Action prototype scriptable objects should be slotted in here")]
[SerializeField]
// 这就是BossRoom的所有技能，都做成asset配置，存到这里了
private Action[] m_ActionPrototypes;

// SceneLoader里SceneLoaderWrapper是BossRoom实现的类
// NetCode自带了一个场景管理NetworkSceneManager，并且官方希望刚开始用NetCode的人都要用这个场景管理
OnSceneEvent附带挺多事件的，加载过程根据这几个状态做进度可视化，直到SynchronizeComplete才做逻辑初始化，可避免掉虚空？
```
# 玩家
```c#
// 对服务器而言玩家只是个网络连接，对客户端而言玩游戏玩的是输入控制
// 玩家不等于游戏角色，只不过玩家的输入控制，影响了游戏角色
// 每个网络连接会绑定一个永久的玩家Prefab，BossRoom里叫PersistentPlayer，它拥有NetworkObject组件，定义在NetworkManager里面，这个东西不销毁

// Startup场景初始化游戏，没有角色也不需要输入控制
// MainMenu已经初始化了游戏，需要指定服务器，输入控制实际上控制的是配置和网络连接，没角色
// CharSelect连上了服务器，但是也不需要角色，输入控制就是组队的UI交互
// BossRoom才真正有了游戏角色，根据网络连接创建角色通知客户端，客户端再判断是不是玩家，是不是自己，如果是自己，就绑定输入控制
void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
{
    if (!InitialSpawnDone && loadSceneMode == LoadSceneMode.Single)
    {
        InitialSpawnDone = true;
        // 为每个网络连接创建游戏角色并初始化位置
        foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
        {
            //ServerBossRoomState定义了PlayerPrefab，创建玩家角色的时候就创建这个prefab
            SpawnPlayer(kvp.Key, false);
        }
    }
}

// 上面的SpawnPlayer常规执行创建，关键是最后
// NetworkObject创建好，要调用SpawnWithOwnership，把自己注册进NetworkSpawnManager，注册完成会回调各个组件的OnNetworkSpawn做初始化，游戏就可以开始了
// spawn players characters with destroyWithScene = true
newPlayer.SpawnWithOwnership(clientId, true);
```
# 角色信息的传递
```c#
// guid不是guide，不是引导！！！！是职业id
// NetworkAvatarGuidState的作用根据guid初始化角色信息，因此它专门建了个asset配置AvatarRegistry，管理所有的角色配置
/// <summary>
/// NetworkBehaviour component to send/receive GUIDs from server to clients.
/// </summary>
public class NetworkAvatarGuidState : NetworkBehaviour
{
    //这个GUID会随机初始化，选角色后更新
    [FormerlySerializedAs("AvatarGuidArray")]
    [HideInInspector]
    public NetworkVariable<NetworkGuid> AvatarGuid = new NetworkVariable<NetworkGuid>();
    [SerializeField]
    AvatarRegistry m_AvatarRegistry;
}

//它被声明在PersistentPlayer里面，先是初始化随机一个值
[RequireComponent(typeof(NetworkObject))]
public class PersistentPlayer : NetworkBehaviour
{
    [SerializeField]
    PersistentPlayerRuntimeCollection m_PersistentPlayerRuntimeCollection;
    [SerializeField]
    NetworkNameState m_NetworkNameState;
    [SerializeField]
    NetworkAvatarGuidState m_NetworkAvatarGuidState;
    public NetworkNameState NetworkNameState => m_NetworkNameState;
    public NetworkAvatarGuidState NetworkAvatarGuidState => m_NetworkAvatarGuidState;
}
// 选角色完成的时候会把角色信息存进去，由此可见，这个persistentPlayer，就是用来存储玩家数据并传递的
void SaveLobbyResults()
{
    foreach (NetworkCharSelection.LobbyPlayerState playerInfo in networkCharSelectioLobbyPlayers)
    {
        var playerNetworkObject = NetworkManager.Singleton.SpawnManageGetPlayerNetworkObject(playerInfo.ClientId)
        if (playerNetworkObject && playerNetworkObject.TryGetComponent(ouPersistentPlayer persistentPlayer))
        {
            // pass avatar GUID to PersistentPlayer
            // it'd be great to simplify this with something like NetworkScriptableObjects :(
            persistentPlayer.NetworkAvatarGuidState.AvatarGuid.Value =
                networkCharSelection.AvatarConfiguration[playerInfo.SeatIdx].GuiToNetworkGuid();
        }
    }
}
// BossRoom场景加载完成，SpawnPlayer的时候，将这个值传给PlayerAvatar里的同名组件
// 这个PlayerAvatar只是战斗用的对象，可以被销毁，虽然也有个NetworkAvatarGuidState，但这个state跟对象一样是一次性的，并且因为是新组件，所以m_Avatar为空，
// pass character type from persistent player to avatar
var networkAvatarGuidStateExists =
    newPlayer.TryGetComponent(out NetworkAvatarGuidState networkAvatarGuidState);

Assert.IsTrue(networkAvatarGuidStateExists,
    $"NetworkCharacterGuidState not found on player avatar!");

// if reconnecting, set the player's position and rotation to its previous state
if (lateJoin)
{
    SessionPlayerData? sessionPlayerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(clientId);
    if (sessionPlayerData is { HasCharacterSpawned: true })
    {
        physicsTransform.SetPositionAndRotation(sessionPlayerData.Value.PlayerPosition, sessionPlayerData.Value.PlayerRotation);
    }
}

networkAvatarGuidState.AvatarGuid.Value =
    persistentPlayer.NetworkAvatarGuidState.AvatarGuid.Value;

// ServerCharacter组件引用了这个state，职业信息CharacterClass的数据从这个state拿
// 因为ServerCharacter引用的GuidState是新创建的，所以m_Avatar是空，所以引用RegisteredAvatar的时候会以实际选择的guid初始化，职业就对上了
public CharacterClass CharacterClass
{
    get
    {
        if (m_CharacterClass == null)
        {
            m_CharacterClass = m_State.RegisteredAvatar.CharacterClass;
        
        return m_CharacterClass;
    
    set => m_CharacterClass = value;
}

NetworkAvatarGuidState m_State;
//这个state存了个角色的avatar
[SerializeField]
AvatarRegistry m_AvatarRegistry
Avatar m_Avatar
public Avatar RegisteredAvatar
{
    get
    {
        if (m_Avatar == null)
        {
            //这时候的guid已经被设置为玩家选择的职业ID了
            RegisterAvatar(AvatarGuid.Value.ToGuid());
        
        return m_Avatar;
    }
}
//根据guid注册avatar，serverCharacter的职业在这里设置
void RegisterAvatar(Guid guid)
{
    if (guid.Equals(Guid.Empty))
    {
        // not a valid Guid
        return;
    
    // based on the Guid received, Avatar is fetched from AvatarRegistry
    if (!m_AvatarRegistry.TryGetAvatar(guid, out var avatar))
    {
        Debug.LogError("Avatar not found!");
        return;
    
    if (m_Avatar != null)
    {
        // already set, this is an idempotent call, we don't want to Instantiate twice
        return;
    
    m_Avatar = avatar
    if (TryGetComponent<ServerCharacter>(out var serverCharacter))
    {
        //职业信息写回ServerCharacter
        serverCharacter.CharacterClass = avatar.CharacterClass;
    }
}
// 最终在ClientAvatarGuidHandler组件里创建了客户端能看到的模型
// 这个函数服务器不执行，毕竟服务器不需要跑渲染
// spawn avatar graphics GameObject
Instantiate(m_NetworkAvatarGuidState.RegisteredAvatar.Graphics, m_GraphicsAnimattransform);
```
# 选角色
```c#
// 选人界面的networkCharSelection.LobbyPlayers存放着玩家的选角信息，用来做选人的校验和同步
private NetworkList<LobbyPlayerState> m_LobbyPlayers;
// OnListChanged是NetworkList定义的一个委托，当玩家信息变化时触发事件，游戏接入处理了选角色时的UI响应
// 具体的网络传输疑似被封装进INetworkUpdateSystem里面，VS不能调试进入
// 跟踪ChangeSeatServerRpc发现，非主机不会走进来，说明这个函数是走了网络，到主机上执行的（实际就是典型的ServerRpc用法）
// NetworkListEvent针对NetworkList定义了增删改查操作，业务通过泛型传具体的结构体数据，然后注册响应事件做处理，定制性还可以
//实际代码客户端处理是先对玩家遍历更新，然后再对主角单独处理ready界面

// 遍历列表，找到客户端的id
// now let's find our local player in the list and update the character/info box appropriately
int localPlayerIdx = -1;
for (int i = 0; i < m_NetworkCharSelection.LobbyPlayers.Count; ++i)
{
    if (m_NetworkCharSelection.LobbyPlayers[i].ClientId == NetworkManager.Singleton.LocalClientId)
    {
        localPlayerIdx = i;
        break;
    }
}
```
```txt
// 客户端的ready按钮变化，是监听了服务器消息，下断点可以看到   NetworkList`1:ReadDelta 发生了Value修改，通过OnLobbyPlayerStateChanged回调到UI

Void Unity.BossRoom.Gameplay.UI.UICharSelectClassInfoBox:SetLockedIn (Boolean)+0x1 atC:\sandbox\unity\TestBossRoom\Assets\Scripts\Gameplay\UI\UICharSelectClassInfoBox.cs[62:13-62:77]	C#
Void Unity.BossRoom.Gameplay.GameState.ClientCharSelectState:UpdateCharacterSelection(SeatState, Int32)+0x15f atC:\sandbox\unity\TestBossRoom\Assets\Scripts\Gameplay\GameState\ClientCharSelectState.cs[288:25-288:59]	C#
>Void Unity.BossRoom.Gameplay.GameState.ClientCharSelectState:OnLobbyPlayerStateChanged (NetworkListEvent`1)+0xeb at C:\sandbox\unity\TestBossRoom\Assets\Scripts\Gameplay\GameState\ClientCharSelectState.cs:[228:17-228:166]	C#
Void Unity.Netcode.NetworkList`1:ReadDelta (FastBufferReader, Boolean)+0x47b at \Library\PackageCache\com.unity.netcode.gameobjects@1.20\Runtime\NetworkVariable\Collections\NetworkList.cs:[312:33-318:36]	C#
Void Unity.Netcode.NetworkVariableDeltaMessage:Handle (NetworkContext)+0x2c3 at \Library\PackageCache\com.unity.netcode.gameobjects@1.20\Runtime\Messaging\Messages\NetworkVariableDeltaMessage.cs:[197:25-197:107]	C#
Void Unity.Netcode.MessagingSystem:ReceiveMessage (FastBufferReader, NetworkContext,MessagingSystem)+0xe2 at .\Library\PackageCache\com.unity.netcode.gameobjects@1.20\Runtime\Messaging\MessagingSystem.cs:[511:17-511:45]	C#
Void Unity.Netcode.MessagingSystem:HandleMessage (MessageHeader, FastBufferReader, UInt64,Single, Int32)+0x13c at .\Library\PackageCache\com.unity.netcode.gameobjects@1.20\Runtime\Messaging\MessagingSystem.cs:[384:25-384:67]	C#
Void Unity.Netcode.MessagingSystem:ProcessIncomingMessageQueue ()+0x32 at \Library\PackageCache\com.unity.netcode.gameobjects@1.2.0\Runtime\Messaging\MessagingSystemcs:[404:17-404:122]	C#
Void Unity.Netcode.NetworkManager:OnNetworkEarlyUpdate ()+0x66 at .\Library\PackageCache\comunity.netcode.gameobjects@1.2.0\Runtime\Core\NetworkManager.cs:[1600:13-1600:59]	C#
Void Unity.Netcode.NetworkManager:NetworkUpdate (NetworkUpdateStage)+0x18 at \Library\PackageCache\com.unity.netcode.gameobjects@1.2.0\Runtime\Core\NetworkManager.cs[1532:21-1532:44]	C#
Void Unity.Netcode.NetworkUpdateLoop:RunNetworkUpdateStage (NetworkUpdateStage)+0x2f at \Library\PackageCache\com.unity.netcode.gameobjects@1.2.0\Runtime\Core\NetworkUpdateLoop.cs[185:17-185:51]	C#
Void <>c:<CreateLoopSystem>b__0_0 ()+0x1 at .\Library\PackageCache\com.unity.netcodegameobjects@1.2.0\Runtime\Core\NetworkUpdateLoop.cs:[208:44-208:97]	C#


```
# 战斗
[NetworkBehaviour官方文档 必看代码简单](https://docs-multiplayer.unity3d.com/netcode/current/basics/networkbehavior/index.html)
```c#
// OnLoadEventCompleted 这个事件会生成玩家，实际上会把八个职业都发送给客户端，即使人数不够，客户端也一样会创建8个玩家对象
// ClientCharacter.cs继承自NetworkBehaviour，复写方法OnNetworkSpawn，
// NetworkBehaviour的同步貌似已经被封装了，找不到，估计能直接用，待测试
// 动态创建先OnNetworkSpawn再Start，静态场景的对象先Start再OnNetworkSpawn，是个坑
// 如果不是NPC，那就是玩家
if (!m_ServerCharacter.IsNpc)
// 如果是主角，就给gameObject加上摄像机，再注册输入的监听
if (m_ServerCharacter.IsOwner)
{
    ActionRequestData data = new ActionRequestData { ActionID = GameDataSource.Instance.GeneralTargetActionPrototActionID };
    m_ClientActionViz.PlayAction(ref data);
    gameObject.AddComponent<CameraController>
    if (m_ServerCharacter.TryGetComponent(out ClientInputSender inputSender))
    {
        // TODO: revisit; anticipated actions would play twice on the host
        if (!IsServer)
        {
            inputSender.ActionInputEvent += OnActionInput;
        }
        inputSender.ClientMoveEvent += OnMoveInput;
    }
}
```

## Action
```c#
//BossRoom的协议包基类是ActionRequestData，继承自INetworkSerializable，定义了很多非必填字段，又根据flag做流量优化，如果字段是默认值，就算缺省
//游戏定义了一种资源类型Action，列举了玩家行为，基本可以认为是同步包协议的定义
//发包函数
void SendInput(ActionRequestData action)
{
    ActionInputEvent?.Invoke(action);
    m_ServerCharacter.RecvDoActionServerRPC(action);
}
//组包发送
var data = new ActionRequestData();
PopulateSkillRequest(k_CachedHit[0].point, actionID, ref data);
SendInput(data);

//施放需要指定目标或范围指示器的技能，也一样被包装进Action，字段是ActionInput
//ArcherVolley里面引用的ClientAoeInpuf，就是个指示器
//实际上会生成一个GameObject在场景，挂脚本AoeActionInput控制
var actionPrototype = GameDataSource.Instance.GetActionPrototypeByID(m_ActionRequests[i].RequestedActionID);
if (actionPrototype.Config.ActionInput != null)
{
    //如果ActionInpuf不为空，就生成指示器
    var skillPlayer = Instantiate(actionPrototype.Config.ActionInput);
    //指示器的参数传了委托Action，sendInput，实际就是发包
    skillPlayer.Initiate(m_ServerCharacter, m_PhysicsWrapper.Transform.position, actionPrototype.ActionID, SendInput, FinishSkill);
    m_CurrentSkillInput = skillPlayer;
}
else
{
    PerformSkill(actionPrototype.ActionID, m_ActionRequests[i].TriggerStyle, m_ActionRequests[i].TargetId);
}
//鼠标操作
//IsPointerOverGameObject是unity很常用的方法，判断点击的是UI还是场景
if (!EventSystem.current.IsPointerOverGameObject() && m_CurrentSkillInput == null)
{
    //IsPointerOverGameObject() is a simple way to determine if the mouse is overUI element. If it is, we don't perform mouse input logic,
    //to model the button "blocking" mouse clicks from falling through ainteracting with the worl
    
    //鼠标输入右键按下是攻击
    if (Input.GetMouseButtonDown(1))
    {
        RequestAction(CharacterClass.Skill1.ActionID, SkillTriggerStyle.MouseClick);
    }
    //左键按下是选中目标，朝目标移动
    if (Input.GetMouseButtonDown(0))
    {
        RequestAction(GameDataSource.Instance.GeneralTargetActionPrototype.ActionISkillTriggerStyle.MouseClick);
    }
    //如果当前帧不是按下左键，但左键还没松开，就认为是控制移动
    else if (Input.GetMouseButton(0))
    {
        m_MoveRequest = true;
        --todo：fixUpdate里面通过射线检测更新目标点
    }
}

//用法
var actionPrototype = GameDataSource.Instance.GetActionPrototypeByID(m_ActionRequests[i].RequestedActionID);
if (actionPrototype.Config.ActionInput != null)
{
    var skillPlayer = Instantiate(actionPrototype.Config.ActionInput);
    skillPlayer.Initiate(m_ServerCharacter, m_PhysicsWrapper.Transform.position, actionPrototype.ActionID, SendInput, FinishSkill);
    m_CurrentSkillInput = skillPlayer;
}
else
{
    PerformSkill(actionPrototype.ActionID, m_ActionRequests[i].TriggerStyle, m_ActionRequests[i].TargetId);
}
```
# rpc
##
```c#
//分主机和客机，serverRpc默认只有主机能调用
//如果声明了RequireOwnership=false，客机也能用，如角色选择
        /// <summary>
        /// RPC to notify the server that a client has chosen a seat.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void ChangeSeatServerRpc(ulong clientId, int seatIdx, bool lockedIn)
        {
            OnClientChangedSeat?.Invoke(clientId, seatIdx, lockedIn);
        }
```