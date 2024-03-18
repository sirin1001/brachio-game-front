using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;
using System.Collections;
using System.Collections.Generic;
using static UnityEditor.Timeline.TimelinePlaybackControls;
using System;
using DG.Tweening;

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
    [SerializeField] Sprite ItemSlot_machineGun_Img;
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

    //��̃v���n�u
    [SerializeField] GameObject emptyPrefab;

    //�v���C���[����ɂ��A�C�e���i�Q�[���I�u�W�F�N�g�j
    [SerializeField] GameObject hand_in_knife;
    [SerializeField] GameObject hand_in_handGun;
    [SerializeField] GameObject hand_in_machineGun;
    [SerializeField] GameObject hand_in_empty;

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
    [SerializeField] AudioSource knifeSE;

    /*�A�C�e��*/
    enum Item
    {
        none,
        handGun,
        machineGun,
        knife,
        herb,
    }

    //�A�C�e���X���b�g�̐��i�ŏ��͂R�j
    [SerializeField]int itemSlotNum = 3;
    //�I�𒆂̃X���b�g
    int nowSlot;
    //�A�C�e���X���b�g
    Item[] itemSlot;
    //�X���b�g�̏����
    private int MaxSlotNum = 7;

    //�������Ă���A�C�e���͏e��
    bool Item_equal_gun = false;

    //�ڐG���Ă���A�C�e��
    private GameObject triggerItem;

    /*�A�C�e���v���n�u*/
    [SerializeField] GameObject handGunPrefab;
    [SerializeField] GameObject knifePrefab;
    [SerializeField] GameObject machineGunPrefab;


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
        itemSlot = new Item[MaxSlotNum];
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
                case 5: NowItemSlot = ItemSlot6; break;
                case 6: NowItemSlot = ItemSlot7; break;
            }
            NowItemSlot.transform.localScale = new Vector3(40, 40, 40);
            selectSlotSE.Play();
        }
    }

    //�X���b�g�ɉ���������
    void SlotProcess(Item itemName, float Speed, float Intervaltime, float BulletSpeed, float BulletLostTime, GameObject Item_bulletPrefab,GameObject hand_in_item,bool equal_gun)
    {
        if (itemSlot[nowSlot] == itemName)
        {
            speed = Speed;
            intervaltime = Intervaltime;
            bulletSpeed = BulletSpeed;
            bulletLostTime = BulletLostTime;
            bulletPrefab = Item_bulletPrefab;
            Item_equal_gun = equal_gun;
            hand_in_item.SetActive(true);
        }
        else
        {
            hand_in_item.SetActive(false);
        }
    } 

    private void Update()
    {

        //�A�C�e���E���{�^���������ꂽ��
        if (_Get_ItemAction.WasPressedThisFrame())
        {
            SpriteRenderer nowSlotImg = NowItemSlot.GetComponent<SpriteRenderer>();
            //�A�C�e���ɐڐG���Ă�����
            if (itemTrigger)
            {
                //�X���b�g��������ɒB���Ă��Ȃ�
                if (itemSlotNum < MaxSlotNum)
                {
                    //�ڐG���Ă���A�C�e�������Ȃ�A�C�e���X���b�g�𑝂₷
                    if (triggerItem.name.Contains("bag"))
                    {
                        itemSlotNum++;
                        switch (itemSlotNum)
                        {
                            case 4: ItemSlot4.SetActive(true); break;
                            case 5: ItemSlot5.SetActive(true); break;
                            case 6: ItemSlot6.SetActive(true); break;
                            case 7: ItemSlot7.SetActive(true); break;
                        }
                        //�ڐG���Ă���A�C�e��������
                        Destroy(triggerItem);
                        getSE.Play();
                    }
                }

                //���ݑI�𒆂̃X���b�g�ɉ����A�C�e�����Ȃ���
                if (itemSlot[nowSlot] == Item.none)
                {             
                    //�ڐG���Ă���A�C�e�������݂̃X���b�g�ɂԂ�����
                    if (triggerItem.name.Contains("handGun"))
                    {
                        itemSlot[nowSlot] = Item.handGun;
                        nowSlotImg.sprite = ItemSlot_handGun_Img;
                    }
                    else if (triggerItem.name.Contains("knife"))
                    {
                        itemSlot[nowSlot] = Item.knife;
                        nowSlotImg.sprite = ItemSlot_knife_Img;
                    }
                    else if (triggerItem.name.Contains("machineGun"))
                    {
                        itemSlot[nowSlot] = Item.machineGun;
                        nowSlotImg.sprite = ItemSlot_machineGun_Img;
                    }

                    //�ڐG���Ă���A�C�e��������
                    Destroy(triggerItem);
                    getSE.Play();
                }
            }
            else if (itemSlot[nowSlot] != Item.none)//�A�C�e���������Ȃ�A�C�e������o
            {
                //�A�C�e���𐶐�����
                switch (itemSlot[nowSlot])
                {
                    case Item.handGun: 
                        GameObject handGun = Instantiate(handGunPrefab, bulletPoint.transform.position, Quaternion.Euler(0, 0, rb.rotation - 90));break;
                        case Item.machineGun: 
                        GameObject machineGun = Instantiate(machineGunPrefab, bulletPoint.transform.position, Quaternion.Euler(0, 0, rb.rotation - 90));break;
                        case Item.knife: 
                        GameObject knife = Instantiate(knifePrefab, bulletPoint.transform.position, Quaternion.Euler(0, 0, rb.rotation)); break;
                }
                //���ݑI�𒆂̃X���b�g����ɂ���
                itemSlot[nowSlot] = Item.none;
                nowSlotImg.sprite = ItemSlot_none_Img;
                dropSE.Play();
            }
        }
        //Item itemName, float Speed, float Intervaltime, float BulletSpeed, float BulletLostTime, GameObject Item_bulletPrefab,GameObject hand_in_item,bool equal_gun
        //���ݑI�𒆂̃X���b�g�ɉ���������(else����������̂ł��ׂĎ��s����j
        SlotProcess(Item.knife,5f,0.5f,1000,1f,handGun_bulletPrefab,hand_in_knife,false);
        SlotProcess(Item.machineGun, 4f, 0.1f, 1000,1f, handGun_bulletPrefab, hand_in_machineGun,true);
        SlotProcess(Item.handGun, 4.5f, 0.5f,1000,1f, handGun_bulletPrefab, hand_in_handGun,true);
        SlotProcess(Item.none, 5f, 0.5f, 1000,1f, emptyPrefab, hand_in_empty,false);

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

            if (!interval && isFirePressed)//�C���^�[�o������Ȃ����Ȃ甭��
            {
            if (Item_equal_gun)//�A�C�e�����e�̏ꍇ
            {
                Debug.Log(bulletPrefab.name);
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
            else if (itemSlot[nowSlot] == Item.knife)//�i�C�t�̏ꍇ
            {
                interval = true;
                knifeSE.Play();
                    hand_in_knife.transform.DOLocalMove(new Vector3(0.53f, 0.7f, 0f), 0.2f);
                    hand_in_knife.transform.DOLocalRotate(new Vector3(0, 0, -50), 0.2f).OnComplete(() =>
                    {
                        hand_in_knife.transform.DOLocalMove(new Vector3(0.53f, -0.8f, 0f), 0.2f);
                        hand_in_knife.transform.DOLocalRotate(new Vector3(0, 0, -137.63f), 0.2f);
                    });
            }
            
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
            triggerItem = collision.gameObject;
            //���ݑI�𒆂̃X���b�g�ɉ����A�C�e�����Ȃ���
            //�������͐ڐG���̃A�C�e�������̎�
            if (itemSlot[nowSlot] == Item.none || triggerItem.name.Contains("bag"))
            {
                //�A�C�e�����E�����b�Z�[�W
                Item_message.SetActive(true);
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