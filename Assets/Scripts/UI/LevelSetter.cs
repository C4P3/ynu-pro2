using UnityEngine;

public class LevelSelectButton : IButton
{
    [Tooltip("このボタンに割り当てる難易度の値（例: 初級=0, 中級=1, 上級=2）")]
    public int levelValue = 0;

    [Tooltip("レベル選択を全体管理するマネージャー")]
    [SerializeField] private LevelSelectionManager manager;


    /// <summary>
    /// ボタンがクリックされた時の処理
    /// </summary>
    public override void OnPointerClick()
    {
        // 1. Managerに、このボタンの難易度が選択されたことを通知
        if (manager != null)
        {
            manager.OnLevelSelected(levelValue);
        }

        // 2. IButtonの基本的なクリック処理（効果音など）を実行
        base.OnPointerClick();
    }

    /// <summary>
    /// Managerからの指示で見た目を変更する
    /// </summary>
    public void SetSelected(bool isSelected)
    {
        if (isSelected)
        {
            Select();
        }
        else
        {
            Deselect();
        }
    }
}