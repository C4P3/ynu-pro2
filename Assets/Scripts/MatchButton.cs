using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchButton : MonoBehaviour, IButton
{
    public void OnPointerClick(){
        SceneManager.LoadScene("GameScene");
    }
    public void OnPointerEnter(){

    }
    public void OnPointerExit(){

    }
}
