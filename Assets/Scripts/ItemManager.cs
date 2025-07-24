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

    // 効果音を再生するためのAudioSource
    private AudioSource _audioSource;

    void Awake()
    {
        // シングルトンパターンの実装
        if (Instance == null) { Instance = this; } else { Destroy(gameObject); }

        // AudioSourceの初期化
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false; // 自動再生を無効化
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
    /// LevelManagerが呼び出すための公開メソッド
    /// 重み付きランダムで配置すべきアイテムを1つ選んで返す
    /// </summary>
    /// <param name="prng">使用する疑似乱数生成器</param>
    public ItemData GetRandomItemToSpawn(System.Random prng)
    {
        if (_itemSpawnSettings.Count == 0) return null;
        float totalWeight = _itemSpawnSettings.Sum(item => item.spawnWeight);
        if (totalWeight <= 0) return null;

        // 0からtotalWeightまでのランダムな値を生成
        float randomValue = (float)prng.NextDouble() * totalWeight;

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
    /// アイテム設定リストのインデックスをIDとして、ItemDataを取得する
    /// </summary>
    public ItemData GetItemDataByID(int id)
    {
        if (id >= 0 && id < _itemSpawnSettings.Count)
        {
            return _itemSpawnSettings[id].itemData;
        }
        return null;
    }

    /// <summary>
    /// アイテムの種類(EffectType)を指定して、対応するItemDataを取得する
    /// </summary>
    public ItemData GetItemDataByType(ItemEffectType type)
    {
        foreach (var setting in _itemSpawnSettings)
        {
            if (setting.itemData != null && setting.itemData.effectType == type)
            {
                return setting.itemData;
            }
        }
        return null;
    }

    /// <summary>
    /// ItemDataからID（リストのインデックス）を取得する
    /// </summary>
    private int GetIDByItemData(ItemData data)
    {
        for (int i = 0; i < _itemSpawnSettings.Count; i++)
        {
            if (_itemSpawnSettings[i].itemData == data)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// [Server-side] アイテム取得処理の本体。サーバーでのみ実行される
    /// </summary>
    public void ServerHandleItemAcquisition(TileBase itemTile, Vector3Int itemPosition, PlayerController playerController)
    {
        if (!_itemDatabase.TryGetValue(itemTile, out ItemData data)) return;

        Debug.Log($"[Server] Player {playerController.playerIndex} acquired: {data.itemName}");

        // タイルマップからアイテムを削除
        var networkInput = playerController.GetComponent<NetworkPlayerInput>();
        if (networkInput != null)
        {
            networkInput.RpcRemoveTile(itemPosition, false); // isBlock = false
        }
        playerController.levelManager.itemTilemap.SetTile(itemPosition, null); // サーバー側でも削除

        // アイテムの効果をサーバー側で適用
        ApplyItemEffectOnServer(data, itemPosition, playerController);

        // 全クライアントにエフェクト再生を通知
        // ただし、相手に効果を及ぼすアイテムの場合は、汎用の取得エフェクトは再生しない
        if (data.effectType != ItemEffectType.Thunder && 
            data.effectType != ItemEffectType.Poison && 
            data.effectType != ItemEffectType.Unchi)
        {
            int itemID = GetIDByItemData(data);
            if (itemID != -1)
            {
                NetworkPlayerInput networkPlayerInput = playerController.GetComponent<NetworkPlayerInput>();
                if (networkPlayerInput != null)
                {
                    networkPlayerInput.RpcPlayItemAcquisitionEffects(itemID, itemPosition);
                }
            }
        }
    }

    /// <summary>
    /// [Server-side] アイテム効果をサーバー上で適用する
    /// </summary>
    private void ApplyItemEffectOnServer(ItemData data, Vector3Int itemPosition, PlayerController playerController)
    {
        int playerIndex = playerController.playerIndex;
        LevelManager levelManager = playerController.levelManager;

        switch (data.effectType)
        {
            case ItemEffectType.OxygenRecovery:
                var oxygenData = data as OxygenRecoveryItemData;
                if (oxygenData != null)
                {
                    GameManagerMulti.Instance.ServerRecoverOxygen(playerIndex, oxygenData.recoveryAmount);
                }
                break;

            case ItemEffectType.Bomb:
                var bombData = data as BombItemData;
                if (bombData != null && levelManager != null)
                {
                    // 爆破処理はLevelManagerに任せる。LevelManager内の破壊処理がRPCで同期されている必要がある。
                    levelManager.ExplodeBlocks(itemPosition, bombData.radius, playerController.GetComponent<NetworkPlayerInput>());
                }
                break;

            case ItemEffectType.Star:
                var starData = data as StarItemData;
                if (starData != null)
                {
                    GameManagerMulti.Instance.StartTemporaryInvincibility(playerIndex, starData.invincibleDuration);
                }
                break;

            case ItemEffectType.Rocket:
                var rocketData = data as RocketItemData;
                if (rocketData != null && levelManager != null)
                {
                    NetworkPlayerInput npi = playerController.GetComponent<NetworkPlayerInput>();
                    Vector3Int direction = playerController.GetLastMoveDirection();
                    
                    // 全クライアントでエフェクトを再生
                    npi.RpcPlayRocketEffect(playerController.transform.position, direction, playerController.gameObject);
                    
                    // サーバー側でブロックを破壊
                    rocketData.Activate(playerController.transform, direction, levelManager.blockTilemap);
                }
                break;

            case ItemEffectType.Poison:
                var poisonData = data as PoisonItemData;
                if (poisonData != null)
                {
                    GameManagerMulti.Instance.ServerApplyPoisonToOpponent(playerIndex, poisonData.poisonAmount);
                }
                break;

            case ItemEffectType.Thunder:
                var thunderData = data as ThunderItemData;
                if (thunderData != null)
                {
                    GameManagerMulti.Instance.ServerStunOpponent(playerIndex, thunderData.stunDuration);
                }
                break;
            
            case ItemEffectType.Unchi:
                GameManagerMulti.Instance.ServerPlaceUnchiOnOpponent(playerIndex);
                break;

            // 他のアイテム効果も同様に実装
        }
    }

    /// <summary>
    /// [Client-side] 全てのクライアントでアイテム取得エフェクトとサウンドを再生する
    /// </summary>
    public void PlayItemAcquisitionEffectsOnClient(ItemData data, Vector3Int itemPosition, PlayerController playerController)
    {
        Debug.Log($"[Client] Playing effects for: {data.itemName}");

        // サウンド再生
        if (data.useSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(data.useSound);
        }

        // パーティクルエフェクト再生
        if (EffectManager.Instance != null)
        {
            EffectManager.Instance.PlayItemAcquisitionEffect(data, itemPosition, playerController.levelManager.itemTilemap, playerController.gameObject);
            if (data.followEffectPrefab != null)
            {
                EffectManager.Instance.PlayFollowEffect(data.followEffectPrefab, data.followEffectDuration, playerController.transform, playerController.gameObject);
            }
        }
    }

    /// <summary>
    /// プレイヤーがアイテムを取得した時に呼ばれるメソッド
    /// </summary>
    public void AcquireItem(TileBase itemTile, Vector3Int itemPosition, LevelManager levelManager, Transform playerTransform, NetworkPlayerInput networkPlayerInput = null)
    {
        if (!_itemDatabase.TryGetValue(itemTile, out ItemData data)) return;

        Debug.Log($"Acquired: {data.itemName}");

        levelManager.itemTilemap.SetTile(itemPosition, null);

        if (data.useSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(data.useSound);
        }

        if (EffectManager.Instance != null)
        {
            EffectManager.Instance.PlayItemAcquisitionEffect(data, itemPosition, levelManager.itemTilemap, playerTransform.gameObject);
            if (data.followEffectPrefab != null)
            {
                EffectManager.Instance.PlayFollowEffect(data.followEffectPrefab, data.followEffectDuration, playerTransform, playerTransform.gameObject);
            }
        }

        // アイテムの種類に応じて効果を発動
        switch (data.effectType)
        {
            case ItemEffectType.OxygenRecovery:
                var oxygenData = data as OxygenRecoveryItemData;
                if (oxygenData != null)
                {
                    if (networkPlayerInput != null)
                    {
                        networkPlayerInput.CmdRecoverOxygen(oxygenData.recoveryAmount);
                    }
                    else if (GameManager.Instance != null)
                    {
                        GameManager.Instance.RecoverOxygen(oxygenData.recoveryAmount);
                    }
                }
                break;

            case ItemEffectType.Bomb:
                var bombData = data as BombItemData;
                if (bombData != null && levelManager != null)
                {
                    levelManager.ExplodeBlocks(itemPosition, bombData.radius, networkPlayerInput);
                }
                break;

            case ItemEffectType.Star:
                var starData = data as StarItemData;
                if (starData != null)
                {
                    if (networkPlayerInput != null)
                    {
                        networkPlayerInput.CmdTemporaryOxygenInvincibility(starData.invincibleDuration);
                    }
                    else if (GameManager.Instance != null)
                    {
                        GameManager.Instance.StartCoroutine(GameManager.Instance.TemporaryOxygenInvincibility(starData.invincibleDuration));
                    }
                }
                break;

            case ItemEffectType.Rocket:
                var rocketData = data as RocketItemData;
                if (rocketData != null && levelManager != null)
                {
                    PlayerController playerController = playerTransform.GetComponent<PlayerController>();
                    Vector3Int direction = (playerController != null) ? playerController.GetLastMoveDirection() : Vector3Int.right;
                    if (EffectManager.Instance != null && rocketData.beamEffectPrefab != null)
                    {
                        EffectManager.Instance.PlayDirectionalEffect(rocketData.beamEffectPrefab, playerTransform.position, direction, playerTransform.gameObject);
                    }
                    rocketData.Activate(playerTransform, direction, levelManager.blockTilemap);
                }
                break;

            case ItemEffectType.Unchi:
                var unchiData = data as UnchiItemData;
                if (unchiData != null && levelManager != null)
                {
                    Vector3Int playerGridCenter = levelManager.itemTilemap.WorldToCell(playerTransform.position);
                    unchiData.Activate(playerGridCenter, levelManager.blockTilemap, levelManager.itemTilemap);
                }
                break;

            case ItemEffectType.Poison:
                var poisonData = data as PoisonItemData;
                if (poisonData != null)
                {
                    if (networkPlayerInput != null)
                    {
                        networkPlayerInput.CmdRecoverOxygen(-Mathf.Abs(poisonData.poisonAmount));
                    }
                    else if (GameManager.Instance != null)
                    {
                        GameManager.Instance.RecoverOxygen(-Mathf.Abs(poisonData.poisonAmount));
                    }
                }
                break;

            case ItemEffectType.Thunder:
                var thunderData = data as ThunderItemData;
                if (thunderData != null && playerTransform != null)
                {
                    PlayerController playerController = playerTransform.GetComponent<PlayerController>();
                    if (playerController != null) playerController.Stun(thunderData.stunDuration);
                }
                break;
        }
    }
}