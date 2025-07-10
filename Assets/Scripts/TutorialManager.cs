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
    public KeyCode key1;              // 修飾キー（例：Shift）なしの場合は None
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

        if (!string.IsNullOrEmpty(step.message))
        {
            popupText.text = step.message;
            popup.SetActive(true);
            tutorialImage.gameObject.SetActive(false);
            videoPlayer.gameObject.SetActive(false);
        }
        else
        {
            popup.SetActive(false);
        }

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
            ProceedToStep(currentStep + 1);
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("動画終了");

        videoPlayer.Pause();  // 最後のフレームで止める

        isVideoPlaying = false;

        StartCoroutine(WaitAndShowNextStep(0.5f));
    }

    IEnumerator WaitAndShowNextStep(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (currentStep < steps.Count - 1)
        {
            currentStep++;
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

    IEnumerator WaitAndProceedAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        ProceedToStep(currentStep + 1);
    }

    void ProceedToStep(int index)
    {
        if (index >= 0 && index < steps.Count)
        {
            currentStep = index;
            ShowCurrentStep();
        }
    }

    public void OnNextButtonPressed()
    {
        if (isWaitingForInput && !isVideoPlaying && currentStep < steps.Count - 1)
        {
            currentStep += 1;
            ShowCurrentStep();
        }
    }

    public void OnBackButtonPressed()
    {
        if (isWaitingForInput && !isVideoPlaying && currentStep > 0)
        {
            currentStep -= 1;
            ShowCurrentStep();
        }
    }
}