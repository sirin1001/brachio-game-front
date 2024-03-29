using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiveDamage : MonoBehaviour
{
    private IPlayerController playerController;
    private NpcController NPCController;
    [SerializeField] private int damageValue;
    [SerializeField] bool Lost;//è¡ñ≈Ç∑ÇÈÇ©

    GameObject MasterPlayer;

    // Start is called before the first frame update
    void Start()
    {
        // GameObject Player = GameObject.Find("Player");
        // playerController = Player.GetComponent<PlayerController>();

        playerController = MasterPlayer.GetComponent<IPlayerController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerController.Damage(damageValue);
            if(Lost)
            Destroy(this.gameObject);
        }
        else if (collision.CompareTag("NPC"))
        {
            NPCController = collision.GetComponent<NpcController>();
            NPCController.Damage(damageValue);
            if (Lost)
                Destroy(this.gameObject);
        }
        
    }

    public void SetMasterplayer(GameObject player)
    {
        MasterPlayer = player;
    }
}
