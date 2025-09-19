using UnityEngine;

// 全ての永続オブジェクトの準備が整った後、
// 最初のゲームシーン (MainMenu) へ遷移するためだけのスクリプト
public class Bootstrapper : MonoBehaviour
{
    void Start()
    {
        // Start()はシーン内の全てのAwake()が完了した後に呼ばれる。
        // これにより、GameAppManager.Instanceがnullでないことが保証される。
        GameAppManager.Instance.LoadScene("StartScene");
    }
}