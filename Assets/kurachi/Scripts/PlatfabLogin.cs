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
            GetUserAccountInfo = true // �ݒ肵��Username������ݒ�ɂ��Ă���
        };

        // �ݒ肵�����[���ƃp�X���[�h�Ń��O�C�������݂�
        var request = new LoginWithEmailAddressRequest { Email = InputEmail.text, Password = InputPassword.text, TitleId = TitleId };
        request.InfoRequestParameters = InfoRequestParams;

        PlayFabClientAPI.LoginWithEmailAddress(request, result => {
            // ���O�C�����������烍�O���o��
            Debug.Log("WelcomeBack:" + result.InfoResultPayload.AccountInfo.Username);
        }, error => { });
    }
}
