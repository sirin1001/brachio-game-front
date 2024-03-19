using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using static UnityEngine.GraphicsBuffer;

public class CameraManager : MonoBehaviour
{
    GameObject _target;
    private void Start()
    {
        if(_target == null)
        {
            _target = this.gameObject;
        }
        
        // GameObject.Find("Main Camera").SetActive(false);
    }
    private void Update()
    {
        var pos = _target.transform.position;
        pos.z = -5;
        transform.position = pos;
    }

    public void SetTargetObject(GameObject Target)
    {
        Debug.Log($"[Debug] SetTargetObject {Target.name}");
        _target = Target;
    }
}
