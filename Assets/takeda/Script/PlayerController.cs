using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class PlayerController : MonoBehaviour
{
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
    // アクション
    private InputAction _aimAction;
    private InputAction _fireAction;

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


    // Start is called before the first frame update
    void Start()
    {
        // プレイヤーにアタッチされているコンポーネントを取得
        rb = gameObject.GetComponent<Rigidbody2D>();
        tf = gameObject.GetComponent<Transform>();
        //アクションをPlayerInputから取得
        _aimAction = _playerInput.actions[_aimActionName];
        _fireAction = _playerInput.actions[_fireActionName];
    }

    private void OnMove(InputValue movementValue)
    {
        // Moveアクションの入力値を取得
        movement = movementValue.Get<Vector2>();
        Debug.Log("moveアクション入力された");
    }
    
    private void OnLook(InputValue LookValue)
    {
        if (gamePad)//gamePadモードなら
        {
            //Lookアクションの入力値を取得
            //Vectorを取得→角度に変換
            look = Vector2ToAngle(LookValue.Get<Vector2>());
            Debug.Log("Lookアクション入力された");
        }
    }

    private void Update()
    {
        // 接続されているコントローラの名前を調べる
        var controllerNames = Input.GetJoystickNames();
        // 一台もコントローラが接続されていなければキーボードマウスモード
        if (controllerNames[0] == "") gamePad = false;
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
            if (!interval&&isFirePressed)
            {
                GameObject bullet = Instantiate(bulletPrefab, bulletPoint.transform.position, Quaternion.identity);
                Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

                // 弾速は自由に設定
                bulletRb.AddForce(AngleToVector2(rb.rotation) * 800);

                // 発射音を出す
                shotSE.Play();

                // 時間差で砲弾を破壊する
                Destroy(bullet, 1.0f);

                interval = true;
            }
            //インターバル処理
            if(interval)
            {
                if(temptime >= intervaltime)
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