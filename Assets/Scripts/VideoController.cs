using UnityEngine;
using UnityEngine.Video;

public class VideoStarter : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    private bool isPrepared = false;

    void Start()
    {
        videoPlayer.Stop();       // 再生状態を初期化

        videoPlayer.Prepare();    // 読み込み開始（再びprepareCompletedで再生可能に）
        videoPlayer.prepareCompleted += OnPrepared;
    }

    void Update()
    {
        // Shift + A を押したら再生（準備が完了しているとき）
        if (isPrepared && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.A))
        {
            videoPlayer.Play();
            Debug.Log("動画を再生しました");
        }
    }

    void OnPrepared(VideoPlayer vp)
    {
        videoPlayer.time = 0f;    // 最初のフレームに戻す
        videoPlayer.Pause();       // 再生を一時停止 
        isPrepared = true;
        Debug.Log("動画の準備完了");
    }
}