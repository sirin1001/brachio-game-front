using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;

public class FireManager : NetworkBehaviour
{
    bool interval;
    [SerializeField]GameObject bulletPrefab;
    [SerializeField] GameObject bulletPoint;

    Rigidbody2D rb;

    [SerializeField] float bulletSpeed;
    [SerializeField] float bulletLostTime;
    float temptime;
    [SerializeField] float intervaltime;

    AudioSource shotSE;
    PhotonPlayerController photonPlayerController;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        photonPlayerController = GetComponent<PhotonPlayerController>();

        shotSE = GameObject.Find("shotSE").GetComponent<AudioSource>();
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void FireRpc(bool IsPressed, bool Item_equal_gun)
    {
        /*�t�@�C�A*/
        // �U���{�^���̉�����Ԏ擾
        bool isFirePressed = IsPressed;

        if (Item_equal_gun)//�A�C�e�����e�̎�
            if (!interval && isFirePressed)//�C���^�[�o������Ȃ����Ȃ甭��
            {
                GameObject bullet = Instantiate(bulletPrefab, bulletPoint.transform.position, Quaternion.identity);
                Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

                // �e���͎��R�ɐݒ�
                bulletRb.AddForce(photonPlayerController.AngleToVector2(rb.rotation) * bulletSpeed);

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
}
