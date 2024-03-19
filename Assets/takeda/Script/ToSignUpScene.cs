using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ToSignUpScene : MonoBehaviour
{
    // Start is called before the first frame update
    public void ToSignUpSceneButton()
    {
        SceneManager.LoadScene("SignUpScene");
    }
}
