# 状态机
> 核心代码在ConnectionManager
```c#
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
## 先在离线状态里准备创建参数，然后切换到创建主机中的状态
> BossRoom提供了IP端口和lobby两种业务方式创建主机，实际的创建方法是同一个
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
## 进入创建主机中状态后，起异步服务，创建主机
```c#
async void StartHost()
{
    try
    {
        // 调用IP或者Relay各自的创建方法，别看，抄就完了
        await m_ConnectionMethod.SetupHostConnectionAsync();
        Debug.Log($"Created relay allocation with join code {m_LocalLobbRelayJoinCode}")

        //实际的服务器创建时这个NetworkManager的StartHost
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
```
## IP连接
```c#
public void StartHostIp(string playerName, string ipaddress, int port)
{
    //StartHostIP这个虚函数只有离线状态做了实现
    m_CurrentState.StartHostIP(playerName, ipaddress, port);
}
//
public override void StartHostIP(string playerName, string ipaddress, int port)
{
    //根据IP和端口创建服务器，这里只是指定服务器参数
    var connectionMethod = new ConnectionMethodIP(ipaddress, (ushort)port, m_ConnectionManager, m_ProfileManager, playerName);
    //切换状态到启动主机中，根据connectionMethod
    m_ConnectionManager.ChangeState(m_ConnectionManager.m_StartingHost.Configure(connectionMethod));
}
//
``# lobby大