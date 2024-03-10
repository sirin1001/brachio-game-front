using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;

public class PlayerController : MonoBehaviour
{
    //移動速度
    public float speed = 0;

    private Rigidbody2D rb;
    private Transform tf;

    //inputSystem操作用
    private Vector2 movement;
    private float look;

    //操作はGamePadか
    public bool gamePad;

    Vector2 mousePos;
    [SerializeField] Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        // プレイヤーにアタッチされているRigidbodyを取得
        rb = gameObject.GetComponent<Rigidbody2D>();
        tf = gameObject.GetComponent<Transform>();
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

    //ベクトルから角度を求める
    public static float Vector2ToAngle(Vector2 vector)
    {
        return Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
    }

    private void Update()
    {
        // 接続されているコントローラの名前を調べる
        var controllerNames = Input.GetJoystickNames();
        // 一台もコントローラが接続されていなければキーボードマウスモード
        if (controllerNames[0] == "") gamePad = false;
        //接続されていればgamepadモード
        else gamePad = true;

        //視点操作
        if (gamePad)//gamePadの場合
        {
            if (look != 0)
                tf.eulerAngles = new Vector3(0, 0, look);
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

        //移動操作
        rb.velocity = movement * speed;
        

    }

}