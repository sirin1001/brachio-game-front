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
    [SerializeField] GameObject ItemSlot1;
    [SerializeField] GameObject ItemSlot2;
    [SerializeField] GameObject ItemSlot3;
    [SerializeField] GameObject ItemSlot4;
    [SerializeField] GameObject ItemSlot5;
    [SerializeField] GameObject ItemSlot6;
    [SerializeField] GameObject ItemSlot7;
    private GameObject NowItemSlot;

    //�摜
    [SerializeField] Sprite Item_message_mouse;
    [SerializeField] Sprite Item_message_gamepad;
    [SerializeField] Sprite ItemSlot_none_Img;
    [SerializeField] Sprite ItemSlot_handGun_Img;
    [SerializeField] Sprite ItemSlot_knife_Img;

    //�ړ����x
    [SerializeField] float speed;

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
    [SerializeField] GameObject handGun_bulletPrefab;
    private GameObject bulletPrefab;

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

    /*SE&BGM*/
    [SerializeField] AudioSource shotSE;
    [SerializeField] AudioSource aimSE;
    [SerializeField] AudioSource dropSE;
    [SerializeField] AudioSource getSE;
    [SerializeField] AudioSource selectSlotSE;

    /*�A�C�e��*/
    enum Item
    {
        none,
        handGun,
        knife,
        herb,
    }

    //�A�C�e���X���b�g�̐�
    [SerializeField]int itemSlotNum = 3;
    //�I�𒆂̃X���b�g
    int nowSlot;
    //�A�C�e���X���b�g
    Item[] itemSlot;

    //�������Ă���A�C�e���͏e��
    bool Item_equal_gun = false;

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
        NowItemSlot = ItemSlot1;
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

    //�A�C�e���X���b�g�̐؂�ւ�
    private void OnSelect_Item(InputValue SelectValue)
    {
        //�A�C�e���؂�ւ��{�^���̓��͒l���󂯎��Vector2
        Vector2 selectV2;
        selectV2 = SelectValue.Get<Vector2>();
        Debug.Log("�A�C�e���Z���N�g�F"+selectV2.y);

        //�A�C�e���؂�ւ��{�^���̓��͒l�����܂��͕��̒l���Ƃ�Ƃ�
        //�����݂̃X���b�g������A�����ɂȂ��Ƃ�
        if(selectV2.y > 0 && nowSlot < itemSlotNum - 1 || selectV2.y < 0 && nowSlot != 0)
        {
            //���̒l�����͂��ꂽ����̃A�C�e���X���b�g�ɐ؂�ւ�
            if (selectV2.y > 0 && nowSlot < itemSlotNum - 1)
            {
                NowItemSlot.transform.localScale = new Vector3(20, 20, 20);
                nowSlot++;
            }
            //���̒l�����͂��ꂽ�����̃A�C�e���X���b�g�ɐ؂�ւ�
            if (selectV2.y < 0 && nowSlot != 0)
            {
                NowItemSlot.transform.localScale = new Vector3(20, 20, 20);
                nowSlot--;
            }
            //�ω�����nowSlot�̒l�ɉ����ăX���b�g�X�V
            switch (nowSlot)
            {
                case 0: NowItemSlot = ItemSlot1; break;
                case 1: NowItemSlot = ItemSlot2; break;
                case 2: NowItemSlot = ItemSlot3; break;
                case 3: NowItemSlot = ItemSlot4; break;
                case 4: NowItemSlot = ItemSlot5; break;
            }
            NowItemSlot.transform.localScale = new Vector3(40, 40, 40);
            selectSlotSE.Play();
        }
    }

    private void Update()
    {

        //�A�C�e���E���{�^���������ꂽ��
        if (_Get_ItemAction.WasPressedThisFrame())
        {
            SpriteRenderer nowSlotImg = NowItemSlot.GetComponent<SpriteRenderer>();
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
                        nowSlotImg.sprite = ItemSlot_handGun_Img;
                    }
                    if (triggerItem.name.Contains("knife"))
                    {
                        itemSlot[nowSlot] = Item.knife;
                        nowSlotImg.sprite = ItemSlot_knife_Img;
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
                nowSlotImg.sprite = ItemSlot_none_Img;
                dropSE.Play();
            }
        }

        //���ݑI�𒆂̃X���b�g�ɉ���������
        if (itemSlot[nowSlot] == Item.none)
        {
            speed = 5f;
            Item_equal_gun = false;
        }
        if (itemSlot[nowSlot] == Item.handGun)
        {
            speed = 4.5f;

            intervaltime = 0.5f;
            bulletSpeed = 1000;
            bulletLostTime = 1f;
            bulletPrefab = handGun_bulletPrefab;
            Item_equal_gun = true;
        }
        if (itemSlot[nowSlot] == Item.knife)
        {
            speed = 5f;

            intervaltime = 0.5f;
            bulletSpeed = 1000;
            bulletPrefab = handGun_bulletPrefab;
            Item_equal_gun = false;
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

        //gamePad�̒l��true�ɕς���ꂽ��
        bool trueCheck = false;

        // �ڑ�����Ă���R���g���[���̖��O�𒲂ׂ�
        var controllerNames = Input.GetJoystickNames();
        // �����R���g���[�����ڑ�����Ă��Ȃ���΃L�[�{�[�h�}�E�X���[�h
        if (controllerNames.Length == 0)//��x���ڑ����ꂽ�`�Ղ��������
            gamePad = false;
        //�ڑ����ꂽ�`�Ղ�����ꍇ�͉ߋ��ɐڑ����ꂽ�R���g���[���̐��������ׂ�
        //��ł��ڑ�����Ă�����gamePad��true�ɂ���
        else for (int i = 0; i < controllerNames.Length; i++)
        {
            if (controllerNames[i] != "")
            {
                gamePad = true;
                trueCheck = true;
                break;
            }
        };
        //trueCheck��false�̂܂܁��R���g���[�����ڑ�����Ă��Ȃ�
        if(!trueCheck)
            gamePad = false;

        /*���_����*/
        if (gamePad)//gamePad�̏ꍇ
        {
            //�X�e�B�b�N��G���Ă��Ȃ��Ƃ���look�̒l��0�Ƃ��ďo�͂����͗l
            //�Ȃ̂ŃX�e�B�b�N����������x�Ƀv���C���[��0�x�̌����������Ȃ��悤��
            //look�̒l���[���̎��ȊO�Ɏ��_��������s���Ă��܂�
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

        if (Item_equal_gun)//�A�C�e�����e�̎�
        {
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
        }
        else
        {
            aimstart = false;
            aimImg.SetActive(false);
        }

        /*�t�@�C�A*/
        // �U���{�^���̉�����Ԏ擾
        bool isFirePressed = _fireAction.IsPressed();

        if (Item_equal_gun)//�A�C�e�����e�̎�
            if (!interval && isFirePressed)//�C���^�[�o������Ȃ����Ȃ甭��
            {
            GameObject bullet = Instantiate(bulletPrefab, bulletPoint.transform.position, Quaternion.identity);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

            // �e���͎��R�ɐݒ�
            bulletRb.AddForce(AngleToVector2(rb.rotation) * bulletSpeed);

            // ���ˉ����o��
            shotSE.Play();

            // ���ԍ��ŖC�e��j�󂷂�
            Destroy(bullet, bulletLostTime);

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