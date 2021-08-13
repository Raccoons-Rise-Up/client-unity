using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KRU.Networking;

namespace KRU.Game
{
    public class UIMenu : MonoBehaviour
    {
        public GameObject m_MenuCanvas;

        public GameObject m_SectionMainMenu;
        public GameObject m_SectionConnecting;
        public GameObject m_SectionOptions;
        public GameObject m_SectionCredits;

        public Transform m_ClientTransform;

        public Transform m_BtnConnectTransform;
        private Button m_BtnConnect;

        private ENetClient m_ClientScript;

        private bool m_InMainMenu;
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

        /// <summary>
        /// Main Menu 'Connect' button to connect to the ENet-CSharp server.
        /// </summary>
        public void BtnConnect()
        {
            m_InMainMenu = false;
            m_InConnecting = true;
            m_SectionMainMenu.SetActive(false);
            m_SectionConnecting.SetActive(true);
            m_ClientScript.Connect();
        }

        /// <summary>
        /// Main Menu 'Options' button
        /// </summary>
        public void BtnOptions() 
        {
            m_InMainMenu = false;
            m_InOptions = true;
            m_SectionMainMenu.SetActive(false);
            m_SectionOptions.SetActive(true);
        }

        /// <summary>
        /// Main Menu 'Credits' button
        /// </summary>
        public void BtnCredits() 
        {
            m_InMainMenu = false;
            m_InCredits = true;
            m_SectionMainMenu.SetActive(false);
            m_SectionCredits.SetActive(true);
        }

        /// <summary>
        /// Main Menu 'Exit' button
        /// </summary>
        public void BtnExit() 
        {
            Application.Quit();
        }
    }
}
