using UnityEngine;

[CreateAssetMenu(fileName = "NewInvincibleItem", menuName = "TypingDriller/Invincibility Item Data")]
public class StarItemData : ItemData
{
    [Header("Invincibility Settings")]
    public float invincibilityDuration = 10f; // 無敵時間（秒）

    // スクリプトが作られた時に、最初から設定しておきたい初期値を書く
    private void OnValidate()
    {
        effectType = ItemEffectType.Invincibility;
    }
}