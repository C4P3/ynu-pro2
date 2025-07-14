using UnityEngine;
using UnityEngine.SceneManagement; // ★ SceneManagerを使うために追加

public class StartSceneBGMManager : MonoBehaviour
{
    public static StartSceneBGMManager Instance;

    public AudioSource audioSource;
    public AudioClip menuBGM;
    public AudioClip taisenBGM;
   
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return; // Awake内でDestroyした後は、以降の処理をしないようにreturnする
        }

        // ★ シーンがロードされた時のイベントにメソッドを登録
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // ★ シーンがロードされるたびに呼び出されるメソッド
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "StartScene")
        {
            // StartSceneならmenuBGMを再生
            PlayBGM(menuBGM);
        }
        else
        {
            // それ以外のシーンならBGMを停止
            StopBGM();
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        if (audioSource.isPlaying && audioSource.clip == clip) return;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void StopBGM()
    {
         if (audioSource != null && audioSource.isPlaying)
         {
             audioSource.Stop();
         }
    }

    // ★ オブジェクトが破棄されるときにイベントの登録を解除（お作法）
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
