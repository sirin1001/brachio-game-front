using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OffGameManager : MonoBehaviour
{
    int killNum = 0;
    [SerializeField] Text killNum_text;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void UpdateKillNum()
    {
        killNum++;
        killNum_text.text = "ÉLÉãêî:"+killNum.ToString();
    }
}
