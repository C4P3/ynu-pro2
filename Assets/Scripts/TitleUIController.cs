using UnityEngine;
using UnityEngine.EventSystems;

public class TitleUIController : MonoBehaviour
{
    public GameObject startButton;

    void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(startButton);
    }
}
