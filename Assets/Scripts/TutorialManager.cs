using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class TutorialStep
{
    public string message;            // 表示する説明文
    public KeyCode key1;              // 修飾キー（例：Shift）
    public KeyCode key2;              // 入力キー（例：A）
    public VideoClip videoClip;       // 再生する動画（任意）
    public Sprite image;              // 表示する画像（任意）
}

public class TutorialManager : MonoBehaviour
{
    public List<TutorialStep> steps;

    public GameObject popup;
    public TextMeshProUGUI popupText;

    public VideoPlayer videoPlayer;
    public Image tutorialImage;

    public Button nextButton;
    public Button backButton;

    private int currentStep = 0;
    private bool isWaitingForInput = false;
    private bool isVideoPlaying = false;

    void Start()
    {
        if (steps == null || steps.Count == 0)
        {
            Debug.LogError("チュートリアルステップが設定されていません。");
            return;
        }

        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.isLooping = false;
        videoPlayer.gameObject.SetActive(false);
        tutorialImage.gameObject.SetActive(false);

        nextButton.onClick.AddListener(OnNextButtonPressed);
        backButton.onClick.AddListener(OnBackButtonPressed);

        ShowCurrentStep();
    }

    void Update()
    {
        if (!isWaitingForInput || isVideoPlaying) return;

        var step = steps[currentStep];

        if ((step.key1 == KeyCode.None && Input.GetKeyDown(step.key2)) ||
            (step.key1 != KeyCode.None && Input.GetKey(step.key1) && Input.GetKeyDown(step.key2)))
        {
            isWaitingForInput = false;
            StartVideoOrImage(step);
        }
    }

    void ShowCurrentStep()
    {
        var step = steps[currentStep];

        // メッセージが空でない場合のみポップアップ表示
        if (!string.IsNullOrEmpty(step.message))
        {
            popupText.text = step.message;
            popup.SetActive(true);

            // 前の画像・動画は非表示にする（ポップアップがある場合だけ）
            tutorialImage.gameObject.SetActive(false);
            videoPlayer.gameObject.SetActive(false);
        }
        else
        {
            popup.SetActive(false);
            // 前の映像はそのまま残す
        }

        // 🔽 ここでボタンを有効にする（次/前ステップへの移動用）
        nextButton.gameObject.SetActive(true);
        backButton.gameObject.SetActive(true);

        isWaitingForInput = true;
        isVideoPlaying = false;

        Debug.Log($"ステップ {currentStep + 1}: {(step.message == "" ? "[ポップアップなし]" : step.message)}");
    }

    void StartVideoOrImage(TutorialStep step)
    {
        popup.SetActive(false);
        nextButton.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);

        if (step.videoClip != null)
        {
            videoPlayer.clip = step.videoClip;
            videoPlayer.Stop();
            videoPlayer.frame = 0;
            videoPlayer.gameObject.SetActive(true);
            videoPlayer.Play();

            tutorialImage.gameObject.SetActive(false);
            isVideoPlaying = true;

            Debug.Log("動画再生開始: " + step.videoClip.name);
        }
        else if (step.image != null)
        {
            tutorialImage.sprite = step.image;
            tutorialImage.gameObject.SetActive(true);
            videoPlayer.gameObject.SetActive(false);
            StartCoroutine(WaitAndProceedAfterSeconds(2f));
        }
        else
        {
            Debug.LogWarning("動画も画像も設定されていません。");
            ProceedToNextStep();
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("動画終了");
        videoPlayer.Stop();
        videoPlayer.frame = 0;
        videoPlayer.Play();   // 先頭に戻す
        videoPlayer.Pause();

        isVideoPlaying = false;
        ProceedToNextStep();
    }

    IEnumerator WaitAndProceedAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        ProceedToNextStep();
    }

    void ProceedToNextStep()
    {
        currentStep++;
        if (currentStep < steps.Count)
        {
            ShowCurrentStep();
        }
        else
        {
            Debug.Log("チュートリアル完了！");
            popup.SetActive(false);
            nextButton.gameObject.SetActive(false);
            backButton.gameObject.SetActive(false);
        }
    }

    public void OnNextButtonPressed()
    {
        if (isWaitingForInput && currentStep < steps.Count - 1)
        {
            currentStep++;
            ShowCurrentStep();
        }
    }

    public void OnBackButtonPressed()
    {
        if (!isVideoPlaying && currentStep > 0)
        {
            currentStep--;
            ShowCurrentStep();
        }
    }
}