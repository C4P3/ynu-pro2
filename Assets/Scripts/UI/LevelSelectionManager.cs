using UnityEngine;
using Models;

public class LevelSelectionManager : MonoBehaviour
{
    [SerializeField] private LevelSelectButton[] levelButtons;

    void Start()
    {
        UpdateAllButtonVisuals(TypingTextStore.levelSetting);
    }

    public void OnLevelSelected(int selectedValue)
    {
        TypingTextStore.levelSetting = selectedValue;
        UpdateAllButtonVisuals(selectedValue);
    }

    private void UpdateAllButtonVisuals(int selectedValue)
    {
        // buttonの型をLevelSelectButtonに変更
        foreach (LevelSelectButton button in levelButtons)
        {
            if (button == null){ return; }
            if (button.levelValue == selectedValue)
            {
                // LevelSelectButtonに実装したSetSelectedを呼び出す
                button.SetSelected(true);
            }
            else
            {
                button.SetSelected(false);
            }
        }
    }
}