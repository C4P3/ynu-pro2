using UnityEngine;
using UnityEngine.SceneManagement;

public class GameAppManager : MonoBehaviour
{
    public static GameAppManager Instance { get; private set; }

    void Awake()
    {
        // シングルトンパターンの設定のみ行う
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void LoadScene(string sceneName)
    {
        MyRelayNetworkManager.Instance.StopHost();
        SceneManager.LoadScene(sceneName);
    }
}
