using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;
using System.Collections;
using System.Collections.Generic;
using static UnityEditor.Timeline.TimelinePlaybackControls;
using System;

public class PlayerController : MonoBehaviour
{
    //UI
    [SerializeField] GameObject Item_message;

    //�摜
    [SerializeField] Sprite Item_message_mouse;
    [SerializeField] Sprite Item_message_gamepad;

    //�ړ����x
    [SerializeField] float speed = 0;

    private Rigidbody2D rb;
    private Transform tf;

    //inputSystem����p
    private Vector2 movement;
    private float look;

    //�����GamePad��
    public bool gamePad;

    private Vector2 mousePos;
    [SerializeField] Camera cam;

    //�Ə�
    [SerializeField] GameObject aimImg;
    //���ł�aim�J�n������
    bool aimstart = false;

    // ���͂��󂯎��PlayerInput
    [SerializeField] private PlayerInput _playerInput;
    // �A�N�V������
    [SerializeField] private string _aimActionName = "Aim";
    [SerializeField] private string _fireActionName = "Fire";
    [SerializeField] private string _Get_ItemActionName = "Get_Item";
    // �A�N�V����
    private InputAction _aimAction;
    private InputAction _fireAction;
    private InputAction _Get_ItemAction;

    //�e�ۂ̃v���n�u
    [SerializeField] GameObject bulletPrefab;
    //���ˈʒu
    [SerializeField] GameObject bulletPoint;
    //���ˍς݂̑ҋ@���Ԓ���
    bool interval = false;
    //���ˊԊu
    [SerializeField] float intervaltime;
    private float temptime;//�C���^�[�o�������p

    /*SE&BGM*/
    [SerializeField] AudioSource shotSE;
    [SerializeField] AudioSource aimSE;
    [SerializeField] AudioSource dropSE;
    [SerializeField] AudioSource getSE;

    /*�A�C�e��*/
    enum Item
    {
        none,
        handGun,
        knife,
        herb,

    }

    //�A�C�e���X���b�g�̐�
    int itemSlotNum = 3;
    //�I�𒆂̃X���b�g
    int nowSlot;
    //�A�C�e���X���b�g
    Item[] itemSlot;

    //�ڐG���Ă���A�C�e��
    private GameObject triggerItem;

    /*�A�C�e���v���n�u*/
    [SerializeField] GameObject handGunPrefab;
    [SerializeField] GameObject knifePrefab;


    // Start is called before the first frame update
    void Start()
    {
        // �v���C���[�ɃA�^�b�`����Ă���R���|�[�l���g���擾
        rb = gameObject.GetComponent<Rigidbody2D>();
        tf = gameObject.GetComponent<Transform>();
        //�A�N�V������PlayerInput����擾
        _aimAction = _playerInput.actions[_aimActionName];
        _fireAction = _playerInput.actions[_fireActionName];
        _Get_ItemAction = _playerInput.actions[_Get_ItemActionName];

        //�A�C�e���X���b�g�쐬
        itemSlot = new Item[itemSlotNum];
        //Array.Resize(ref itemSlot, 4);������Ȋ����ŃX���b�g�̗e�ʕς�����
        nowSlot = 0;
        //�A�C�e���X���b�g������
        for (int i = 0; i < itemSlotNum; i++)
        {
            itemSlot[i] = Item.none;
        }

        Item_message.SetActive(false);
    }

    private void OnMove(InputValue movementValue)
    {
        // Move�A�N�V�����̓��͒l���擾
        movement = movementValue.Get<Vector2>();
    }

    private void OnLook(InputValue LookValue)
    {
        if (gamePad)//gamePad���[�h�Ȃ�
        {
            //Look�A�N�V�����̓��͒l���擾
            //Vector���擾���p�x�ɕϊ�
            look = Vector2ToAngle(LookValue.Get<Vector2>());
        }
    }

    private void Update()
    {

        //�A�C�e���E���{�^���������ꂽ��
        if (_Get_ItemAction.WasPressedThisFrame())
        {
            //���ݑI�𒆂̃X���b�g�ɉ����A�C�e�����Ȃ���
            if (itemSlot[nowSlot] == Item.none)
            {
                //�A�C�e���ɐڐG���Ă�����
                if (itemTrigger)
                {
                    //�ڐG���Ă���A�C�e�������݂̃X���b�g�ɂԂ�����
                    if (triggerItem.name.Contains("handGun"))
                    {
                        itemSlot[nowSlot] = Item.handGun;
                    }
                    if (triggerItem.name.Contains("knife"))
                    {
                        itemSlot[nowSlot] = Item.knife;
                    }

                    //�ڐG���Ă���A�C�e��������
                    Destroy(triggerItem);
                    getSE.Play();
                }
            }
            else//�A�C�e���������Ȃ�A�C�e������o
            {
                //�A�C�e���𐶐�����
                if (itemSlot[nowSlot] == Item.handGun)
                {
                    GameObject handGun = Instantiate(handGunPrefab, bulletPoint.transform.position, Quaternion.Euler(0, 0, rb.rotation-90));
                }
                if (itemSlot[nowSlot] == Item.knife)
                {
                    GameObject handGun = Instantiate(knifePrefab, bulletPoint.transform.position, Quaternion.Euler(0,0, rb.rotation));
                }
                if (itemSlot[nowSlot] != Item.none)
                    //���ݑI�𒆂̃X���b�g����ɂ���
                    itemSlot[nowSlot] = Item.none;
                    dropSE.Play();
            }
        }

        //gamePad�̏ꍇ
        if (gamePad)
        {
            Item_message.gameObject.GetComponent<SpriteRenderer>().sprite = Item_message_gamepad;
        }
        //�L�[�}�E�X�̏ꍇ
        else
        {
            Item_message.gameObject.GetComponent<SpriteRenderer>().sprite = Item_message_mouse;
        }


        // �ڑ�����Ă���R���g���[���̖��O�𒲂ׂ�
        var controllerNames = Input.GetJoystickNames();
        // �����R���g���[�����ڑ�����Ă��Ȃ���΃L�[�{�[�h�}�E�X���[�h
        if (controllerNames.Length == 0) gamePad = false;
        else if (controllerNames[0] == "") gamePad = false;
        //�ڑ�����Ă����gamepad���[�h
        else gamePad = true;

        /*���_����*/
        if (gamePad)//gamePad�̏ꍇ
        {
            if (look != 0)
                rb.rotation = look;
        }
        else//�}�E�X�ł̏ꍇ
        {
            mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            //�}�E�X�̌������擾
            Vector2 lookDir = mousePos - rb.position;
            //�}�E�X�̊p�x���擾����
            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
            rb.rotation = angle;
        }

        /*�ړ�����*/
        rb.velocity = movement * speed;

        /*�G�C��*/
        //�G�C���{�^���̉�����Ԏ擾
        bool isAimPressed = _aimAction.IsPressed();
        // �{�^���̉�����Ԃ����O�o��
        //print($"[{_aimActionName}] isPressed = {isPressed}");
        if (isAimPressed)
        {
            if (!aimstart)
            {
                aimSE.Play();
                aimstart = true;
            }
            aimImg.SetActive(true);
        }
        else
        {
            aimstart = false;
            aimImg.SetActive(false);
        }

        /*�t�@�C�A*/
        // �U���{�^���̉�����Ԏ擾
        bool isFirePressed = _fireAction.IsPressed();
        //�C���^�[�o������Ȃ����Ȃ甭��
        if (!interval && isFirePressed)
        {
            GameObject bullet = Instantiate(bulletPrefab, bulletPoint.transform.position, Quaternion.identity);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

            // �e���͎��R�ɐݒ�
            bulletRb.AddForce(AngleToVector2(rb.rotation) * 1000);

            // ���ˉ����o��
            shotSE.Play();

            // ���ԍ��ŖC�e��j�󂷂�
            Destroy(bullet, 1.0f);

            interval = true;
        }
        //�C���^�[�o������
        if (interval)
        {
            if (temptime >= intervaltime)
            {
                temptime = 0f;
                interval = false;
            }
            else
            {
                temptime += Time.deltaTime;
            }

        }
    }

    private bool itemTrigger = false;

    private void OnTriggerStay2D(Collider2D collision)
    {
        //�A�C�e���ƐڐG���̎�
        if (collision.CompareTag("Item"))
        {
            Debug.Log("�A�C�e���ƐڐG��");
            itemTrigger = true;
            //���ݑI�𒆂̃X���b�g�ɉ����A�C�e�����Ȃ���
            if (itemSlot[nowSlot] == Item.none)
            {
                //�A�C�e�����E�����b�Z�[�W
                Item_message.SetActive(true);
                triggerItem = collision.gameObject;
            }
        }
    }
        //�A�C�e�����痣�ꂽ�Ƃ�
        void OnTriggerExit2D(Collider2D other)
        {
            //���ꂽ�I�u�W�F�N�g�̃^�O��"Item"�̂Ƃ�
            if (other.CompareTag("Item"))
            {
                Item_message.SetActive(false);
                itemTrigger = false;
            }
        }
        //�x�N�g������p�x�����߂�
        public static float Vector2ToAngle(Vector2 vector)
        {
            return Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
        }
        //�p�x����x�N�g�������߂�
        public static Vector2 AngleToVector2(float angle)
        {
            var radian = angle * (Mathf.PI / 180);
            return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian)).normalized;
        }
}