using UnityEngine;

[CreateAssetMenu(fileName = "NewOxygenItem", menuName = "TypingDriller/Oxygen Recovery Item Data")]
public class OxygenRecoveryItemData : ItemData
{
    [Header("Oxygen Recovery Settings")]
    public float recoveryAmount; // 酸素の回復量
    
    // スクリプトが作られた時に、最初から設定しておきたい初期値を書く
    private void OnValidate()
    {
        effectType = ItemEffectType.OxygenRecovery;
    }
}