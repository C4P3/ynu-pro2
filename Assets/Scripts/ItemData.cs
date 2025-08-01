using UnityEngine;
using UnityEngine.Tilemaps;

// アイテムの種類を判別しやすくするためのenum
public enum ItemEffectType
{
    // シングルプレイ用アイテム
    OxygenRecovery,
    Bomb,
    Star,
    Rocket,

    // マルチプレイ用アイテム
    Unchi,
    Poison,
    Thunder
}

[CreateAssetMenu(fileName = "NewItemData", menuName = "TypingDriller/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Common Info")]
    public string itemName;
    public ItemEffectType effectType; // このアイテムがどの種類の効果を持つか
    public TileBase itemTile; // タイルマップに表示されるときのタイル

    [Header("Visuals & Effects")]
    [Tooltip("このアイテムを取得した時に再生されるエフェクトのプレハブ")]
    public GameObject acquisitionEffectPrefab; // アイテム取得時に再生するエフェクト

    [Header("Follow Effect (Optional)")]
    [Tooltip("取得後、プレイヤーに追従して表示されるエフェクトのプレハブ")]
    public GameObject followEffectPrefab; // プレイヤー追従エフェクト
    [Tooltip("追従エフェクトの表示時間（秒）")]
    public float followEffectDuration = 3f; // 表示時間。デフォルトを3秒に設定

    [Header("効果音")]
    public AudioClip useSound;  //アイテム使用時の効果音
}