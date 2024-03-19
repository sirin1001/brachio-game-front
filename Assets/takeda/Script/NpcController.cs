using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.AI;

public class NpcController : MonoBehaviour
{
    NavMeshAgent2D agent;

    //�p�[�e�B�N��
    [SerializeField] ParticleSystem bloodFX;
    [SerializeField] ParticleSystem recoveryFX;

    //�ړ����x
    [SerializeField] float speed;
    //HP
    private int HP = 100;

    /*SE&BGM*/
    [SerializeField] AudioSource shotSE;
    [SerializeField] AudioSource knifeSE;
    [SerializeField] AudioSource damageSE;

    //NPC�̏��
    enum State
    {
        chase,
        attack,
        walk,
        wait
    }

    #region
    NavMeshAgent navMesh;

    //�U���͈́A�ǂ������͈͂̔���
     private bool attackRange;
     private bool chaseRange;

    private Transform TargetTransform;

    //���s���x�A���s���x
    public float walkSpeed = 0.5f;
    public float chaseSpeed = 5f;

    //walk�̈ړ���ϐ�
    private int goal_num;

    //wait�̑҂�����
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
        //Debug�p
        if (chaseRange)
        {
            agent.destination = TargetTransform.position;
        }
        else
        {
        }
    }

    //�_���[�W����
    public void Damage(int damageValue)
    {
        damageSE.Play();
        HP -= damageValue;//hp�����炷

        gameObject.GetComponent<SpriteRenderer>().DOColor(Color.red, 0.15f).OnComplete(() =>
        {
            gameObject.GetComponent<SpriteRenderer>().DOColor(Color.white, 0.1f);
            bloodFX.Play();
        });
    }

    //�͈͂ɓ����Ă��邩�̔���
    public void ChaseRangeJudge(bool judge)
    {
        chaseRange = judge;
    }
    public void AttackRangeJudge(bool judge)
    {
        attackRange = judge;
    }

    //�Ώۂ����
    public void Targetting(GameObject target)
    {
        TargetTransform = target.transform;
    }
}
