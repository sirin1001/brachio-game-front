using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static Unity.Collections.Unicode;
using Fusion;

public class CharacterMove : NetworkBehaviour
{
    [SerializeField] private PlayerInput _playerInput;
    private Vector2 movement;
    float speed = 10f; 
    private Rigidbody2D rb;
    void OnMove(InputValue movementValue)
    {
        // Moveアクションの入力値を取得
        movement = movementValue.Get<Vector2>();
        rb = GetComponent<Rigidbody2D>();
    }

    public override void FixedUpdateNetwork()
    {
        var pos = transform.position;
        pos.x += movement.x * Runner.DeltaTime * speed;
        pos.y += movement.y * Runner.DeltaTime * speed;
        transform.position = pos;
        //rb.velocity = movement * Runner.DeltaTime * speed;
        //Debug.Log($"[Debug] rb.velocity {rb.velocity}");
    }
}
