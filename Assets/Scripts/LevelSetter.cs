using UnityEngine;
using UnityEngine.UI;
using Models;  // TypingTextStoreを使うために必要

public class LevelSetter : MonoBehaviour
{
    public int levelValue = 0; // 初級=0, 中級=1, 上級=2

    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(() =>
            {
                TypingTextStore.levelSetting = levelValue;
                Debug.Log($"レベルが {levelValue} に設定されました");
                Debug.Log($"レベルが {TypingTextStore.levelSetting}");
            });
        }
    }
}