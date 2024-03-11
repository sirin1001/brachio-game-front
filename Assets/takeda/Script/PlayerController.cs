using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class PlayerController : MonoBehaviour
{
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
    // �A�N�V����
    private InputAction _aimAction;
    private InputAction _fireAction;

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


    // Start is called before the first frame update
    void Start()
    {
        // �v���C���[�ɃA�^�b�`����Ă���R���|�[�l���g���擾
        rb = gameObject.GetComponent<Rigidbody2D>();
        tf = gameObject.GetComponent<Transform>();
        //�A�N�V������PlayerInput����擾
        _aimAction = _playerInput.actions[_aimActionName];
        _fireAction = _playerInput.actions[_fireActionName];
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

    private void Update()
    {
        // �ڑ�����Ă���R���g���[���̖��O�𒲂ׂ�
        var controllerNames = Input.GetJoystickNames();
        // �����R���g���[�����ڑ�����Ă��Ȃ���΃L�[�{�[�h�}�E�X���[�h
        if (controllerNames[0] == "") gamePad = false;
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
            if (!interval&&isFirePressed)
            {
                GameObject bullet = Instantiate(bulletPrefab, bulletPoint.transform.position, Quaternion.identity);
                Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

                // �e���͎��R�ɐݒ�
                bulletRb.AddForce(AngleToVector2(rb.rotation) * 800);

                // ���ˉ����o��
                shotSE.Play();

                // ���ԍ��ŖC�e��j�󂷂�
                Destroy(bullet, 1.0f);

                interval = true;
            }
            //�C���^�[�o������
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