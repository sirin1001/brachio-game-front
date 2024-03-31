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

    //�p�[�e�B�N��
    [SerializeField] ParticleSystem bloodFX;
    [SerializeField] ParticleSystem recoveryFX;
    [SerializeField] ParticleSystem deadFX;
    //HP
    private float HP = 100;

    /*SE&BGM*/
    [SerializeField] AudioSource shotSE;
    [SerializeField] AudioSource knifeSE;
    [SerializeField] AudioSource damageSE;
    [SerializeField] AudioSource deadSE;

    //�e��
    private float bulletSpeed;

    //�e�����ł���܂ł̎���
    private float bulletLostTime;

    //���ˈʒu
    [SerializeField] GameObject bulletPoint;
    //���ˍς݂̑ҋ@���Ԓ���
    bool interval = false;
    //���ˊԊu
    [SerializeField] float intervaltime;
    private float temptime;//�C���^�[�o�������p

    //NPC����ɂ��A�C�e���i�Q�[���I�u�W�F�N�g�j
    [SerializeField] GameObject hand_in_knife;
    [SerializeField] GameObject knifeDamageRange;
    [SerializeField] GameObject hand_in_handGun;
    [SerializeField] GameObject hand_in_machineGun;
    //��̃v���n�u
    [SerializeField] GameObject emptyPrefab;

    //�e�ۂ̃v���n�u
    [SerializeField] GameObject handGun_bulletPrefab;
    [SerializeField] GameObject machineGun_bulletPrefab;
    private GameObject bulletPrefab;

    //�ړ����x
    private float speed;

    Vector2 goalPosition;

    //NPC�̏��
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

    //�U���͈́A�ǂ������͈͂̔���
    private bool attackRange;
    private bool chaseRange;

    private GameObject Target;
    private Transform TargetTransform;

    State currentState = State.wait;
    bool stateEnter = true;
    #endregion
    //�A�C�e���ɉ���������
    private void NPCItemProcess(float Speed, float Intervaltime, float BulletSpeed, float BulletLostTime, GameObject Item_bulletPrefab, GameObject hand_in_item)
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
        shotSE = GameObject.Find("shotSE").GetComponent<AudioSource>();
        knifeSE = GameObject.Find("knifeSE").GetComponent<AudioSource>();
        damageSE = GameObject.Find("damageSE").GetComponent<AudioSource>();
        deadSE = GameObject.Find("deadSE").GetComponent<AudioSource>();

        agent = GetComponent<NavMeshAgent2D>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        if (NpcItem == Item.knife)
        {
            NPCItemProcess(4.5f, 0.8f, 1000, 1f, emptyPrefab, hand_in_knife);
        }
        else if (NpcItem == Item.handGun)
        {
            NPCItemProcess(4f, 0.8f, 1000, 0.7f, handGun_bulletPrefab, hand_in_handGun);
        }
        else if (NpcItem == Item.machineGun)
        {
            NPCItemProcess(3f, 0.11f, 1000, 0.7f, machineGun_bulletPrefab, hand_in_machineGun);
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
        //�C���^�[�o������
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
                    Debug.Log("wait���[�h�Ɉڍs");
                }
                if (HP <= 50)
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
                    Debug.Log("wait���[�h�Ɉڍs");
                    int x = Random.Range(-44, 43);
                    int y = Random.Range(-30, 33);
                    goalPosition = new Vector2(x, y);
                    Debug.Log(goalPosition);
                }
                else
                {
                    if (HP <= 80)
                    {
                        HP += 0.5f * Time.deltaTime;
                        if (chaseRange)
                        {
                            Vector2 targetPosition = TargetTransform.position;
                            Vector2 vector2 = (rb.position - targetPosition);
                            Vector2 goalPos = rb.position + vector2 / 10;
                            if (goalPos.x < 43 && goalPos.x >= -44 && goalPos.y < 33 && goalPos.y >= -30)
                                agent.destination = goalPos;
                            if (attackRange)
                            {
                                Vector2 lookDir = targetPosition - rb.position;
                                //�^�[�Q�b�g�Ƃ̊p�x���擾����
                                float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
                                rb.rotation = angle;

                                if (!interval)//�C���^�[�o������Ȃ����Ȃ甭��
                                {
                                    if (NpcItem == Item.knife)//�i�C�t�̏ꍇ
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
                                    else//�A�C�e�����e�̏ꍇ
                                    {
                                        GameObject bullet = Instantiate(bulletPrefab, bulletPoint.transform.position, Quaternion.Euler(0, 0, rb.rotation - 90));
                                        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

                                        // �e���͎��R�ɐݒ�
                                        bulletRb.AddForce(AngleToVector2(rb.rotation) * bulletSpeed);

                                        // ���ˉ����o��
                                        shotSE.Play();

                                        // ���ԍ��ŖC�e��j�󂷂�
                                        Destroy(bullet, bulletLostTime);

                                        interval = true;
                                    }

                                }
                            }
                        }
                        else
                        {
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
                    Debug.Log("walk���[�h�Ɉڍs");
                    agent.speed = 3;
                    //�������ɍ����烉���_���Ȉʒu�Ɉړ�����v���O��������

                    int x = Random.Range(-44, 43);
                    int y = Random.Range(-30, 33);
                    goalPosition = new Vector2(x, y);
                    Debug.Log(goalPosition);

                    agent.destination = goalPosition;
                    Debug.Log("Destination position: " + agent.destination);

                }
                else
                {
                    agent.destination = goalPosition;
                    if ((rb.position - goalPosition).magnitude <= 0.5f)
                        ChangeState(State.wait);
                }
                if (chaseRange)
                {
                    ChangeState(State.chase);
                    return;
                }
                break;

            case State.chase:
                if (stateEnter)
                {
                    stateEnter = false;
                    Debug.Log("chase���[�h�Ɉڍs");
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
                    //�ړI�n���^�[�Q�b�g��
                    agent.destination = targetPosition;

                    Vector2 lookDir = targetPosition - rb.position;
                    //�^�[�Q�b�g�Ƃ̊p�x���擾����
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
                    Debug.Log("attack���[�h�Ɉڍs");
                }
                //��������NPC�̃A�^�b�N�̋��������܂���
                if (HP <= 50)
                {
                    ChangeState(State.escape);
                    return;
                }
                if (attackRange)
                {
                    Vector2 targetPosition = TargetTransform.position;
                    agent.destination = targetPosition;
                    Vector2 lookDir = targetPosition - rb.position;
                    //�^�[�Q�b�g�Ƃ̊p�x���擾����
                    float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
                    rb.rotation = angle;

                    if (!interval)//�C���^�[�o������Ȃ����Ȃ甭��
                    {
                        if (NpcItem == Item.knife)//�i�C�t�̏ꍇ
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
                        else//�A�C�e�����e�̏ꍇ
                        {
                            agent.speed = 2;
                            GameObject bullet = Instantiate(bulletPrefab, bulletPoint.transform.position, Quaternion.Euler(0, 0, rb.rotation - 90));
                            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

                            // �e���͎��R�ɐݒ�
                            bulletRb.AddForce(AngleToVector2(rb.rotation) * bulletSpeed);

                            // ���ˉ����o��
                            shotSE.Play();

                            // ���ԍ��ŖC�e��j�󂷂�
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
        Target = target;
        TargetTransform = Target.transform;
    }

    //�p�x����x�N�g�������߂�
    public static Vector2 AngleToVector2(float angle)
    {
        var radian = angle * (Mathf.PI / 180);
        return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian)).normalized;
    }
    //���S
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
