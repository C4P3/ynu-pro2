using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public int playerID = 1;
    private Transform target;

    // カメラとターゲットの相対的な距離を保存する変数
    private Vector3 offset;
    
    // ターゲットを見つけたかどうかのフラグ
    private bool isTargetFound = false;

    void LateUpdate()
    {
        // まだターゲットを見つけていなければ、探す
        if (!isTargetFound)
        {
            string targetTag = "Player" + playerID;
            GameObject playerObject = GameObject.FindWithTag(targetTag);

            if (playerObject != null)
            {
                target = playerObject.transform;

                // ★★★ 修正点 ★★★
                // ターゲットを発見した瞬間の、カメラとターゲットの「位置の差」を計算して保存しておく
                offset = transform.position - target.position;
                
                isTargetFound = true;
                Debug.Log("カメラ" + playerID + "が追従ターゲット(" + target.name + ")を発見！オフセットを計算しました。");
            }
            else
            {
                return;
            }
        }
        
        // ターゲットが見つかっていれば追従する
        if (target != null)
        {
            // ★★★ 修正点 ★★★
            // 常に「ターゲットの位置」に、最初に計算した「位置の差（オフセット）」を足してカメラの位置を決める
            // これでZ座標も正しく保たれる
            transform.position = target.position + offset;
        }
    }
}