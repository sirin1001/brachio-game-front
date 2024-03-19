using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ToLoginScene : MonoBehaviour
{
    // Start is called before the first frame update
public void ToLoginSceneButton()
    {
        SceneManager.LoadScene("LoginScene");
    }
}
