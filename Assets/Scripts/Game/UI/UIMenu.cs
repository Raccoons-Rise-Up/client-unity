using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using KRU.Networking;

namespace KRU.Game
{
    public class UIMenu : MonoBehaviour
    {
        public GameObject m_MenuCanvas;

        public GameObject m_SectionMainMenu;
        public GameObject m_SectionLogin;
        public GameObject m_SectionConnecting;
        public GameObject m_SectionOptions;
        public GameObject m_SectionCredits;

        public Transform m_ClientTransform;

        public Transform m_BtnConnectTransform;
        private Button m_BtnConnect;

        private ENetClient m_ClientScript;

        private bool m_InMainMenu;
        private bool m_InLogin;
        private bool m_InConnecting;
        private bool m_InOptions;
        private bool m_InCredits;

        private void Start()
        {
            m_ClientScript = m_ClientTransform.GetComponent<ENetClient>();
            m_BtnConnect = m_BtnConnectTransform.GetComponent<Button>();

            // Just to make sure in case someone forgot to disable / enable the appropriate ones in the editor while working
            m_InMainMenu = true;
            m_MenuCanvas.SetActive(true);
            m_SectionMainMenu.SetActive(true);
            m_SectionLogin.SetActive(false);
            m_SectionConnecting.SetActive(false);
            m_SectionOptions.SetActive(false);
            m_SectionCredits.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) 
            {
                if (m_InMainMenu)
                {
                    // If not connected to the main server do nothing

                    // If connected to the main server then escape to the game
                    if (m_ClientScript.IsConnected()) 
                    {
                        m_InMainMenu = false;
                        m_MenuCanvas.SetActive(false);
                        m_SectionMainMenu.SetActive(false);
                    }
                }
                else 
                {
                    m_InMainMenu = true;
                    m_MenuCanvas.SetActive(true);
                    m_SectionMainMenu.SetActive(true);
                }

                if (m_InLogin) 
                {
                    // Go back to the main menu
                    m_InLogin = false;
                    m_SectionLogin.SetActive(false);
                    m_SectionMainMenu.SetActive(true);
                }

                if (m_InConnecting) 
                {
                    // Cancel connection attempt to server
                    m_ClientScript.EnqueueENetInstruction(ENetInstruction.CancelConnection);
                    m_InConnecting = false;
                    m_SectionConnecting.SetActive(false);
                    m_SectionMainMenu.SetActive(true);
                }

                if (m_InOptions) 
                {
                    // Go back to the main menu
                    m_InOptions = false;
                    m_SectionOptions.SetActive(false);
                    m_SectionMainMenu.SetActive(true);
                }

                if (m_InCredits)
                {
                    // Go back to the main menu
                    m_InCredits = false;
                    m_SectionCredits.SetActive(false);
                    m_SectionMainMenu.SetActive(true);
                }
            }
        }

        public void FromConnectingToMainMenu() 
        {
            m_InConnecting = false;
            m_InMainMenu = true;
            m_MenuCanvas.SetActive(true);
            m_SectionConnecting.SetActive(false);
            m_SectionMainMenu.SetActive(true);
            m_BtnConnect.interactable = true;
        }

        public void FromConnectingToMainScene() 
        {
            m_InConnecting = false;
            m_InMainMenu = false;
            m_MenuCanvas.SetActive(false);
            m_SectionConnecting.SetActive(false);
            m_BtnConnect.interactable = false;
        }

        IEnumerator GetRequest(string url)
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
                    Debug.Log(test.message);

                    break;
            }
        }

        public void BtnMainMenuLogin()
        {
            m_InMainMenu = false;
            m_InLogin = true;
            m_SectionMainMenu.SetActive(false);
            m_SectionLogin.SetActive(true);
        }

        public void BtnLogin() 
        {
            StartCoroutine(GetRequest("localhost:4000/api"));

            //m_ClientScript.Connect();
        }

        public void BtnOptions() 
        {
            m_InMainMenu = false;
            m_InOptions = true;
            m_SectionMainMenu.SetActive(false);
            m_SectionOptions.SetActive(true);
        }

        public void BtnCredits() 
        {
            m_InMainMenu = false;
            m_InCredits = true;
            m_SectionMainMenu.SetActive(false);
            m_SectionCredits.SetActive(true);
        }

        public void BtnExit() 
        {
            Application.Quit();
        }
    }
}
