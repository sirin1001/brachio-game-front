using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PhotonPlayerController : MonoBehaviour,IPlayerController
{
    void IPlayerController.OnLook(InputValue LookValue)
    {
        throw new System.NotImplementedException();
    }

    void IPlayerController.OnMove(InputValue movementValue)
    {
        throw new System.NotImplementedException();
    }

    void IPlayerController.OnSelect_Item(InputValue SelectValue)
    {
        throw new System.NotImplementedException();
    }

    void IPlayerController.OnTriggerExit2D(Collider2D other)
    {
        throw new System.NotImplementedException();
    }

    void IPlayerController.OnTriggerStay2D(Collider2D other)
    {
        throw new System.NotImplementedException();
    }

    float IPlayerController.Vector2ToAngle(Vector2 vector)
    {
        throw new System.NotImplementedException();
    }
    Vector2 IPlayerController.AngleToVector2(float angle)
    {
        throw new System.NotImplementedException();
    }
}
