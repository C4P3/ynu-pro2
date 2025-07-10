using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class LoginUIController : MonoBehaviour
{
    public GameObject loginButton;

    void OnEnable()
    {
        StartCoroutine(SelectAfterFrame());
    }

    private IEnumerator SelectAfterFrame()
    {
        yield return null; // UIが完全に初期化されるまで待つ
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(loginButton);
    }
}