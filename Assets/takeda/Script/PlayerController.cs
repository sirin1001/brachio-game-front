using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;

public class PlayerController : MonoBehaviour
{
    //�ړ����x
    public float speed = 0;

    private Rigidbody2D rb;
    private Transform tf;

    //inputSystem����p
    private Vector2 movement;
    private float look;

    //�����GamePad��
    public bool gamePad;

    Vector2 mousePos;
    [SerializeField] Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        // �v���C���[�ɃA�^�b�`����Ă���Rigidbody���擾
        rb = gameObject.GetComponent<Rigidbody2D>();
        tf = gameObject.GetComponent<Transform>();
    }

    private void OnMove(InputValue movementValue)
    {
        // Move�A�N�V�����̓��͒l���擾
        movement = movementValue.Get<Vector2>();
        Debug.Log("move�A�N�V�������͂��ꂽ");
    }
    
    private void OnLook(InputValue LookValue)
    {
        if (gamePad)//gamePad���[�h�Ȃ�
        {
            //Look�A�N�V�����̓��͒l���擾
            //Vector���擾���p�x�ɕϊ�
            look = Vector2ToAngle(LookValue.Get<Vector2>());
            Debug.Log("Look�A�N�V�������͂��ꂽ");
        }
        
    }

    //�x�N�g������p�x�����߂�
    public static float Vector2ToAngle(Vector2 vector)
    {
        return Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
    }

    private void Update()
    {
        // �ڑ�����Ă���R���g���[���̖��O�𒲂ׂ�
        var controllerNames = Input.GetJoystickNames();
        // �����R���g���[�����ڑ�����Ă��Ȃ���΃L�[�{�[�h�}�E�X���[�h
        if (controllerNames[0] == "") gamePad = false;
        //�ڑ�����Ă����gamepad���[�h
        else gamePad = true;

        //���_����
        if (gamePad)//gamePad�̏ꍇ
        {
            if (look != 0)
                tf.eulerAngles = new Vector3(0, 0, look);
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

        //�ړ�����
        rb.velocity = movement * speed;
        

    }

}