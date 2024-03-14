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

    //画像
    [SerializeField] Sprite Item_message_mouse;
    [SerializeField] Sprite Item_message_gamepad;

    //移動速度
    [SerializeField] float speed = 0;

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
    [SerializeField] GameObject bulletPrefab;
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

    /*アイテム*/
    enum Item
    {
        none,
        handGun,
        knife,
        herb,

    }

    //アイテムスロットの数
    int itemSlotNum = 3;
    //選択中のスロット
    int nowSlot;
    //アイテムスロット
    Item[] itemSlot;

    //接触しているアイテム
    private GameObject triggerItem;

    /*アイテムプレハブ*/
    [SerializeField] GameObject handGunPrefab;
    [SerializeField] GameObject knifePrefab;


    // Start is called before the first frame update
    void Start()
    {
        // プレイヤーにアタッチされているコンポーネントを取得
        rb = gameObject.GetComponent<Rigidbody2D>();
        tf = gameObject.GetComponent<Transform>();
        //アクションをPlayerInputから取得
        _aimAction = _playerInput.actions[_aimActionName];
        _fireAction = _playerInput.actions[_fireActionName];
        _Get_ItemAction = _playerInput.actions[_Get_ItemActionName];

        //アイテムスロット作成
        itemSlot = new Item[itemSlotNum];
        //Array.Resize(ref itemSlot, 4);←こんな感じでスロットの容量変えられる
        nowSlot = 0;
        //アイテムスロット初期化
        for (int i = 0; i < itemSlotNum; i++)
        {
            itemSlot[i] = Item.none;
        }

        Item_message.SetActive(false);
    }

    private void OnMove(InputValue movementValue)
    {
        // Moveアクションの入力値を取得
        movement = movementValue.Get<Vector2>();
    }

    private void OnLook(InputValue LookValue)
    {
        if (gamePad)//gamePadモードなら
        {
            //Lookアクションの入力値を取得
            //Vectorを取得→角度に変換
            look = Vector2ToAngle(LookValue.Get<Vector2>());
        }
    }

    private void Update()
    {

        //アイテム拾うボタンが押された時
        if (_Get_ItemAction.WasPressedThisFrame())
        {
            //現在選択中のスロットに何もアイテムがない時
            if (itemSlot[nowSlot] == Item.none)
            {
                //アイテムに接触していたら
                if (itemTrigger)
                {
                    //接触しているアイテムを現在のスロットにぶち込む
                    if (triggerItem.name.Contains("handGun"))
                    {
                        itemSlot[nowSlot] = Item.handGun;
                    }
                    if (triggerItem.name.Contains("knife"))
                    {
                        itemSlot[nowSlot] = Item.knife;
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
                    GameObject handGun = Instantiate(handGunPrefab, bulletPoint.transform.position, Quaternion.Euler(0, 0, rb.rotation-90));
                }
                if (itemSlot[nowSlot] == Item.knife)
                {
                    GameObject handGun = Instantiate(knifePrefab, bulletPoint.transform.position, Quaternion.Euler(0,0, rb.rotation));
                }
                if (itemSlot[nowSlot] != Item.none)
                    //現在選択中のスロットを空にする
                    itemSlot[nowSlot] = Item.none;
                    dropSE.Play();
            }
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


        // 接続されているコントローラの名前を調べる
        var controllerNames = Input.GetJoystickNames();
        // 一台もコントローラが接続されていなければキーボードマウスモード
        if (controllerNames.Length == 0) gamePad = false;
        else if (controllerNames[0] == "") gamePad = false;
        //接続されていればgamepadモード
        else gamePad = true;

        /*視点操作*/
        if (gamePad)//gamePadの場合
        {
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
        // ボタンの押下状態をログ出力
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

        /*ファイア*/
        // 攻撃ボタンの押下状態取得
        bool isFirePressed = _fireAction.IsPressed();
        //インターバルじゃない時なら発射
        if (!interval && isFirePressed)
        {
            GameObject bullet = Instantiate(bulletPrefab, bulletPoint.transform.position, Quaternion.identity);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

            // 弾速は自由に設定
            bulletRb.AddForce(AngleToVector2(rb.rotation) * 1000);

            // 発射音を出す
            shotSE.Play();

            // 時間差で砲弾を破壊する
            Destroy(bullet, 1.0f);

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

    private void OnTriggerStay2D(Collider2D collision)
    {
        //アイテムと接触中の時
        if (collision.CompareTag("Item"))
        {
            Debug.Log("アイテムと接触中");
            itemTrigger = true;
            //現在選択中のスロットに何もアイテムがない時
            if (itemSlot[nowSlot] == Item.none)
            {
                //アイテムを拾うメッセージ
                Item_message.SetActive(true);
                triggerItem = collision.gameObject;
            }
        }
    }
        //アイテムから離れたとき
        void OnTriggerExit2D(Collider2D other)
        {
            //離れたオブジェクトのタグが"Item"のとき
            if (other.CompareTag("Item"))
            {
                Item_message.SetActive(false);
                itemTrigger = false;
            }
        }
        //ベクトルから角度を求める
        public static float Vector2ToAngle(Vector2 vector)
        {
            return Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
        }
        //角度からベクトルを求める
        public static Vector2 AngleToVector2(float angle)
        {
            var radian = angle * (Mathf.PI / 180);
            return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian)).normalized;
        }
}