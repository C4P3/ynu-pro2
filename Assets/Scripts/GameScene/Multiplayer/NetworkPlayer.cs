// 【役割】ネットワーク上のプレイヤー。ローカルプレイヤーの初期化を担当
// 【配置場所】プレイヤープレハブ
using UnityEngine;
using UnityEngine.UI;
using Mirror;

[RequireComponent(typeof(PlayerBrain))]
public class NetworkPlayer : NetworkBehaviour
{
    [SerializeField] private PlayerBrain brain;

    [Header("Prefabs to Spawn")]
    [Tooltip("プレイヤーごとに生成するワールド環境のプレハub")]
    public GameObject worldPrefab;
    [Tooltip("プレイヤーごとに生成するUIのプレハブ")]
    public GameObject gameUiPrefab; // ★UIプレハブへの参照

    [SyncVar] public int playerIndex;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        // --- ワールドとUIを生成 ---
        GameObject worldInstance = Instantiate(worldPrefab, GetWorldPosition(), Quaternion.identity);
        GameObject uiInstance = Instantiate(gameUiPrefab);
        
        // --- レイヤー設定 ---
        int worldLayer = LayerMask.NameToLayer(isPlayer1() ? "P1_World" : "P2_World");
        int playerLayer = LayerMask.NameToLayer(isPlayer1() ? "P1_Player" : "P2_Player");
        SetLayerRecursively(worldInstance, worldLayer);
        gameObject.layer = playerLayer;

        // 1. カメラとプレイヤーを接続
        var myPlayerCamera = GetMyCamera(); // P1 or P2 Camera
        if (myPlayerCamera != null)
        {
            myPlayerCamera.GetComponent<CameraFollow>()?.SetTarget(this.transform);
        }
        
        // 2. UIとManagerを接続
        GameManager gameManager = worldInstance.GetComponentInChildren<GameManager>();
        Slider oxygenSlider = uiInstance.GetComponentInChildren<Slider>();
        if (gameManager != null && oxygenSlider != null)
        {
            gameManager.SetOxygenSlider(oxygenSlider);
        }

        // 3. Typing UI とプレイヤー用カメラを接続
        TypingManager typingManager = worldInstance.GetComponentInChildren<TypingManager>();
        if (typingManager != null && myPlayerCamera != null)
        {
            Canvas typingCanvas = typingManager.GetComponentInChildren<Canvas>();
            if (typingCanvas != null)
            {
                // マルチプレイでは、自分のプレイヤーに対応するカメラを割り当てる
                typingCanvas.worldCamera = myPlayerCamera;
            }
        }

        // 3. 最後に、全ての設定が完了したことをプレイヤーの頭脳に伝え、ゲームを開始させる
        brain.InitializeForScene(worldInstance, GetWorldPosition());
    }
    
    // --- 以下、ヘルパー関数 ---
    private bool isPlayer1() => playerIndex == 0;
    private Vector3 GetWorldPosition() => isPlayer1() ? Vector3.zero : new Vector3(1000, 0, 0);
    
    // 自分のカメラを見つける
    private Camera GetMyCamera()
    {
        string cameraName = isPlayer1() ? "P1_Camera" : "P2_Camera";
        GameObject camGo = GameObject.Find(cameraName);
        return camGo?.GetComponent<Camera>();
    }

    private void SetLayerRecursively(GameObject obj, int layer) { /* ... */ }
}