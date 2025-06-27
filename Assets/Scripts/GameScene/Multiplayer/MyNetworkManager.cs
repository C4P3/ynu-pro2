// 【新規】
using Mirror;

public class MyNetworkManager : NetworkManager
{
    private int playerCount = 0;

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        if (conn.identity.TryGetComponent<NetworkPlayer>(out var newPlayer))
        {
            newPlayer.playerIndex = playerCount;
            playerCount++;
        }
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        playerCount--;
    }
}