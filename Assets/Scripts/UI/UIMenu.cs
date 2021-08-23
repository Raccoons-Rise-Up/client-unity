using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KRU.Networking;
using TMPro;

namespace KRU.Game
{
    public class UIMenu : MonoBehaviour
    {
        public GameObject menuCanvas;
        public GameObject gameCanvas;

        public GameObject sectionMainMenu;
        public GameObject sectionGameMenu;
        public GameObject sectionLogin;
        public GameObject sectionOptions;
        public GameObject sectionCredits;

        public Transform clientTransform;
        [HideInInspector] public ENetClient clientScript;

        public Transform btnLoginTransform;
        [HideInInspector] public Button btnLogin;

        public Transform gameTransform;
        [HideInInspector] public KRUGame gameScript;

        private enum MenuSection { 
            MainMenu,
            GameMenu,
            Login,
            Options,
            Credits,
            None
        }

        private MenuSection menuSection;

        private void Start()
        {
            clientScript = clientTransform.GetComponent<ENetClient>();

            btnLogin = btnLoginTransform.GetComponent<Button>();
            gameScript = gameTransform.GetComponent<KRUGame>();

            // Just to make sure in case someone forgot to disable / enable the appropriate ones in the editor while working
            menuSection = MenuSection.MainMenu;
            gameCanvas.SetActive(false);
            menuCanvas.SetActive(true);
            sectionMainMenu.SetActive(true);
            sectionGameMenu.SetActive(false);
            sectionLogin.SetActive(false);
            sectionOptions.SetActive(false);
            sectionCredits.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) 
            {
                if (gameScript.Player.InGame) 
                {
                    if (menuSection == MenuSection.GameMenu)
                    {
                        // Go back to the game
                        menuSection = MenuSection.None;
                        menuCanvas.SetActive(false);
                        sectionGameMenu.SetActive(false);
                    }
                    else 
                    {
                        if (menuSection != MenuSection.Options) // Temporary fix, obviously there is a better solution
                        {
                            menuSection = MenuSection.GameMenu;
                            menuCanvas.SetActive(true);
                            sectionGameMenu.SetActive(true);
                        }
                    }
                }

                if (menuSection == MenuSection.Login) 
                {
                    // Go back to the main menu
                    menuSection = MenuSection.MainMenu;
                    sectionLogin.SetActive(false);
                    sectionMainMenu.SetActive(true);
                }

                if (menuSection == MenuSection.Options) 
                {
                    // Go back to the menu
                    
                    sectionOptions.SetActive(false);

                    if (gameScript.Player.InGame)
                    {
                        menuSection = MenuSection.GameMenu;
                        sectionGameMenu.SetActive(true);
                    }
                    else 
                    {
                        menuSection = MenuSection.MainMenu;
                        sectionMainMenu.SetActive(true);
                    }
                }

                if (menuSection == MenuSection.Credits)
                {
                    // Go back to the main menu
                    menuSection = MenuSection.MainMenu;
                    sectionCredits.SetActive(false);
                    sectionMainMenu.SetActive(true);
                }
            }
        }

        public void LoadTimeoutDisconnectScene() 
        {
            menuSection = MenuSection.Login;
            StopCoroutine(gameScript.GameLoop);
            gameCanvas.SetActive(false);
            menuCanvas.SetActive(true);
            sectionLogin.SetActive(true);
            btnLogin.interactable = true;
        }

        public void FromConnectingToMainScene() 
        {
            menuSection = MenuSection.None;
            btnLogin.interactable = false;
            sectionLogin.SetActive(false);
            menuCanvas.SetActive(false);
            gameCanvas.SetActive(true);
        }

        public void BtnMainMenuLogin()
        {
            menuSection = MenuSection.Login;
            sectionMainMenu.SetActive(false);
            sectionLogin.SetActive(true);
        }

        public void BtnDisconnect() 
        {
            gameCanvas.SetActive(false);
            sectionGameMenu.SetActive(false);
            menuSection = MenuSection.MainMenu;
            sectionMainMenu.SetActive(true);
            gameScript.Player.InGame = false;
            btnLogin.interactable = true;
            clientScript.Disconnect();
        }

        public void BtnOptions() 
        {
            menuSection = MenuSection.Options;

            if (gameScript.Player.InGame) 
            {
                sectionGameMenu.SetActive(false);
            }
            else 
            {
                sectionMainMenu.SetActive(false);
            }

            sectionOptions.SetActive(true);
        }

        public void BtnCredits() 
        {
            menuSection = MenuSection.Credits;
            sectionMainMenu.SetActive(false);
            sectionCredits.SetActive(true);
        }

        public void BtnExit() 
        {
            Application.Quit();
        }
    }
}
