using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.AI;
using Unity.VisualScripting;
using static UnityEditor.Progress;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.UIElements;
using TMPro;

public class NpcController : MonoBehaviour
{
    NavMeshAgent2D agent;
    private Rigidbody2D rb;

    //パーティクル
    [SerializeField] ParticleSystem bloodFX;
    [SerializeField] ParticleSystem recoveryFX;
    [SerializeField] ParticleSystem deadFX;
    //HP
    private float HP = 100;

    /*SE&BGM*/
    [SerializeField] AudioSource hG_shotSE;
    [SerializeField] AudioSource mG_shotSE;
    [SerializeField] AudioSource knifeSE;
    [SerializeField] AudioSource damageSE;
    [SerializeField] AudioSource deadSE;

    //弾速
    private float bulletSpeed;

    //弾数
    private float limitedBullet;
    //残弾数
    private float tempBullet;

    //弾が消滅するまでの時間
    private float bulletLostTime;

    //発射位置
    [SerializeField] GameObject bulletPoint;
    //発射済みの待機時間中か
    bool interval = false;
    //発射間隔
    [SerializeField] float intervaltime;
    private float temptime;//インターバル処理用

    //NPCが手にもつアイテム（ゲームオブジェクト）
    [SerializeField] GameObject hand_in_knife;
    [SerializeField] GameObject knifeDamageRange;
    [SerializeField] GameObject hand_in_handGun;
    [SerializeField] GameObject hand_in_machineGun;
    //空のプレハブ
    [SerializeField] GameObject emptyPrefab;

    //弾丸のプレハブ
    [SerializeField] GameObject handGun_bulletPrefab;
    [SerializeField] GameObject machineGun_bulletPrefab;
    private GameObject bulletPrefab;

    //移動速度
    private float speed;

    //移動範囲
    private float x_Max;
    private float x_Min;

    private float y_Max;
    private float y_Min;

    //ランダムな目的地
    private Vector2 goalPosition;
    //範囲設定用ゲームオブジェクト
    GameObject goal1;
    GameObject goal2;

    //NPCの状態
    enum State
    {
        chase,
        attack,
        walk,
        wait,
        escape,
    }
    enum Item
    {
        handGun,
        machineGun,
        knife
    }
    [SerializeField] Item NpcItem;

    #region
    NavMeshAgent navMesh;

    //攻撃範囲、追いかけ範囲の判定
    private bool attackRange;
    private bool chaseRange;

    private GameObject Target;
    private Transform TargetTransform;

    State currentState = State.wait;
    bool stateEnter = true;
    #endregion
    //アイテムに応じた処理
    private void NPCItemProcess(float Speed, float Intervaltime, float BulletSpeed, float BulletLostTime, GameObject Item_bulletPrefab, GameObject hand_in_item,int limitedbullet)
    {
        speed = Speed;
        intervaltime = Intervaltime;
        bulletSpeed = BulletSpeed;
        bulletLostTime = BulletLostTime;
        bulletPrefab = Item_bulletPrefab;
        hand_in_item.SetActive(true);
        limitedBullet = limitedbullet;
        tempBullet = limitedbullet;
    }

    // Start is called before the first frame update
    void Start()
    {
        hG_shotSE = GameObject.Find("hG_shotSE").GetComponent<AudioSource>();
        mG_shotSE = GameObject.Find("mG_shotSE").GetComponent<AudioSource>();

        knifeSE = GameObject.Find("knifeSE").GetComponent<AudioSource>();
        damageSE = GameObject.Find("damageSE").GetComponent<AudioSource>();
        deadSE = GameObject.Find("deadSE").GetComponent<AudioSource>();

        agent = GetComponent<NavMeshAgent2D>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        if (NpcItem == Item.knife)
        {
            NPCItemProcess(4.5f, 0.8f, 1000, 1f, emptyPrefab, hand_in_knife,0);
        }
        else if (NpcItem == Item.handGun)
        {
            NPCItemProcess(4f, 0.8f, 1000, 0.7f, handGun_bulletPrefab, hand_in_handGun,10);
        }
        else if (NpcItem == Item.machineGun)
        {
            NPCItemProcess(3f, 0.11f, 1000, 0.7f, machineGun_bulletPrefab, hand_in_machineGun,30);
        }

        goal1 = GameObject.Find("goal1");
        goal2 = GameObject.Find("goal2");
        x_Max = goal2.transform.position.x;
        x_Min = goal1.transform.position.x;
        y_Max = goal1.transform.position.y;
        y_Min = goal2.transform.position.y;

    }

    void ChangeState(State newState)
    {
        currentState = newState;
        stateEnter = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (HP <= 0)
            Dead();
        //インターバル処理
        if (interval)
        {
            if (temptime >= intervaltime)
            {
                temptime = 0f;
                knifeDamageRange.SetActive(false);
                interval = false;
            }
            else
            {
                temptime += Time.deltaTime;
            }

        }
        switch (currentState)
        {
            case State.wait:
                if (stateEnter)
                {

                    stateEnter = false;
                    Debug.Log("waitモードに移行");
                }
                if (HP <= 40)
                {
                    ChangeState(State.escape);
                    return;
                }
                if (chaseRange)
                {
                    ChangeState(State.chase);
                    return;
                }
                else
                {
                    ChangeState(State.walk);
                }
                break;
            case State.escape:
                if (stateEnter)
                {
                    agent.speed = speed;
                    stateEnter = false;
                    Debug.Log("waitモードに移行");
                    float x = Random.Range(x_Min, x_Max);
                    float y = Random.Range(y_Min, y_Max);
                    goalPosition = new Vector2(x, y);
                    Debug.Log(goalPosition);
                }
                else
                {
                    if (HP <= 60)
                    {                       
                        Vector2 targetPosition = TargetTransform.position;
                        Vector2 vector2 = (rb.position - targetPosition);
                        if (attackRange)
                        {
                            Vector2 lookDir = targetPosition - rb.position;
                            //ターゲットとの角度を取得する
                            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
                            rb.rotation = angle;

                            Vector2 goalPos = rb.position + vector2 / 10;
                            if (goalPos.x < 43 && goalPos.x >= -44 && goalPos.y < 33 && goalPos.y >= -30)
                                agent.destination = goalPos;

                            if (!interval)//インターバルじゃない時なら発射
                            {
                                if (NpcItem == Item.knife)//ナイフの場合
                                {
                                    interval = true;
                                    knifeDamageRange.SetActive(true);
                                    knifeSE.Play();
                                    hand_in_knife.transform.DOLocalMove(new Vector3(0.53f, 0.7f, 0f), 0.2f);
                                    hand_in_knife.transform.DOLocalRotate(new Vector3(0, 0, -50), 0.2f).OnComplete(() =>
                                    {
                                        hand_in_knife.transform.DOLocalMove(new Vector3(0.53f, -0.8f, 0f), 0.2f);
                                        hand_in_knife.transform.DOLocalRotate(new Vector3(0, 0, -137.63f), 0.2f);
                                    });
                                }
                                else//アイテムが銃の場合
                                {
                                    GameObject bullet = Instantiate(bulletPrefab, bulletPoint.transform.position, Quaternion.Euler(0, 0, rb.rotation - 90));
                                    Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

                                    // 弾速は自由に設定
                                    bulletRb.AddForce(AngleToVector2(rb.rotation) * bulletSpeed);

                                    // 発射音を出す
                                    switch (NpcItem)
                                    {
                                        case Item.handGun:hG_shotSE.Play(); break;
                                        case Item.machineGun:mG_shotSE.Play();break;
                                    }

                                    // 時間差で砲弾を破壊する
                                    Destroy(bullet, bulletLostTime);

                                    interval = true;
                                }

                            }
                        }
                        else if (chaseRange)
                        {
                            HP += 0.5f * Time.deltaTime;
                            Vector2 goalPos = rb.position + vector2 / 10;
                            if (goalPos.x < 43 && goalPos.x >= -44 && goalPos.y < 33 && goalPos.y >= -30)
                                agent.destination = goalPos;
                         
                        }
                        else
                        {
                            HP += 1 * Time.deltaTime;
                            agent.destination = goalPosition;
                            if ((rb.position - goalPosition).magnitude <= 0.5f)
                                ChangeState(State.wait);
                        }
                    }
                    else
                    {
                        ChangeState(State.wait); break;
                    }
                }
                break;

            case State.walk:

                if (stateEnter)
                {
                    stateEnter = false;
                    Debug.Log("walkモードに移行");
                    agent.speed = 1;
                    //ランダムな位置に移動

                    float x = Random.Range(x_Min, x_Max);
                    float y = Random.Range(y_Min, y_Max);
                    goalPosition = new Vector2(x, y);
                    Debug.Log(goalPosition);

                    agent.destination = goalPosition;
                    Debug.Log("Destination position: " + agent.destination);

                }
                else if (chaseRange)
                {
                    ChangeState(State.chase);
                    return;
                }
                else
                {
                    if(tempBullet<limitedBullet)
                    {
                        tempBullet += 1 * Time.deltaTime;
                    }
                    agent.destination = goalPosition;
                    if ((rb.position - goalPosition).magnitude <= 0.5f)
                        ChangeState(State.wait);
                }
                
                break;

            case State.chase:
                if (stateEnter)
                {
                    stateEnter = false;
                    Debug.Log("chaseモードに移行");
                }
                if (HP <= 50)
                {
                    ChangeState(State.escape);
                    return;
                }
                if (attackRange)
                {
                    ChangeState(State.attack);
                    return;
                }
                else if (chaseRange)
                {
                    agent.speed = speed;
                    Vector2 targetPosition = TargetTransform.position;
                    //目的地をターゲットに
                    agent.destination = targetPosition;

                    Vector2 lookDir = targetPosition - rb.position;
                    //ターゲットとの角度を取得する
                    float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
                    rb.rotation = angle;
                }
                else
                {
                    ChangeState(State.wait); return;
                }

                break;

            case State.attack:
                if (stateEnter)
                {
                    stateEnter = false;
                    Debug.Log("attackモードに移行");
                }
                //ここからNPCのアタックの挙動書きます↓
                else if (HP <= 40)
                {
                    ChangeState(State.escape);
                    return;
                }
                else if (attackRange)
                {
                    Vector2 targetPosition = TargetTransform.position;
                    if(tempBullet > 0)
                    {
                        agent.destination = targetPosition;
                        Vector2 lookDir = targetPosition - rb.position;
                        //ターゲットとの角度を取得する
                        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
                        rb.rotation = angle;
                    }
                    
                    if (!interval)//インターバルじゃない時なら発射
                    {
                        if (NpcItem == Item.knife)//ナイフの場合
                        {
                            agent.speed = 3;
                            interval = true;
                            knifeDamageRange.SetActive(true);
                            knifeSE.Play();
                            hand_in_knife.transform.DOLocalMove(new Vector3(0.53f, 0.7f, 0f), 0.2f);
                            hand_in_knife.transform.DOLocalRotate(new Vector3(0, 0, -50), 0.2f).OnComplete(() =>
                            {
                                hand_in_knife.transform.DOLocalMove(new Vector3(0.53f, -0.8f, 0f), 0.2f);
                                hand_in_knife.transform.DOLocalRotate(new Vector3(0, 0, -137.63f), 0.2f);
                            });
                        }
                        else//アイテムが銃の場合
                        {
                            //残弾あり
                            if (tempBullet > 0)
                            {
                                tempBullet--;
                                agent.speed = 2;
                                GameObject bullet = Instantiate(bulletPrefab, bulletPoint.transform.position, Quaternion.Euler(0, 0, rb.rotation - 90));
                                Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

                                // 弾速は自由に設定
                                bulletRb.AddForce(AngleToVector2(rb.rotation) * bulletSpeed);

                                // 発射音を出す
                                switch (NpcItem)
                                {
                                    case Item.handGun: hG_shotSE.Play(); break;
                                    case Item.machineGun: mG_shotSE.Play(); break;
                                }

                                // 時間差で砲弾を破壊する
                                Destroy(bullet, bulletLostTime);

                                interval = true;
                            }
                            //残弾なしなら逃げる
                            else
                            {
                                targetPosition = TargetTransform.position;
                                Vector2 vector2 = (rb.position - targetPosition);

                                Vector2 lookDir = targetPosition - rb.position;
                                //ターゲットとの角度を取得する
                                float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
                                rb.rotation = angle;

                                Vector2 goalPos = rb.position + vector2 / 10;
                                if (goalPos.x < 43 && goalPos.x >= -44 && goalPos.y < 33 && goalPos.y >= -30)
                                    agent.destination = goalPos;
                            }
                                
                        }

                    }

                }
                else
                {
                    ChangeState(State.chase); return;
                }
                break;
        }
    }

    //ダメージ処理
    public void Damage(int damageValue)
    {
        damageSE.Play();
        HP -= damageValue;//hpを減らす

        gameObject.GetComponent<SpriteRenderer>().DOColor(Color.red, 0.15f).OnComplete(() =>
        {
            gameObject.GetComponent<SpriteRenderer>().DOColor(Color.white, 0.1f);
            bloodFX.Play();
        });
    }

    //範囲に入っているかの判定
    public void ChaseRangeJudge(bool judge)
    {
        chaseRange = judge;
    }
    public void AttackRangeJudge(bool judge)
    {
        attackRange = judge;
    }

    //対象を特定
    public void Targetting(GameObject target)
    {
        Target = target;
        TargetTransform = Target.transform;
    }

    //角度からベクトルを求める
    public static Vector2 AngleToVector2(float angle)
    {
        var radian = angle * (Mathf.PI / 180);
        return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian)).normalized;
    }
    //死亡
    private void Dead()
    {
        deadSE.Play();
        deadFX.Play();
        gameObject.GetComponent<SpriteRenderer>().DOColor(Color.red, 0.3f).OnComplete(() =>
        {

            gameObject.GetComponent<SpriteRenderer>().DOColor(Color.white, 0.1f).OnComplete(() =>
            {
                Destroy(this.gameObject);
            });

        });
    }
}
