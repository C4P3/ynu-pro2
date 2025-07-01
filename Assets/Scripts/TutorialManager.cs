using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class TutorialStep
{
    public string message;             // 説明文
    public KeyCode key1;               // 修飾キー（例：Shift）
    public KeyCode key2;               // 操作キー（例：A）
    public VideoClip videoClip;        // 再生する動画
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
            videoPlayer.Play();
            StartCoroutine(WaitAndProceed());
        }
    }

    void ShowCurrentStep()
    {
        var step = steps[currentStep];
        popupText.text = step.message;
        popup.SetActive(true);
        isWaitingForInput = true;
    }

    IEnumerator WaitAndProceed()
    {
        while (videoPlayer.isPlaying)
            yield return null;

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