using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseRange : MonoBehaviour
{

    NpcController npcController;
    // Start is called before the first frame update
    void Start()
    {
        GameObject parentNPC = transform.parent.gameObject;
        npcController = parentNPC.GetComponent<NpcController>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")||collision.CompareTag("NPC"))
        {
            npcController.Targetting(collision.gameObject);
            npcController.ChaseRangeJudge(true);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") || collision.CompareTag("NPC"))
        {
            npcController.ChaseRangeJudge(false);
            Debug.Log("chaseRangeÇfalseÇ…ÇµÇ‹ÇµÇΩÅB");
        }
    }
}
