using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameMenuManager : MonoBehaviour
{

    [SerializeField] GameObject escPanel;
    [SerializeField] CanvasGroup productionPanel;
    [SerializeField] CanvasGroup informationPanel;
    [SerializeField] UnitPlacementManager unitPlacementManager;

    void Start()
    {
        if (escPanel != null) escPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
           toggleGameMenu();
        }
    }

    public void toggleGameMenu()
    {
        if (escPanel != null && unitPlacementManager.placementLock() == false)
        {
            if (escPanel.activeInHierarchy)
            {
                escPanel.SetActive(false);
                productionPanel.blocksRaycasts = true;
                informationPanel.blocksRaycasts = true;
            }
            else
            {
                escPanel.SetActive(true);
                productionPanel.blocksRaycasts = false;
                informationPanel.blocksRaycasts = false;
            } 
        }
    }

    public void startGameAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void exitGame()
    {
        Application.Quit();
    }
}
