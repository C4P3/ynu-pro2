using UnityEngine;

public class GameSceneBGMManager : MonoBehaviour
{
    public static GameSceneBGMManager Instance;

    public AudioSource audioSource;
    public AudioClip gameBGM;
    public AudioClip kikenBGM;
   
    void Awake()
    {
        // シングルトンにする
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // シーン開始時に menuBGM を自動再生
        if (gameBGM != null && audioSource != null)
        {
            PlayBGM(gameBGM);
        }
        else
        {
            Debug.LogWarning("gameBGM または audioSource が未設定です");
        }
    }
    public void SetBGMState(float pitch)
    {
        if (audioSource == null || audioSource.clip == null)
        return;

            audioSource.pitch = pitch;

          // 再生されていなければ、今設定されている clip を再生
        if (!audioSource.isPlaying && audioSource.clip != null)
        {
             audioSource.Play();
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