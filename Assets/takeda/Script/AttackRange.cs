using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackRange : MonoBehaviour
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
        if (collision.CompareTag("Player") || collision.CompareTag("NPC"))
        {
            npcController.Targetting(collision.gameObject);
            npcController.AttackRangeJudge(true);
            Debug.Log("AttackRangeÇtrueÇ…ÇµÇ‹ÇµÇΩÅB");
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("NPC"))
        {
            npcController.AttackRangeJudge(false);
            Debug.Log("AttackRangeÇfalseÇ…ÇµÇ‹ÇµÇΩÅB");
        }
    }
}
