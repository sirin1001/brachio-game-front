using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SetBasicAuth : MonoBehaviour
{
    [SerializeField] InputField InputEmail;
    [SerializeField] InputField InputPassword;
    [SerializeField] InputField InputName;

    private string userEmail = "user@example.com";
    private string userPassword = "yourPassword";
    private string userName = "yourUsername";
    private string titleId = "671D2"; // PlayFab Title ID

    public void OnClick()
    {
        ReadInputField();
        UpdateUserTitleDisplayName();
        InitPlayer();
        AddUsernamePasswordToAccount();
        PlayFabAuthService.Instance.UnlinkSilentAuth();
    }

    void ReadInputField()
    {
        userEmail = InputEmail.text;
        userPassword = InputPassword.text;
        userName = InputName.text;
    }

    private void UpdateUserTitleDisplayName()
    {
        PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = userName
        }, result =>
        {
            Debug.Log("プレイヤー名：" + result.DisplayName);
        }, error => Debug.LogError(error.GenerateErrorReport()));
    }

    private void InitPlayer()
    {
        var request = new UpdateUserDataRequest()
        {
            Data = new Dictionary<string, string>
        {
            { "Rank", "0" }
        }
        };

        PlayFabClientAPI.UpdateUserData(request
            , result =>
            {
                Debug.Log("プレイヤーの初期化完了");
                // ------------------------------
                // 処理成功時のコールバックで表示名の更新
                // ------------------------------
                UpdateUserTitleDisplayName();
            }, error => Debug.LogError(error.GenerateErrorReport()));
    }

    // Add Username and Password to an existing account
    public void AddUsernamePasswordToAccount()
    {
        var request = new AddUsernamePasswordRequest
        {
            Email = userEmail,
            Password = userPassword,
            Username = userName
        };

        PlayFabClientAPI.AddUsernamePassword(request, OnAddUsernamePasswordSuccess, OnError);
    }

    private void OnAddUsernamePasswordSuccess(AddUsernamePasswordResult result)
    {
        Debug.Log("Username and password added successfully!");
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError("Error adding username and password: " + error.GenerateErrorReport());
    }
}
