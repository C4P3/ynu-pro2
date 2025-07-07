using UnityEngine;

public class StartSceneBGMManager : MonoBehaviour
{
    public static StartSceneBGMManager Instance;

    public AudioSource audioSource;
    public AudioClip menuBGM;
    public AudioClip taisenBGM;
   

    void Awake()
    {
        // シングルトンにする
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーンをまたいでも破棄されないように
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // シーン開始時に menuBGM を自動再生
        if (menuBGM != null && audioSource != null)
        {
            PlayBGM(menuBGM);
        }
        else
        {
            Debug.LogWarning("menuBGM または audioSource が未設定です");
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        if (audioSource.clip == clip) return;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void StopBGM()
    {
         if (audioSource.isPlaying)
         {
             audioSource.Stop();
         }
    }
}