using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class GameUI : MonoBehaviourPunCallbacks
{
    public TextMeshProUGUI winText;

    public static GameUI instance;

    private void Awake()
    {
        instance = this;
    }

    public void SetWinText(string winnerName)
    {
        winText.gameObject.SetActive(true);
        winText.text = winnerName + " wins!";
    }
}

