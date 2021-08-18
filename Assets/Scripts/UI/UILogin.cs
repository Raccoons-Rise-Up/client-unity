using TMPro;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace KRU.Game 
{
    public class UILogin : MonoBehaviour
    {
        public Transform menuTransform;
        private UIMenu UIMenuScript;

        public Transform btnConnectTransform;
        [HideInInspector] public Button btnConnect;

        public Transform webServerResponseTextTransform;
        [HideInInspector] public TextMeshProUGUI webServerResponseText;

        public Transform inputUsernameTransform;
        private TMP_InputField inputUsername;

        public Transform inputPasswordTransform;
        private TMP_InputField inputPassword;

        private enum LoginOpcode
        {
            LOGIN_SUCCESS,
            INVALID_USERNAME,
            INVALID_PASSWORD,
            ACCOUNT_DOES_NOT_EXIST,
            PASSWORDS_DO_NOT_MATCH
        }

        private void Start()
        {
            UIMenuScript = menuTransform.GetComponent<UIMenu>();
            btnConnect = btnConnectTransform.GetComponent<Button>();
            webServerResponseText = webServerResponseTextTransform.GetComponent<TextMeshProUGUI>();
            webServerResponseText.text = "";
            inputUsername = inputUsernameTransform.GetComponent<TMP_InputField>();
            inputPassword = inputPasswordTransform.GetComponent<TMP_InputField>();
        }

        public void BtnConnect()
        {
            var username = inputUsername.text;
            var password = inputPassword.text;

            if (username == "") 
            {
                webServerResponseText.text = "Please enter a username";
                return;
            }

            if (password == "") 
            {
                webServerResponseText.text = "Please enter a password";
                return;
            }

            webServerResponseText.text = "Connecting to web server...";

            if (!btnConnect.interactable) // Just for readability
                return;

            btnConnect.interactable = false;

            var webAcc = new WebAccount
            {
                username = inputUsername.text,
                password = inputPassword.text
            };

            StartCoroutine(PostRequest("localhost:4000/api/login", webAcc.ToJsonString(), WebLoginResponse));
        }

        private IEnumerator GetRequest(string url)
        {
            using UnityWebRequest webRequest = UnityWebRequest.Get(url);
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = url.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);

                    var test = JsonUtility.FromJson<WebAccount>(webRequest.downloadHandler.text);
                    //Debug.Log(test.message);

                    break;
            }
        }

        private void WebLoginResponse(UnityWebRequest request)
        {
            var response = JsonUtility.FromJson<WebLoginResponse>(request.downloadHandler.text);

            switch ((LoginOpcode)response.opcode)
            {
                case LoginOpcode.ACCOUNT_DOES_NOT_EXIST:
                    webServerResponseText.text = "Account does not exist";
                    btnConnect.interactable = true;
                    break;
                case LoginOpcode.INVALID_PASSWORD:
                    webServerResponseText.text = "Invalid password";
                    btnConnect.interactable = true;
                    break;
                case LoginOpcode.INVALID_USERNAME:
                    webServerResponseText.text = "Invalid username";
                    btnConnect.interactable = true;
                    break;
                case LoginOpcode.PASSWORDS_DO_NOT_MATCH:
                    webServerResponseText.text = "Passwords do not match";
                    btnConnect.interactable = true;
                    break;
                case LoginOpcode.LOGIN_SUCCESS:
                    webServerResponseText.text = "Connecting to game server...";
                    UIMenuScript.clientScript.Connect();
                    break;
            }

            request.Dispose();
        }

        private IEnumerator PostRequest(string url, string postData, Action<UnityWebRequest> response)
        {
            // UnityWebRequest code copied from Sir-Gatlin https://forum.unity.com/threads/posting-json-through-unitywebrequest.476254/

            var request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(postData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            switch (request.result) 
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ProtocolError:
                    webServerResponseText.text = request.error;
                    Debug.Log(request.error);
                    request.Dispose();
                    btnConnect.interactable = true;
                    break;
                case UnityWebRequest.Result.Success:
                    response.Invoke(request);
                    break;
            }
        }
    }
}
