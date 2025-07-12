using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public AudioSource audioSource;
    public AudioClip startsound;
    public AudioClip gameoversound;
   

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
    
    public void PlaySound(AudioClip clip)
    {
        if (audioSource.clip == clip) return;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void StopSound()
    {
         if (audioSource.isPlaying)
         {
             audioSource.Stop();
         }
    }
}