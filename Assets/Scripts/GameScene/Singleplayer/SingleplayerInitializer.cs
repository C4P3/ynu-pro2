// 【役割】シングルプレイの全てを初期化する起動役
// 【配置場所】シングルプレイ用シーンに配置した空のオブジェクト（例: _Initializer）
using UnityEngine;
using UnityEngine.UI;

public class SingleplayerInitializer : MonoBehaviour
{
    [Header("Scene References")]
    [Tooltip("シーンに配置されたプレイヤー（PlayerBrainを持つオブジェクト）")]
    [SerializeField] private PlayerBrain playerBrain;

    [Tooltip("シーンに配置されたワールドのルートオブジェクト")]
    [SerializeField] private GameObject worldContext;
    
    [Tooltip("シーンに配置されたUIのCanvasオブジェクト")]
    [SerializeField] private GameObject uiCanvas;

    void Start()
    {
        // 1. カメラとプレイヤーを接続
        //    (シングルプレイではカメラは1台なのでCamera.mainで探せる)
        var cameraFollow = Camera.main.GetComponent<CameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.SetTarget(playerBrain.transform);
        }

        // 2. UIとManagerを接続
        //    ワールドからGameManagerを探し、UIからSliderを探して、両者を繋ぐ
        GameManager gameManager = worldContext.GetComponentInChildren<GameManager>();
        Slider oxygenSlider = uiCanvas.GetComponentInChildren<Slider>();
        if (gameManager != null && oxygenSlider != null)
        {
            gameManager.SetOxygenSlider(oxygenSlider);
        }

        // ワールドからTypingManagerを探し、そのCanvasにメインカメラを設定する
        TypingManager typingManager = worldContext.GetComponentInChildren<TypingManager>();
        if (typingManager != null)
        {
            Canvas typingCanvas = typingManager.GetComponentInChildren<Canvas>();
            if (typingCanvas != null)
            {
                // シングルプレイでは、メインカメラを割り当てる
                typingCanvas.worldCamera = Camera.main;
            }
        }

        // 3. 最後に、全ての設定が完了したことをプレイヤーの頭脳に伝え、ゲームを開始させる
        playerBrain.InitializeForScene(worldContext, Vector3.zero);
    }
}