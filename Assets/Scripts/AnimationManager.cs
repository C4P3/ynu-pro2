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
}