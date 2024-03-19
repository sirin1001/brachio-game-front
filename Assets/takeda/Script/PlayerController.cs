using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;
using System.Collections;
using System.Collections.Generic;
// using static UnityEditor.Timeline.TimelinePlaybackControls;
using System;
using DG.Tweening;
using UnityEngine.UI;
// using static UnityEditor.Progress;

public class PlayerController : MonoBehaviour
{
    //UI
    [SerializeField] GameObject Item_message;
    [SerializeField] GameObject SlotMax_message;
    [SerializeField] GameObject ItemSlot1;
    [SerializeField] GameObject ItemSlot2;
    [SerializeField] GameObject ItemSlot3;
    [SerializeField] GameObject ItemSlot4;
    [SerializeField] GameObject ItemSlot5;
    [SerializeField] GameObject ItemSlot6;
    [SerializeField] GameObject ItemSlot7;
    private GameObject NowItemSlot;
    [SerializeField] Image HP_gauze;

    //画像
    [SerializeField] Sprite Item_message_mouse;
    [SerializeField] Sprite Item_message_gamepad;
    [SerializeField] Sprite ItemSlot_none_Img;
    [SerializeField] Sprite ItemSlot_handGun_Img;
    [SerializeField] Sprite ItemSlot_machineGun_Img;
    [SerializeField] Sprite ItemSlot_knife_Img;
    [SerializeField] Sprite ItemSlot_herbs_Img;
    private SpriteRenderer nowSlotImg;

    //パーティクル
    [SerializeField] ParticleSystem bloodFX;
    [SerializeField] ParticleSystem recoveryFX;

    //移動速度
    [SerializeField] float speed;
    //HP
    private int HP=100;

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
    [SerializeField] GameObject machineGun_bulletPrefab;
    private GameObject bulletPrefab;

    //空のプレハブ
    [SerializeField] GameObject emptyPrefab;

    //プレイヤーが手にもつアイテム（ゲームオブジェクト）
    [SerializeField] GameObject hand_in_knife;
    [SerializeField] GameObject hand_in_handGun;
    [SerializeField] GameObject hand_in_machineGun;
    [SerializeField] GameObject hand_in_empty;
    [SerializeField] GameObject hand_in_herbs;

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
    [SerializeField] AudioSource knifeSE;
    [SerializeField] AudioSource damageSE;
    [SerializeField] AudioSource recoverySE;

    /*アイテム*/
    enum Item
    {
        none,
        handGun,
        machineGun,
        knife,
        herbs,
    }

    //アイテムスロットの数（最初は３）
    [SerializeField]int itemSlotNum = 3;
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
    [SerializeField] GameObject herbsPrefab;



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
        itemSlot = new Item[MaxSlotNum];
        nowSlot = 0;
        //アイテムスロット初期化
        for (int i = 0; i < itemSlotNum; i++)
        {
            itemSlot[i] = Item.none;
        }

        Item_message.SetActive(false);
        SlotMax_message.SetActive(false);
        NowItemSlot = ItemSlot1;
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

    //アイテムスロットの切り替え
    private void OnSelect_Item(InputValue SelectValue)
    {
        //アイテム切り替えボタンの入力値を受け取るVector2
        Vector2 selectV2;
        selectV2 = SelectValue.Get<Vector2>();
        Debug.Log("アイテムセレクト："+selectV2.y);

        //アイテム切り替えボタンの入力値が正または負の値をとるとき
        //かつ現在のスロットが上限、下限にないとき
        if(selectV2.y > 0 && nowSlot < itemSlotNum - 1 || selectV2.y < 0 && nowSlot != 0)
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

    //スロットに応じた処理
    private void SlotProcess(Item itemName, float Speed, float Intervaltime, float BulletSpeed, float BulletLostTime, GameObject Item_bulletPrefab,GameObject hand_in_item,bool equal_gun,Sprite ItemSlot_itemImage)
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
            nowSlotImg.sprite = ItemSlot_itemImage;
        }
        else
        {
            hand_in_item.SetActive(false);
        }
    } 

    private void Update()
    {
        //デバッグ用
        if (Input.GetKeyUp(KeyCode.Space))
        {
            Damage(10);
        }
        if (Input.GetKeyUp(KeyCode.Backspace))
        {
            Recovery(10);
        }

        //現在選択中のスロット画像
        nowSlotImg = NowItemSlot.GetComponent<SpriteRenderer>();

        //アイテム拾うボタンが押された時
        if (_Get_ItemAction.WasPressedThisFrame())
        {
            
            //アイテムに接触していたら
            if (itemTrigger)
            {
                //接触しているアイテムが鞄
                if (triggerItem.name.Contains("bag"))
                {
                    //スロット数が上限に達していない
                    if (itemSlotNum<MaxSlotNum)
                    {
                        //スロットを増やす
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

                //現在選択中のスロットに何もアイテムがない時 かつ　接触アイテムが鞄じゃない
                if (itemSlot[nowSlot] == Item.none&&!triggerItem.name.Contains("bag"))
                {             
                    //接触しているアイテムを現在のスロットにぶち込む
                    if (triggerItem.name.Contains("handGun"))
                        itemSlot[nowSlot] = Item.handGun;
                    else if (triggerItem.name.Contains("knife"))
                        itemSlot[nowSlot] = Item.knife;
                    else if (triggerItem.name.Contains("machineGun"))
                    itemSlot[nowSlot] = Item.machineGun;
                    else if (triggerItem.name.Contains("herbs"))
                    itemSlot[nowSlot] = Item.herbs;

                    //接触しているアイテムを消す
                    Destroy(triggerItem);
                    getSE.Play();
                }
            }
            else if (itemSlot[nowSlot] != Item.none)//アイテム所持中ならアイテムを放出
            {
                //アイテムを生成する
                switch (itemSlot[nowSlot])
                {
                    case Item.handGun: 
                        GameObject handGun = Instantiate(handGunPrefab, bulletPoint.transform.position, Quaternion.Euler(0, 0, rb.rotation - 90));break;
                        case Item.machineGun: 
                        GameObject machineGun = Instantiate(machineGunPrefab, bulletPoint.transform.position, Quaternion.Euler(0, 0, rb.rotation - 90));break;
                        case Item.knife: 
                        GameObject knife = Instantiate(knifePrefab, bulletPoint.transform.position, Quaternion.Euler(0, 0, rb.rotation)); break;
                    case Item.herbs:
                        GameObject herbs = Instantiate(herbsPrefab, bulletPoint.transform.position, Quaternion.Euler(0, 0, rb.rotation-30)); break;
                }
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
        if(!trueCheck)
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

            if (!interval && isFirePressed)//インターバルじゃない時なら発射
            {
            if (Item_equal_gun)//アイテムが銃の場合
            {
                Debug.Log(bulletPrefab.name);
                GameObject bullet = Instantiate(bulletPrefab, bulletPoint.transform.position, Quaternion.Euler(0, 0, rb.rotation-90));
                Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

                // 弾速は自由に設定
                bulletRb.AddForce(AngleToVector2(rb.rotation) * bulletSpeed);

                // 発射音を出す
                shotSE.Play();

                // 時間差で砲弾を破壊する
                Destroy(bullet, bulletLostTime);

                interval = true;
            }
            else if (itemSlot[nowSlot] == Item.knife)//ナイフの場合
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
            else if (itemSlot[nowSlot] == Item.herbs)//ハーブの場合
            {
                //回復してスロットを空に
                itemSlot[nowSlot] = Item.none;
                Recovery(10);
                interval = true;
            }
            
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

        //Item itemName, float Speed, float Intervaltime, float BulletSpeed, float BulletLostTime, GameObject Item_bulletPrefab,GameObject hand_in_item,bool equal_gun,ItemSlot_item_Image
        //現在選択中のスロットに応じた処理(else処理もあるのですべて実行する）
        SlotProcess(Item.knife, 5f, 0.5f, 1000, 1f, emptyPrefab, hand_in_knife, false, ItemSlot_knife_Img);
        SlotProcess(Item.machineGun, 4f, 0.1f, 1000, 1f, machineGun_bulletPrefab, hand_in_machineGun, true, ItemSlot_machineGun_Img);
        SlotProcess(Item.handGun, 4.5f, 0.5f, 1000, 1f, handGun_bulletPrefab, hand_in_handGun, true, ItemSlot_handGun_Img);
        SlotProcess(Item.none, 5f, 0, 0, 0, emptyPrefab, hand_in_empty, false, ItemSlot_none_Img);
        SlotProcess(Item.herbs, 5f, 0.5f, 0, 0, emptyPrefab, hand_in_herbs, false, ItemSlot_herbs_Img);
    }

    private bool itemTrigger = false;

    private void OnTriggerStay2D(Collider2D collision)
    {
        //アイテムと接触中の時
        if (collision.CompareTag("Item"))
        {
            itemTrigger = true;
            triggerItem = collision.gameObject;
            //スロット数が上限に達していて接触中アイテムが鞄
            if (itemSlotNum >= MaxSlotNum&&triggerItem.name.Contains("bag"))
            {
                SlotMax_message.SetActive(true);
            }
                //現在選択中のスロットに何もアイテムがない時
                //もしくは接触中のアイテムが鞄の時
               else if (itemSlot[nowSlot] == Item.none || triggerItem.name.Contains("bag"))
            {
                //アイテムを拾うメッセージ
                Item_message.SetActive(true);
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
            SlotMax_message.SetActive(false);
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

    //ダメージ処理
    public void Damage(int damageValue)
    {
        damageSE.Play();
        HP -= damageValue;//hpを減らす

        var sequence = DOTween.Sequence();
        //fill amountを使うためにhpを0～1の値に変える
        float max1_hpgauze = (float)HP / 100f;

        //hpが0より大きいならその値までhpバーの画像を変化
        if(HP > 0)
        sequence.Append(HP_gauze.DOFillAmount(max1_hpgauze, 0.3f));
        else//0以下なら0にする
            sequence.Append(HP_gauze.DOFillAmount(0, 0.3f));

        gameObject.GetComponent<SpriteRenderer>().DOColor(Color.red, 0.15f).OnComplete(() =>
        {
            gameObject.GetComponent<SpriteRenderer>().DOColor(Color.white, 0.1f);
            bloodFX.Play();
        });
    }
    //回復処理
    public void Recovery(int recoveryValue)
    {
        recoverySE.Play();
        HP += recoveryValue;//hpを増やす

        var sequence = DOTween.Sequence();
        //fill amountを使うためにhpを0～1の値に変える
        float max1_hpgauze = (float)HP / 100f;

            sequence.Append(HP_gauze.DOFillAmount(max1_hpgauze, 0.3f));

        gameObject.GetComponent<SpriteRenderer>().DOColor(Color.green, 0.15f).OnComplete(() =>
        {
            gameObject.GetComponent<SpriteRenderer>().DOColor(Color.white, 0.1f);
            recoveryFX.Play();
        });
    }
}