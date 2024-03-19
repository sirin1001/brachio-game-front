using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlatfabLogin : MonoBehaviour
{
    [SerializeField] InputField InputEmail;
    [SerializeField] InputField InputPassword;

    private string password = "password";
    private string email = "aiueo@email.com";
    private string TitleId = "671D2";

    public void OnClick()
    {
        Login(InputEmail.text, InputEmail.text);
    }
    public void Login(string UserEmail,string UserPassword)
    {
        Debug.Log("[Debug]Login");
        GetPlayerCombinedInfoRequestParams InfoRequestParams = new GetPlayerCombinedInfoRequestParams
        {
            GetUserAccountInfo = true // 設定したUsernameを取れる設定にしておく
        };

        // 設定したメールとパスワードでログインを試みる
        var request = new LoginWithEmailAddressRequest { Email = InputEmail.text, Password = InputPassword.text, TitleId = TitleId };
        request.InfoRequestParameters = InfoRequestParams;

        PlayFabClientAPI.LoginWithEmailAddress(request, result => {
            // ログイン成功したらログを出す
            Debug.Log("WelcomeBack:" + result.InfoResultPayload.AccountInfo.Username);
        }, error => { });
    }
}
