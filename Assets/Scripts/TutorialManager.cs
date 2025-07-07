using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class TutorialStep
{
    public string message;
    public KeyCode key1;
    public KeyCode key2;
    public VideoClip videoClip;
}

public class TutorialManager : MonoBehaviour
{
    public List<TutorialStep> steps;
    public GameObject popup;
    public TextMeshProUGUI popupText;
    public VideoPlayer videoPlayer;

    private int currentStep = 0;
    private bool isWaitingForInput = false;

    void Start()
    {
        if (steps == null || steps.Count == 0)
        {
            Debug.LogError("チュートリアルステップが設定されていません。");
            return;
        }

        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.isLooping = false;

        ShowCurrentStep();
    }

    void Update()
    {
        if (!isWaitingForInput) return;

        var step = steps[currentStep];
        if (Input.GetKey(step.key1) && Input.GetKeyDown(step.key2))
        {
            isWaitingForInput = false;
            popup.SetActive(false);

            videoPlayer.clip = step.videoClip;
            videoPlayer.Stop();
            videoPlayer.frame = 0;   // ←これで最初のフレームを表示
            videoPlayer.Play();

            Debug.Log("動画再生開始: " + step.videoClip.name);
        }
    }

    void ShowCurrentStep()
    {
        var step = steps[currentStep];
        popupText.text = step.message;
        popup.SetActive(true);
        isWaitingForInput = true;
        Debug.Log("ステップ " + (currentStep + 1) + ": " + step.message);
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("動画終了");

        // フレームを最初に戻し、見た目も戻す
        videoPlayer.Stop();
        videoPlayer.frame = 0;
        videoPlayer.Play();   // 再生して…
        videoPlayer.Pause();  // …即停止して先頭フレームを表示

        currentStep++;
        if (currentStep < steps.Count)
        {
            ShowCurrentStep();
        }
        else
        {
            Debug.Log("チュートリアル完了！");
            popup.SetActive(false);
        }
    }
}