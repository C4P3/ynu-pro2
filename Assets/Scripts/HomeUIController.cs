using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HomeUIController : MonoBehaviour
{
    public GameObject startButton;

    void OnEnable()
    {
        // HomeUI がアクティブになった時に最初のボタンを選択状態にする
        EventSystem.current.SetSelectedGameObject(null); // 一旦クリア
        EventSystem.current.SetSelectedGameObject(startButton);
    }
}
