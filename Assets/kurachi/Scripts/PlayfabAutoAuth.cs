using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using LoginResult = PlayFab.ClientModels.LoginResult;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PlayFab.CloudScriptModels;
using PlayFab.Internal;

public class PlayfabAutoAuth : MonoBehaviour
{

    void Start()
    {
        PlayFabAuthService.OnLoginSuccess += PlayFabAuthService_OnLoginSuccess;
        PlayFabAuthService.Instance.Authenticate(Authtypes.Silent);
    }

    void OnEnable()
    {
        PlayFabAuthService.OnLoginSuccess += PlayFabLogin_OnLoginSuccess;
    }
    private void PlayFabLogin_OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Login Success!");
    }
    private void OnDisable()
    {
        PlayFabAuthService.OnLoginSuccess -= PlayFabLogin_OnLoginSuccess;
    }

    private void PlayFabAuthService_OnLoginSuccess(LoginResult success)
    {
        // 新規作成したかどうか
        if (success.NewlyCreated)
        {
            Debug.Log("New");
            SceneManager.LoadScene("InputNameScene");
        }
        else
        {
            Debug.Log("NotNew");
            SceneManager.LoadScene("InputNameScene");
        }

    }

    private void CheckBasicLogin()
    {
        string PlayFabId = "";
        PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest()
        {
            PlayFabId = PlayFabId, // ユーザーのPlayFab ID
            ProfileConstraints = new PlayerProfileViewConstraints()
            {
                ShowLinkedAccounts = true // LinkedAccounts情報を含める
            }
        }, result => {
            var linkedAccounts = result.PlayerProfile.LinkedAccounts;
            foreach (var account in linkedAccounts)
            {
                if (account.Platform == PlayFab.ClientModels.LoginIdentityProvider.PlayFab)
                {
                    Debug.Log("User is authenticated with PlayFab using email and password.");
                }
            }
        }, error => {
            Debug.LogError(error.GenerateErrorReport());
        });
    }
}
