// ItemManager.cs
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// アイテムの出現設定（データと出現確率）を管理するためのクラス
/// </summary>
[System.Serializable]
public class ItemSpawnSetting
{
    public ItemData itemData;
    [Tooltip("他のアイテムと比較したときの出現しやすさ")]
    public float spawnWeight;
}

/// <summary>
/// アイテムに関する全てのデータを管理し、効果を発動するクラス
/// </summary>
public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    [Header("Item Database")]
    [Tooltip("ここですべてのアイテムの種類と出現率を設定します")]
    [SerializeField] private List<ItemSpawnSetting> _itemSpawnSettings;

    // タイルからItemDataを高速に逆引きするための辞書
    private Dictionary<TileBase, ItemData> _itemDatabase;

    void Awake()
    {
        // シングルトンパターンの実装
        if (Instance == null) { Instance = this; } else { Destroy(gameObject); }
        // ゲーム開始時にデータベースを構築する
        BuildDatabase();
    }

    /// <summary>
    /// 設定リストから、タイルをキーとするデータベース（辞書）を作成する
    /// </summary>
    private void BuildDatabase()
    {
        _itemDatabase = new Dictionary<TileBase, ItemData>();
        foreach (var setting in _itemSpawnSettings)
        {
            if (setting.itemData != null && !_itemDatabase.ContainsKey(setting.itemData.itemTile))
            {
                _itemDatabase.Add(setting.itemData.itemTile, setting.itemData);
            }
        }
    }

    /// <summary>
    /// LevelManagerが呼び出すための公開メソッド。
    /// 重み付きランダムで配置すべきアイテムを1つ選んで返す。
    /// </summary>
    public ItemData GetRandomItemToSpawn()
    {
        if (_itemSpawnSettings.Count == 0) return null;
        float totalWeight = _itemSpawnSettings.Sum(item => item.spawnWeight);
        if (totalWeight <= 0) return null;

        float randomValue = Random.Range(0, totalWeight);
        foreach (var setting in _itemSpawnSettings)
        {
            if (randomValue < setting.spawnWeight)
            {
                return setting.itemData;
            }
            randomValue -= setting.spawnWeight;
        }
        return null;
    }

    /// <summary>
    /// プレイヤーがアイテムを取得した時に呼ばれるメソッド
    /// </summary>
    /// <param name="itemTile">取得したアイテムのタイル</param>
    /// <param name="itemPosition">取得したアイテムのタイルマップ座標</param>
    public void AcquireItem(TileBase itemTile, Vector3Int itemPosition)
    {
        // データベースに登録されていないタイルであれば何もしない
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

            case ItemEffectType.Star:
                var starData = data as StarItemData;
                if (starData != null && GameManager.Instance != null)
                {
                    GameManager.Instance.StartCoroutine(GameManager.Instance.TemporaryOxygenInvincibility(starData.invincibleDuration));
                }
                break;
        }
    }
}