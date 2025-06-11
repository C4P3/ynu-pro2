// ItemManager.cs
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    [Tooltip("LevelManagerに設定したものと同じアイテムのリストを設定してください")]
    public List<ItemSpawnSetting> itemSpawnSettings;

    // タイルからItemDataを高速に逆引きするための辞書
    private Dictionary<TileBase, ItemData> _itemDatabase;

    void Awake()
    {
        if (Instance == null) { Instance = this; } else { Destroy(gameObject); }
        BuildDatabase();
    }

    private void BuildDatabase()
    {
        _itemDatabase = new Dictionary<TileBase, ItemData>();
        foreach (var setting in itemSpawnSettings)
        {
            if (!_itemDatabase.ContainsKey(setting.itemData.itemTile))
            {
                _itemDatabase.Add(setting.itemData.itemTile, setting.itemData);
            }
        }
    }

    // プレイヤーがアイテムを取得した時に呼ばれる
    public void AcquireItem(TileBase itemTile, Vector3Int itemPosition)
    {
        if (!_itemDatabase.TryGetValue(itemTile, out ItemData data)) return;

        Debug.Log($"Acquired: {data.itemName}");

        // アイテムの種類に応じて効果を発動
        switch (data.effectType)
        {
            case ItemEffectType.OxygenRecovery:
                var oxygenData = data as OxygenRecoveryItemData;
                if (oxygenData != null && GameManager.Instance != null)
                {
                    GameManager.Instance.RecoverOxygen(oxygenData.recoveryAmount);
                }
                break;

            case ItemEffectType.Bomb:
                var bombData = data as BombItemData;
                if (bombData != null && LevelManager.Instance != null)
                {
                    LevelManager.Instance.ExplodeBlocks(itemPosition, bombData.radius);
                }
                break;
        }
    }
}