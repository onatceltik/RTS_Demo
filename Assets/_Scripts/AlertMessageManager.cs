using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class AlertMessageManager : MonoBehaviour
{
    [SerializeField] GameObject alertMessageBox;
    [SerializeField] TextMeshProUGUI alertMessageText;
    static WaitForSeconds waitMessageForSeconds = new WaitForSeconds(3f);

    public enum AlertType {NoEmptySpaceOnSpawn, InvalidPlacement};

    public static event Action<AlertType> OnAlertReceived;

    void OnEnable()
    {
        OnAlertReceived += handleAlertMessage;
    }

    void OnDisable()
    {
        OnAlertReceived -= handleAlertMessage;
    }

    void Start()
    {
        // Hide the text at the start of the game
        clearAlertMessageBox();
    }

    void enableAlertMessageBox() { alertMessageBox.SetActive(true); }
    void enableAlertMessageBox(string messageText) { alertMessageText.text = messageText; }

    public static void triggerAlert(AlertType _alertType)
    {
        OnAlertReceived?.Invoke(_alertType);
    }

    void handleAlertMessage(AlertType _alertType)
    {
        enableAlertMessageBox();
        switch (_alertType) {
            case AlertType.NoEmptySpaceOnSpawn: handleNoEmptySpaceOnSpawn(); break;
            case AlertType.InvalidPlacement: handleInvalidPlacement(); break;
        }

        StartCoroutine(keepMessageForSeconds());
    }

    IEnumerator keepMessageForSeconds()
    {
        yield return waitMessageForSeconds;
        clearAlertMessageBox();
    }

    void clearAlertMessageBox()
    {
        if (alertMessageBox.activeInHierarchy) alertMessageBox.SetActive(false); 
        if (alertMessageText) alertMessageText.text = string.Empty;
    }

    void handleNoEmptySpaceOnSpawn()
    {
        enableAlertMessageBox("Cannot spawn unit! No available place around building.");
    }

    void handleInvalidPlacement()
    {
        enableAlertMessageBox("Cannot build unit here!");
    }
}
