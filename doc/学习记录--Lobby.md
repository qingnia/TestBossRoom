# 状态机
> 核心代码在ConnectionManager
```c#
// 网络连接状态与角色选择状态不是一个概念，角色选择过程玩家还是主机/连接中
// 断网状态
internal readonly OfflineState m_Offline = new OfflineState();
// 连接中
internal readonly ClientConnectingState m_ClientConnecting = neClientConnectingState();
// 连接上
internal readonly ClientConnectedState m_ClientConnected = neClientConnectedState();
// 重连中
internal readonly ClientReconnectingState m_ClientReconnecting = neClientReconnectingState();
// 启动主机中
internal readonly StartingHostState m_StartingHost = new StartingHostSta();
// 主机
internal readonly HostingState m_Hosting = new HostingState();
```
# 主机创建
> BossRoom提供了IP端口(Unity Transport)和lobby(Unity Relay)两种业务方式创建主机，对应demo的IP和大厅，实际的创建方法是同一个
```c#
// 无论是IP还是lobby，启动配置都是基于ConnectionMethodBase
public abstract class ConnectionMethodBase
{
    protected ConnectionManager m_ConnectionManager;
    readonly ProfileManager m_ProfileManager;
    protected readonly string m_PlayerName;

    //两个抽象函数，一个创建主机，一个连接主机
    public abstract Task SetupHostConnectionAsync();

    public abstract Task SetupClientConnectionAsync();
// IP+Port的具体参数是ConnectionMethodIP
class ConnectionMethodIP : ConnectionMethodBase
{
    string m_Ipaddress;
    ushort m_Port;
// lobby是ConnectionMethodRelay
class ConnectionMethodRelay : ConnectionMethodBase
{
    LobbyServiceFacade m_LobbyServiceFacade;
    LocalLobby m_LocalLobby;
```
## 先在离线状态里准备创建参数，然后切换到创建主机中的状态
```c#
// Transport方式的初始化
public override void StartHostIP(string playerName, string ipaddress, int port)
{
        var connectionMethod = new ConnectionMethodIP(ipaddress, (ushort)port, m_ConnectionManager, m_ProfileManager, playerName);
    m_ConnectionManager.ChangeState(m_ConnectionManager.m_StartingHost.Configure(connectionMethod));
}
// relay方式的初始化
public override void StartHostLobby(string playerName)
{
        var connectionMethod = new ConnectionMethodRelay(m_LobbyServiceFacade, m_LocalLobby, m_ConnectionManager, m_ProfileManager, playerName);
    m_ConnectionManager.ChangeState(m_ConnectionManager.m_StartingHost.Configure(connectionMethod));
}
```
## 进入创建主机中状态后，起异步服务，创建主机
> NetManager有常用事件，业务监听事件做处理：OnClientConnectedCallback、OnClientDisconnectCallback、OnServerStarted、OnTransportFailure
```c#
async void StartHost()
{
    try
    {
        // 调用Transport或者Relay各自的创建方法，别看，抄就完了
        // Transport是本地创建服务器，是同步；relay需要与unity后台通信，是异步，这里await是给relay用
        await m_ConnectionMethod.SetupHostConnectionAsync();
        Debug.Log($"Created relay allocation with join code {m_LocalLobbRelayJoinCode}")

        //host mode 跟 server mode还不一样，这个游戏是host mode
        // NGO's StartHost launches everything
        if (!m_ConnectionManager.NetworkManager.StartHost())
        {
            OnClientDisconnect(m_ConnectionManager.NetworkManageLocalClientId);
        }
    }
    catch (Exception)
    {
        StartHostFailed();
        throw;
    }
}
## 创建完成后切到主机状态
// host mode和server mode的响应都是OnServerStarted，需要注册委托才能用
public override void OnServerStarted()
{
    m_ConnectStatusPublisher.Publish(ConnectStatus.Success);
    m_ConnectionManager.ChangeState(m_ConnectionManager.m_Hosting);
}

// 场景管理由服务器控制
```
# 客户端连接
> 客户端连接和重连的参数都要设置好，切到连接状态
```c#
internal async Task ConnectClientAsync()
{
    try
    {
        //同服务器，await主要是给Relay服务用，跟unity后台通信
        // Setup NGO with current connection method
        await m_ConnectionMethod.SetupClientConnectionAsync()
        // NetManager连接的时候，会调用Transport的连接，然后调用ClientBindAndConnect
        // NGO's StartClient launches everything
        if (!m_ConnectionManager.NetworkManager.StartClient())
        {
            throw new Exception("NetworkManager StartClient failed");
        
        // 监听场景事件，实际是跟随服务器的场景管理，如果允许不同玩家在不同场景，那就改这个回调
        SceneLoaderWrapper.Instance.AddOnSceneEventCallback();
    }
    catch (Exception e)
    {
        Debug.LogError("Error connecting client, see following exception");
        Debug.LogException(e);
        StartingClientFailedAsync();
        throw;
    }
}
// UTP实际是udp协议，设置了payload就够了，不会真的建立连接，不过这种校验方式也不好，最好还是应该做一下签名校验
// 实际服务器有ApprovalCheck，创建主机状态会做校验；客户端连接也会校验，看注释在ClientConnectingState触发，没找到具体位置
// 猜测是StartClient，往里看有ClientBindAndConnect，但是没看到await
public override async Task SetupClientConnectionAsync()
{
    SetConnectionPayload(GetPlayerId(), m_PlayerName);
    var utp = (UnityTransport)m_ConnectionManager.NetworkManager.NetworkConfig.NetworkTransport;
    utp.SetConnectionData(m_Ipaddress, m_Port);
}
```
## 监听连接成功，切到连接上的状态
```c#
public override void OnClientConnected(ulong _)
{
    m_ConnectStatusPublisher.Publish(ConnectStatus.Success);
    m_ConnectionManager.ChangeState(m_ConnectionManager.m_ClientConnected);
}
```
