using Mirror;
using UnityEngine;

public class SingletonSpawner : NetworkBehaviour
{
    [Header("Prefabs to Spawn")]
    [SerializeField] private GameObject gameDataSyncPrefab;

    public override void OnStartServer()
    {
        // サーバー起動時にGameDataSyncのインスタンスがなければ、プレハブから生成してスポーンさせる
        if (GameDataSync.Instance == null && gameDataSyncPrefab != null)
        {
            Debug.Log("Spawning GameDataSync singleton on server start.");
            GameObject singletonInstance = Instantiate(gameDataSyncPrefab);
            NetworkServer.Spawn(singletonInstance);
        }
    }
}
