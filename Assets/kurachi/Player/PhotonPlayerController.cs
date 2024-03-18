using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;

public class PhotonPlayerController : NetworkBehaviour, IPlayerController
{
    IPlayerController photonPlayerController;
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

    //画像
    [SerializeField] Sprite Item_message_mouse;
    [SerializeField] Sprite Item_message_gamepad;
    [SerializeField] Sprite ItemSlot_none_Img;
    [SerializeField] Sprite ItemSlot_handGun_Img;
    [SerializeField] Sprite ItemSlot_machineGun_Img;
    [SerializeField] Sprite ItemSlot_knife_Img;

    //移動速度
    [SerializeField] float speed;

    private Rigidbody2D rb;
    private Transform tf;

    //inputSystem操作用
    private Vector2 movement;
    private float look;

    //操作はGamePadか
    public bool gamePad;

    private Vector2 mousePos;
    [SerializeField] Camera cam;

    //照準
    [SerializeField] GameObject aimImg;
    //すでにaim開始したか
    bool aimstart = false;

    // 入力を受け取るPlayerInput
    [SerializeField] private PlayerInput _playerInput;
    // アクション名
    [SerializeField] private string _aimActionName = "Aim";
    [SerializeField] private string _fireActionName = "Fire";
    [SerializeField] private string _Get_ItemActionName = "Get_Item";
    // アクション
    private InputAction _aimAction;
    private InputAction _fireAction;
    private InputAction _Get_ItemAction;

    //弾丸のプレハブ
    [SerializeField] GameObject handGun_bulletPrefab;
    private GameObject bulletPrefab;

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

    /*SE&BGM*/
    [SerializeField] AudioSource shotSE;
    [SerializeField] AudioSource aimSE;
    [SerializeField] AudioSource dropSE;
    [SerializeField] AudioSource getSE;
    [SerializeField] AudioSource selectSlotSE;

    /*アイテム*/
    enum Item
    {
        none,
        handGun,
        machineGun,
        knife,
        herb,
    }

    //アイテムスロットの数（最初は３）
    [SerializeField] int itemSlotNum = 3;
    //選択中のスロット
    int nowSlot;
    //アイテムスロット
    Item[] itemSlot;
    //スロットの上限数
    private int MaxSlotNum = 7;

    //所持しているアイテムは銃か
    bool Item_equal_gun = false;

    //接触しているアイテム
    private GameObject triggerItem;

    /*アイテムプレハブ*/
    [SerializeField] GameObject handGunPrefab;
    [SerializeField] GameObject knifePrefab;
    [SerializeField] GameObject machineGunPrefab;

    void Start()
    {
        photonPlayerController = this.GetComponent<IPlayerController>();

        // Scene上のオブジェクトを取得
        Item_message = GameObject.Find("Item_message");
        ItemSlot1 = GameObject.Find("Slot1");
        ItemSlot2 = GameObject.Find("Slot2");
        ItemSlot3 = GameObject.Find("Slot3");
        ItemSlot4 = GameObject.Find("Slot4");
        ItemSlot5 = GameObject.Find("Slot5");
        ItemSlot6 = GameObject.Find("Slot6");
        ItemSlot7 = GameObject.Find("Slot7");

        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        GameObject.Find("Main Camera").GetComponent<CameraManager>().SetTargetObject(this.gameObject );

        // プレイヤーにアタッチされているコンポーネントを取得
        rb = gameObject.GetComponent<Rigidbody2D>();
        tf = gameObject.GetComponent<Transform>();
        //アクションをPlayerInputから取得
        _aimAction = _playerInput.actions[_aimActionName];
        _fireAction = _playerInput.actions[_fireActionName];
        _Get_ItemAction = _playerInput.actions[_Get_ItemActionName];

        //アイテムスロット作成
        itemSlot = new Item[MaxSlotNum];
        nowSlot = 0;
        //アイテムスロット初期化
        for (int i = 0; i < itemSlotNum; i++)
        {
            itemSlot[i] = Item.none;
        }

        Item_message.SetActive(false);
        NowItemSlot = ItemSlot1;
    }
    void OnMove(InputValue movementValue)
    {
        photonPlayerController.OnMove(movementValue);
    }
    void IPlayerController.OnMove(InputValue movementValue)
    {
        // Moveアクションの入力値を取得
        movement = movementValue.Get<Vector2>();
        Debug.Log($"[Debug] movement {movement}");
    }
    void OnLook(InputValue LookValue)
    {
        photonPlayerController.OnLook(LookValue);
    }
    void IPlayerController.OnLook(InputValue LookValue)
    {
        if (gamePad)//gamePadモードなら
        {
            //Lookアクションの入力値を取得
            //Vectorを取得→角度に変換
            look = photonPlayerController.Vector2ToAngle(LookValue.Get<Vector2>());
        }
    }
    void OnSelect_Item(InputValue SelectValue)
    {
        photonPlayerController.OnSelect_Item(SelectValue);
    }
    void IPlayerController.OnSelect_Item(InputValue SelectValue)
    {
        //アイテム切り替えボタンの入力値を受け取るVector2
        Vector2 selectV2;
        selectV2 = SelectValue.Get<Vector2>();
        Debug.Log("アイテムセレクト：" + selectV2.y);

        //アイテム切り替えボタンの入力値が正または負の値をとるとき
        //かつ現在のスロットが上限、下限にないとき
        if (selectV2.y > 0 && nowSlot < itemSlotNum - 1 || selectV2.y < 0 && nowSlot != 0)
        {
            //正の値が入力されたら一つ上のアイテムスロットに切り替え
            if (selectV2.y > 0 && nowSlot < itemSlotNum - 1)
            {
                NowItemSlot.transform.localScale = new Vector3(20, 20, 20);
                nowSlot++;
            }
            //負の値が入力されたら一つ下のアイテムスロットに切り替え
            if (selectV2.y < 0 && nowSlot != 0)
            {
                NowItemSlot.transform.localScale = new Vector3(20, 20, 20);
                nowSlot--;
            }
            //変化したnowSlotの値に応じてスロット更新
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

    private void Update()
    {
        //アイテム拾うボタンが押された時
        if (_Get_ItemAction.WasPressedThisFrame())
        {
            SpriteRenderer nowSlotImg = NowItemSlot.GetComponent<SpriteRenderer>();
            //アイテムに接触していたら
            if (itemTrigger)
            {
                //スロット数が上限に達していない
                if (itemSlotNum < MaxSlotNum)
                {
                    //接触しているアイテムが鞄ならアイテムスロットを増やす
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
                        //接触しているアイテムを消す
                        Destroy(triggerItem);
                        getSE.Play();
                    }
                }

                //現在選択中のスロットに何もアイテムがない時
                if (itemSlot[nowSlot] == Item.none)
                {
                    //接触しているアイテムを現在のスロットにぶち込む
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
                    if (triggerItem.name.Contains("machineGun"))
                    {
                        itemSlot[nowSlot] = Item.machineGun;
                        nowSlotImg.sprite = ItemSlot_machineGun_Img;
                    }

                    //接触しているアイテムを消す
                    Destroy(triggerItem);
                    getSE.Play();
                }
            }
            else//アイテム所持中ならアイテムを放出
            {
                //アイテムを生成する
                if (itemSlot[nowSlot] == Item.handGun)
                {
                    GameObject handGun = Instantiate(handGunPrefab, bulletPoint.transform.position, Quaternion.Euler(0, 0, rb.rotation - 90));
                }
                if (itemSlot[nowSlot] == Item.knife)
                {
                    GameObject knife = Instantiate(knifePrefab, bulletPoint.transform.position, Quaternion.Euler(0, 0, rb.rotation));
                }
                if (itemSlot[nowSlot] == Item.machineGun)
                {
                    GameObject machineGun = Instantiate(machineGunPrefab, bulletPoint.transform.position, Quaternion.Euler(0, 0, rb.rotation - 90));
                }
                //現在選択中のスロットを空にする
                itemSlot[nowSlot] = Item.none;
                nowSlotImg.sprite = ItemSlot_none_Img;
                dropSE.Play();
            }
        }

        //現在選択中のスロットに応じた処理
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
        if (itemSlot[nowSlot] == Item.machineGun)
        {
            speed = 4f;

            intervaltime = 0.1f;
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

        //gamePadの場合
        if (gamePad)
        {
            Item_message.gameObject.GetComponent<SpriteRenderer>().sprite = Item_message_gamepad;
        }
        //キーマウスの場合
        else
        {
            Item_message.gameObject.GetComponent<SpriteRenderer>().sprite = Item_message_mouse;
        }

        //gamePadの値がtrueに変えられたか
        bool trueCheck = false;

        // 接続されているコントローラの名前を調べる
        var controllerNames = Input.GetJoystickNames();
        // 一台もコントローラが接続されていなければキーボードマウスモード
        if (controllerNames.Length == 0)//一度も接続された形跡が無ければ
            gamePad = false;
        //接続された形跡がある場合は過去に接続されたコントローラの数だけ調べる
        //一つでも接続されていたらgamePadをtrueにする
        else for (int i = 0; i < controllerNames.Length; i++)
            {
                if (controllerNames[i] != "")
                {
                    gamePad = true;
                    trueCheck = true;
                    break;
                }
            };
        //trueCheckがfalseのまま→コントローラが接続されていない
        if (!trueCheck)
            gamePad = false;

        /*視点操作*/
        if (gamePad)//gamePadの場合
        {
            //スティックを触っていないときはlookの値が0として出力される模様
            //なのでスティックから手を放す度にプレイヤーが0度の向きを向かないように
            //lookの値がゼロの時以外に視点操作を実行しています
            if (look != 0)
                rb.rotation = look;
        }
        else//マウスでの場合
        {
            mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            //マウスの向きを取得
            Vector2 lookDir = mousePos - rb.position;
            //マウスの角度を取得する
            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
            rb.rotation = angle;
        }

        /*移動操作*/
        rb.velocity = movement * speed;

        /*エイム*/
        //エイムボタンの押下状態取得
        bool isAimPressed = _aimAction.IsPressed();

        if (Item_equal_gun)//アイテムが銃の時
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

        /*ファイア*/
        // 攻撃ボタンの押下状態取得
        bool isFirePressed = _fireAction.IsPressed();

        if (Item_equal_gun)//アイテムが銃の時
            if (!interval && isFirePressed)//インターバルじゃない時なら発射
            {
                GameObject bullet = Instantiate(bulletPrefab, bulletPoint.transform.position, Quaternion.identity);
                Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

                // 弾速は自由に設定
                bulletRb.AddForce(photonPlayerController.AngleToVector2(rb.rotation) * bulletSpeed);

                // 発射音を出す
                shotSE.Play();

                // 時間差で砲弾を破壊する
                Destroy(bullet, bulletLostTime);

                interval = true;
            }
        //インターバル処理
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
    void OnTriggerStay2D(Collider2D collision)
    {
        photonPlayerController.OnTriggerStay2D(collision);
    }
    void IPlayerController.OnTriggerStay2D(Collider2D collision)
    {
        //アイテムと接触中の時
        if (collision.CompareTag("Item"))
        {
            itemTrigger = true;
            triggerItem = collision.gameObject;
            //現在選択中のスロットに何もアイテムがない時
            //もしくは接触中のアイテムが鞄の時
            if (itemSlot[nowSlot] == Item.none || triggerItem.name.Contains("bag"))
            {
                //アイテムを拾うメッセージ
                Item_message.SetActive(true);
            }
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        photonPlayerController.OnTriggerExit2D(other);
    }
    void IPlayerController.OnTriggerExit2D(Collider2D other)
    {
        //離れたオブジェクトのタグが"Item"のとき
        if (other.CompareTag("Item"))
        {
            Item_message.SetActive(false);
            itemTrigger = false;
        }
    }
    float IPlayerController.Vector2ToAngle(Vector2 vector)
    {
        return Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
    }
    Vector2 IPlayerController.AngleToVector2(float angle)
    {
        var radian = angle * (Mathf.PI / 180);
        return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian)).normalized;
    }
}
