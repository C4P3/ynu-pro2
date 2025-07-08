// EffectManager.cs
using UnityEngine;
using UnityEngine.Tilemaps; // Tilemapクラスを利用するために追加
using System.Collections; // コルーチンを使うために追加
/// <summary>
/// ゲーム内のエフェクト（パーティクルなど）の再生と管理を行うクラス
/// </summary>
public class EffectManager : MonoBehaviour
{
    // シングルトンパターンの実装
    public static EffectManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// アイテムデータとグリッド座標から、アイテム取得エフェクトを再生する
    /// </summary>
    /// <param name="referenceTilemap">座標変換の基準となるタイルマップ</param>  // ★★★ 引数を追加 ★★★
    public void PlayItemAcquisitionEffect(ItemData itemData, Vector3Int gridPosition, Tilemap referenceTilemap)
    {
        if (itemData == null || itemData.acquisitionEffectPrefab == null) return;

        // ★★★ 引数で渡されたタイルマップを基準にする ★★★
        if (referenceTilemap == null)
        {
            Debug.LogWarning("PlayItemAcquisitionEffectにreferenceTilemapが渡されませんでした。");
            PlayEffect(itemData.acquisitionEffectPrefab, Vector3.zero);
            return;
        }

        Vector3 worldPosition = referenceTilemap.GetCellCenterWorld(gridPosition);
        PlayEffect(itemData.acquisitionEffectPrefab, worldPosition);
    }

    /// <summary>
    /// 指定されたプレハブからエフェクトを生成し、指定の位置で再生する
    /// </summary>
    /// <param name="effectPrefab">再生するエフェクトのGameObjectプレハブ</param>
    /// <param name="position">エフェクトを再生するワールド座標</param>
    public void PlayEffect(GameObject effectPrefab, Vector3 position)
    {
        if (effectPrefab == null)
        {
            Debug.LogWarning("PlayEffect was called with a null prefab.");
            return;
        }

        // 生成したエフェクトのインスタンスを保持する
        GameObject effectInstance = Instantiate(effectPrefab, position, Quaternion.identity);

        // 生成したエフェクトにParticleSystemがついているかチェック
        ParticleSystem ps = effectInstance.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            // ParticleSystemの再生が終了した後にオブジェクトを破棄する
            // ps.main.durationだけだと、パーティクルの生存時間(startLifetime)が考慮されないため、
            // durationとstartLifetimeの最大値を取ることで、おおよその終了時間を担保します。
            // これでも消えない場合は、パーティクルプレハブのStopActionを"Destroy"に設定するのが最も確実です。
            float lifeTime = Mathf.Max(ps.main.duration, ps.main.startLifetime.constantMax);
            Destroy(effectInstance, lifeTime);
        }
        else
        {
            // パーティクルシステムがない場合、5秒後に消去する（保険）
            Destroy(effectInstance, 5f);
            Debug.LogWarning($"The effect '{effectInstance.name}' does not have a ParticleSystem component. It will be destroyed in 5 seconds.");
        }
    }

    /// <summary>
    /// 指定された対象に追従するエフェクトを一定時間再生します。
    /// </summary>
    /// <param name="followTarget">エフェクトが追従する対象</param> // ★★★ 引数を追加 ★★★
    public void PlayFollowEffect(GameObject effectPrefab, float duration, Transform followTarget)
    {
        if (effectPrefab == null) return;
        
        // ★★★ 引数で渡された追従対象を使う ★★★
        if (followTarget == null)
        {
            Debug.LogWarning("PlayFollowEffectにFollow Targetが渡されませんでした。");
            return;
        }

        StartCoroutine(FollowAndDestroyCoroutine(effectPrefab, duration, followTarget));
    }

    /// <summary>
    /// 指定された方向に合わせてエフェクトを再生します。
    /// </summary>
    /// <param name="effectPrefab">再生するエフェクトのプレハブ</param>
    /// <param name="position">再生するワールド座標</param>
    /// <param name="direction">エフェクトの向きを示すVector3Int (例: Vector3Int.right)</param>
    public void PlayDirectionalEffect(GameObject effectPrefab, Vector3 position, Vector3Int direction)
    {
        if (effectPrefab == null)
        {
            Debug.LogWarning("PlayDirectionalEffect was called with a null prefab.");
            return;
        }

        // 方向ベクトルから回転角度を計算 (Y軸が上、X軸が右の2D座標系を想定)
        // Vector3.right (1, 0, 0) を基準に、どのくらい回転させるかを計算します。
        Quaternion rotation = Quaternion.FromToRotation(Vector3.right, (Vector3)direction);

        // エフェクトを生成し、計算した回転を適用します。
        GameObject effectInstance = Instantiate(effectPrefab, position, rotation);

        // GetComponent を GetComponentInChildren に変更します。
        // これにより、プレハブのルートだけでなく、その子オブジェクトも検索して
        // 最初に見つかったParticleSystemを取得します。
        ParticleSystem ps = effectInstance.GetComponentInChildren<ParticleSystem>(); 
        if (ps != null)
        {
            float lifeTime = Mathf.Max(ps.main.duration, ps.main.startLifetime.constantMax);
            Destroy(effectInstance, lifeTime);
        }
        else
        {
            Destroy(effectInstance, 5f);
            Debug.LogWarning($"The effect '{effectInstance.name}' does not have a ParticleSystem component. It will be destroyed in 5 seconds.");
        }
    }

    /// <summary>
    /// エフェクトを追従させ、指定時間後に破棄するコルーチン
    /// </summary>
    private IEnumerator FollowAndDestroyCoroutine(GameObject effectPrefab, float duration, Transform followTarget)
    {
        GameObject effectInstance = Instantiate(effectPrefab, followTarget.position, Quaternion.identity, followTarget);
        yield return new WaitForSeconds(duration);
        if (effectInstance != null)
        {
            Destroy(effectInstance);
        }
    }
}