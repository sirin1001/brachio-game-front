using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.AI;

public class NpcController : MonoBehaviour
{
    NavMeshAgent2D agent;

    //パーティクル
    [SerializeField] ParticleSystem bloodFX;
    [SerializeField] ParticleSystem recoveryFX;

    //移動速度
    [SerializeField] float speed;
    //HP
    private int HP = 100;

    /*SE&BGM*/
    [SerializeField] AudioSource shotSE;
    [SerializeField] AudioSource knifeSE;
    [SerializeField] AudioSource damageSE;

    //NPCの状態
    enum State
    {
        chase,
        attack,
        walk,
        wait
    }

    #region
    NavMeshAgent navMesh;

    //攻撃範囲、追いかけ範囲の判定
     private bool attackRange;
     private bool chaseRange;

    private Transform TargetTransform;

    //歩行速度、走行速度
    public float walkSpeed = 0.5f;
    public float chaseSpeed = 5f;

    //walkの移動先変数
    private int goal_num;

    //waitの待ち時間
    private float ran;

    State currentState = State.wait;
    bool stateEnter = true;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent2D>();
    }

    void ChangeState(State newState)
    {
        currentState = newState;
        stateEnter = true;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug用
        if (chaseRange)
        {
            agent.destination = TargetTransform.position;
        }
        else
        {
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
        TargetTransform = target.transform;
    }
}
