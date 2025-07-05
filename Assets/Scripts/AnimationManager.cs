using UnityEngine;

/// <summary>
/// プレイヤーのアニメーション（Idle, Walk, Attack）とスプライトの向きを管理するクラス。
/// PlayerオブジェクトにAnimatorコンポーネントと一緒にアタッチしてください。
/// </summary>
[RequireComponent(typeof(Animator))]
public class AnimationManager : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    void Awake()
    {
        // 必要なコンポーネントへの参照を自動で取得します。
        animator = GetComponent<Animator>();
        // プレイヤーのグラフィックを持つ子オブジェクトのSpriteRendererを取得します。
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRendererが見つかりませんでした。プレイヤーの子オブジェクトに配置してください。", this);
        }
    }

    /// <summary>
    /// 歩行アニメーションの状態を設定します。
    /// Animator Controllerの"IsWalking"という名前のboolパラメータを制御します。
    /// </summary>
    /// <param name="isWalking">歩いている状態にする場合はtrueを指定します。</param>
    public void SetWalking(bool isWalking)
    {
        if (animator == null) return;
        animator.SetBool("IsWalking", isWalking);
    }

    /// <summary>
    /// タイピング中（攻撃中）のアニメーション状態を設定します。
    /// このメソッドは、以前のTriggerAttack()を置き換えるものです。
    /// Animator Controllerの"IsTyping"という名前のboolパラメータを制御します。
    /// </summary>
    /// <param name="isTyping">タイピング中の場合はtrueを指定します。</param>
    public void SetTyping(bool isTyping)
    {
        if (animator == null) return;
        // Animatorの"IsTyping"パラメータを更新し、Attack(Typing)アニメーションへ遷移させます。
        animator.SetBool("IsTyping", isTyping);
    }

    /// <summary>
    /// キャラクターの向きを、指定された角度にZ軸回転させて設定します。
    /// </summary>
    /// <param name="direction">プレイヤーの移動方向のベクトル。</param>
    public void UpdateSpriteDirection(Vector3Int direction)
    {
        if (spriteRenderer == null || direction == Vector3Int.zero)
        {
            return;
        }

        Transform spriteTransform = spriteRenderer.transform;
        float angle = spriteTransform.localEulerAngles.z; // 現在の角度を維持

        // 水平方向の移動を優先して角度を決定します
        if (direction.x < 0) // 左へ移動
        {
            angle = 90f;
        }
        else if (direction.x > 0) // 右へ移動
        {
            angle = -90f;
        }
        else if (direction.y < 0) // 下へ移動
        {
            angle = 180f;
        }
        else if (direction.y > 0) // 上へ移動
        {
            angle = 0f;
        }

        // 計算した角度をZ軸の回転として適用します
        spriteTransform.localRotation = Quaternion.Euler(0, 0, angle);
    }
}