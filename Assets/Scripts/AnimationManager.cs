using UnityEngine;

/// <summary>
/// スプライトの向きを管理するクラス。
/// </summary>
public class AnimationManager : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    
    [Header("Directional Sprites")]
    [Tooltip("上向きの時のスプライト")]
    public Sprite spriteUp;
    [Tooltip("下向きの時のスプライト")]
    public Sprite spriteDown;
    [Tooltip("左向きの時のスプライト")]
    public Sprite spriteLeft;
    [Tooltip("右向きの時のスプライト")]
    public Sprite spriteRight;

    [Header("Effects")]
    [Tooltip("通常（下左右）のタイピング中に表示するエフェクトのプレハブ")]
    public GameObject typingEffectPrefab;
    [Tooltip("上（後ろ）向き専用のタイピングエフェクトのプレハブ")]
    public GameObject typingEffectPrefab_Back;
    [Tooltip("プレイヤーの中心からエフェクトをどれだけ離すか")]
    public float typingEffectOffset = 0.5f;

    // 現在表示しているタイピングエフェクトのインスタンスを保持する変数
    private GameObject _currentTypingEffectInstance;

    void Awake()
    {
        // プレイヤーのグラフィックを持つ子オブジェクトのSpriteRendererを取得します。
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRendererが見つかりませんでした。プレイヤーの子オブジェクトに配置してください。", this);
        }
    }

    /// <summary>
    /// キャラクターの向きに応じて、表示するスプライトを切り替えます。
    /// </summary>
    /// <param name="direction">プレイヤーの移動方向のベクトル。</param>
    public void UpdateSpriteDirection(Vector3Int direction)
    {
        if (spriteRenderer == null || direction == Vector3Int.zero)
        {
            return;
        }

        // 移動方向に応じてスプライトを切り替える
        if (direction.y > 0) // 上へ移動
        {
            spriteRenderer.sprite = spriteUp;
        }
        else if (direction.y < 0) // 下へ移動
        {
            spriteRenderer.sprite = spriteDown;
        }
        else if (direction.x < 0) // 左へ移動
        {
            spriteRenderer.sprite = spriteLeft;
        }
        else if (direction.x > 0) // 右へ移動
        {
            spriteRenderer.sprite = spriteRight;
        }
    }
    /// <summary>
    /// タイピングエフェクトの表示を開始します。
    /// エフェクトはプレイヤーの子オブジェクトとして生成され、向きに合わせて回転・配置されます。
    /// </summary>
    /// <param name="direction">エフェクトを表示する向き</param>
    public void StartTypingEffect(Vector3Int direction)
    {
        // 既にエフェクトが表示されていれば、一度停止する
        if (_currentTypingEffectInstance != null)
        {
            StopTypingEffect();
        }

        GameObject prefabToUse = null;

        // 向きに応じて使用するプレハブを選択する
        if (direction.y > 0) // 上向き（後ろ）の場合
        {
            prefabToUse = typingEffectPrefab_Back;
        }
        else // それ以外の向き（下、左、右）の場合
        {
            prefabToUse = typingEffectPrefab;
        }

        // 使用するプレハブが設定されていなければ、ここで処理を終了
        if (prefabToUse == null)
        {
            return;
        }
        
        // プレイヤーの位置から、指定された向きにoffset分だけ離れた位置を計算
        Vector3 position = transform.position + ((Vector3)direction * typingEffectOffset);

        // エフェクトを生成し、計算した位置と初期回転を適用
        _currentTypingEffectInstance = Instantiate(prefabToUse, position, Quaternion.identity);

        // エフェクトの向きをdirectionに基づいて設定
        if (direction.y > 0) // 上方向
        {
            _currentTypingEffectInstance.transform.localEulerAngles = new Vector3(0f, 0f, 180f);
        }
        else if (direction.y < 0) // 下方向
        {
            _currentTypingEffectInstance.transform.localEulerAngles = Vector3.zero;

        }
        else if (direction.x < 0) // 左方向
        {
            _currentTypingEffectInstance.transform.localEulerAngles = new Vector3(0f, 0f, -90f);
        }
        else if (direction.x > 0) // 右方向
        {
            _currentTypingEffectInstance.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
        }

        // エフェクトをプレイヤーの子オブジェクトにすることで、プレイヤーの移動に追従させる
        _currentTypingEffectInstance.transform.SetParent(this.transform);
    }

    /// <summary>
    /// 表示されているタイピングエフェクトを停止（破棄）します。
    /// </summary>
    public void StopTypingEffect()
    {
        // エフェクトのインスタンスが存在すれば破棄する
        if (_currentTypingEffectInstance != null)
        {
            Destroy(_currentTypingEffectInstance);
            _currentTypingEffectInstance = null; // 参照をクリア
        }
    }
}