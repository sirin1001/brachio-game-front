using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiveDamage : MonoBehaviour
{
    private PlayerController playerController;
    [SerializeField] private int damageValue;

    // Start is called before the first frame update
    void Start()
    {
        GameObject Player = GameObject.Find("Player");
        playerController = Player.GetComponent<PlayerController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerController.Damage(damageValue);
        }
        Destroy(this.gameObject);
    }
}
