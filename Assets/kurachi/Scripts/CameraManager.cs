using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    // �L�����N�^�[�I�u�W�F�N�g
    private GameObject playerObject;
    // �J�����Ƃ̋���
    private Vector3 offset;

    void Start()
    {
        offset = transform.position - playerObject.transform.position;
    }

    void LateUpdate()
    {
        var pos = transform.position;
        Debug.Log($"playerObject.transform.position {playerObject}");
        Debug.Log($"offset {offset}");
        pos = playerObject.transform.position + offset;
        pos.z = -5;
        transform.position = pos;
    }
    public void SetTargetObject(GameObject obj)
    {
        playerObject = obj;
    }
}
