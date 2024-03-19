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

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")||collision.CompareTag("NPC"))
        {
            npcController.ChaseRangeJudge(true);
            npcController.Targetting(collision.gameObject);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player") || other.CompareTag("NPC"))
        {
            npcController.ChaseRangeJudge(false);
        }
    }
}
