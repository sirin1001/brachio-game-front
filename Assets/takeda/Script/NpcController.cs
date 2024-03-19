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
    private int HP = 100;

    /*SE&BGM*/
    [SerializeField] AudioSource shotSE;
    [SerializeField] AudioSource knifeSE;
    [SerializeField] AudioSource damageSE;
    [SerializeField] AudioSource deadSE;

    //弾速
    private float bulletSpeed;

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

    //NPCの状態
    enum State
    {
        chase,
        attack,
        walk,
        wait
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
    private void NPCItemProcess(float Speed,float Intervaltime, float BulletSpeed, float BulletLostTime, GameObject Item_bulletPrefab, GameObject hand_in_item)
    {
            speed = Speed;
            intervaltime = Intervaltime;
            bulletSpeed = BulletSpeed;
            bulletLostTime = BulletLostTime;
            bulletPrefab = Item_bulletPrefab;
            hand_in_item.SetActive(true);
    }

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent2D>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        if(NpcItem == Item.knife)
        {
            NPCItemProcess(4.7f,0.5f, 1000, 1f, emptyPrefab, hand_in_knife);
        }
        else if(NpcItem == Item.handGun)
        {
            NPCItemProcess(4f,0.8f, 1000, 0.5f, handGun_bulletPrefab, hand_in_handGun);
        }
        else if(NpcItem == Item.machineGun)
        {
            NPCItemProcess(3f,0.13f, 1000, 0.5f, machineGun_bulletPrefab, hand_in_machineGun);
        }
        
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
                    float ran = Random.Range(0f, 5f);
                    Debug.Log("waitモードに移行");
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

            case State.walk:
                if (stateEnter)
                {
                    stateEnter = false;
                    Debug.Log("walkモードに移行");
                    //↓ここに今からランダムな位置に移動するプログラム書く
                }
                if (chaseRange)
                {
                    ChangeState(State.chase);
                    return;
                }
                //if (navMesh.remainingDistance <= 0.1f && !navMesh.pathPending)
                //{
                //    ChangeState(State.wait); return;
                //}
                break;

            case State.chase:
                if (stateEnter)
                {
                    stateEnter = false;
                    Debug.Log("chaseモードに移行");
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
                
                if (attackRange)
                {
                    Vector2 targetPosition = TargetTransform.position;
                    agent.destination = targetPosition;
                    Vector2 lookDir = targetPosition - rb.position;
                    //ターゲットとの角度を取得する
                    float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
                    rb.rotation = angle;

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
                            agent.speed = 2;
                            GameObject bullet = Instantiate(bulletPrefab, bulletPoint.transform.position, Quaternion.Euler(0, 0, rb.rotation - 90));
                            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

                            // 弾速は自由に設定
                            bulletRb.AddForce(AngleToVector2(rb.rotation) * bulletSpeed);

                            // 発射音を出す
                            shotSE.Play();

                            // 時間差で砲弾を破壊する
                            Destroy(bullet, bulletLostTime);

                            interval = true;
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
