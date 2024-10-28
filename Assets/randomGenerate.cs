using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class randomGenerate : MonoBehaviour
{
    int GenerateEnemyInterval = 30;
    int GenerateItemInterval = 5;
    float enemyTime = 0;
    float itemTime = 0;
    [SerializeField] Vector2 spawnsize;

    [SerializeField] GameObject[] ItemPrafabs= new GameObject[6];
    [SerializeField] GameObject[] EnemyPrafabs = new GameObject[3];
    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0;i<3;i++)
        {
            int enemyNum = Random.Range(0, EnemyPrafabs.Length);
            spawnPrefab(EnemyPrafabs[enemyNum]);
        }
        for(int i=0;i<10;i++)
        {
            int itemNum = Random.Range(0, ItemPrafabs.Length);
            spawnPrefab(ItemPrafabs[itemNum]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //敵をランダムスポーン
        if (enemyTime > GenerateEnemyInterval)
        {
            print("enemySpawn");
            enemyTime = 0;
            if(GenerateEnemyInterval>5)
            {
                GenerateEnemyInterval--;
            }
            int enemyNum = Random.Range(0, EnemyPrafabs.Length);
            spawnPrefab(EnemyPrafabs[enemyNum]);
        }
        else
        {
            enemyTime += Time.deltaTime;
        }

        //アイテムランダムスポーン
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
        if (itemTime>GenerateItemInterval&&items.Length<20)
        {
            print("itemSpawn");
            itemTime = 0;
            int itemNum=Random.Range(0, ItemPrafabs.Length);
            spawnPrefab(ItemPrafabs[itemNum]);
        }
        else
        {
            itemTime += Time.deltaTime;
        }
    }
    private void spawnPrefab(GameObject Prefab)
    {
        //ランダムな位置にアイテムをスポーン
        //確率で排出率を決める
        float x = Random.Range(-spawnsize.x, spawnsize.x);
        float y = Random.Range(-spawnsize.y, spawnsize.y);
        Instantiate(Prefab, new Vector3(x, y, 0), Quaternion.identity);
    }
}