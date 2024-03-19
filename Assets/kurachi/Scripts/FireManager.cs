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
        /*ファイア*/
        // 攻撃ボタンの押下状態取得
        bool isFirePressed = IsPressed;

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
}
