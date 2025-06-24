using UnityEngine;

public class UserConfigButton : MonoBehaviour, IButton
{
    public GameConfigButton gameConfigButton;
    public void OnPointerClick()
    {
        // Implement the logic for when the button is clicked
        Debug.Log("User Config Button Clicked");
        // Here you can add functionality to start or stop recording
        this.gameObject.GetComponent<UnityEngine.UI.Image>().color = new Color(1f, 1f, 1f, 1f);
        gameConfigButton.GetComponent<UnityEngine.UI.Image>().color = new Color(1f, 1f, 1f, 0.5f);

    }
    public void OnPointerEnter()
    {

    }
    public void OnPointerExit()
    {

    }
}
