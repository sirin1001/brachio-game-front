using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    [SerializeField] string SceneName;
    public void sceneChange()
    {
        SceneManager.LoadScene(SceneName);
    }
}
