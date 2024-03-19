using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMoveController : MonoBehaviour
{
    // �L�����N�^�[�I�u�W�F�N�g
    public GameObject playerObj;
    // �J�����Ƃ̋���
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
