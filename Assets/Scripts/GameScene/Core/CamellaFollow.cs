// 【役割】指定されたターゲットを滑らかに追従する
// 【配置場所】シーン上の、プレイヤーを追いかけるカメラ（MainCamera, P1_Camera, P2_Cameraなど）
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 0, -10); // Zが-10なのは2Dで一般的な設定

    // LateUpdateで追従することで、プレイヤーの移動処理が完了した後にカメラが動く
    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    /// <summary>
    /// 【重要】初期化役から呼び出され、このカメラが追いかけるべきターゲットを設定する
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}