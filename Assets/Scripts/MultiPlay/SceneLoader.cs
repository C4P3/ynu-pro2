using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    void Start()
    {
        // World1とWorld2を現在のシーンに追加でロードする
        SceneManager.LoadScene("World1", LoadSceneMode.Additive);
        SceneManager.LoadScene("World2", LoadSceneMode.Additive);
        Debug.Log("test");
    }
}