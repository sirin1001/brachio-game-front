using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMoveController : MonoBehaviour
{
    // キャラクターオブジェクト
    public GameObject playerObj;
    // カメラとの距離
    private Vector3 offset;

    void Start()
    {
        offset = transform.position - playerObj.transform.position;
    }

    void LateUpdate()
    {
        if (playerObj != null) 
        transform.position = playerObj.transform.position + offset;
    }
}
