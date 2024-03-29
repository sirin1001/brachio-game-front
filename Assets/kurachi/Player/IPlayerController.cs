using UnityEngine;
using UnityEngine.InputSystem;

public interface IPlayerController
{
    void OnMove(InputValue movementValue);
    void OnLook(InputValue LookValue);
    void OnSelect_Item(InputValue SelectValue);
    void OnTriggerStay2D(Collider2D other);
    void OnTriggerExit2D(Collider2D other);
    float Vector2ToAngle(Vector2 vector);
    Vector2 AngleToVector2(float angle);
    void Damage(float damage);
}
