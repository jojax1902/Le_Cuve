using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Stats")]
    public bool gameEnded = false;
    public int spawnOut = 0;

    public Material[] library = new Material[4];

    [Header("Players")]
    public string playerPrefabLocation; 
    public Transform[] spawnPoints;     
    public PlayerController[] players;  
    private int playersInGame;          

    // instance
    public static GameManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        players = new PlayerController[PhotonNetwork.PlayerList.Length];
        photonView.RPC("ImInGame", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void ImInGame()
    {
        playersInGame++;
        if (playersInGame == PhotonNetwork.PlayerList.Length)
        {
            SpawnPlayer();
        }
           
    }

    void SpawnPlayer()
    {
        GameObject playerObj = PhotonNetwork.Instantiate(playerPrefabLocation, spawnPoints[Random.Range(0, spawnPoints.Length)].position, Quaternion.Euler(0,Random.Range(0, 359), 0));
        PlayerController playerScript = playerObj.GetComponent<PlayerController>();
        print(playerScript.id);
        playerObj.GetComponentInChildren<MeshRenderer>().material = library[playerScript.id];

        playerScript.photonView.RPC("Initialize", RpcTarget.All, PhotonNetwork.LocalPlayer);
    }

    public PlayerController GetPlayer(int playerId)
    {
        return players.First(x => x.id == playerId);
    }

    public PlayerController GetPlayer(GameObject playerObject)
    {
        return players.First(x => x.gameObject == playerObject);
    }

    public void CheckLiving()
    {
        int living = playersInGame;
        print(living);
        foreach (PlayerController p in players)
        {
            if (p.alive == false)
            {
                living--;
            }
        }
        if (living < 2)
        {
            foreach (PlayerController p in players)
            {
                if (p.alive == true)
                {
                    photonView.RPC("WinGame", RpcTarget.All, p.id);
                }
            }
        }

    }


    [PunRPC]
    void WinGame(int playerId)
    {
        gameEnded = true;
        PlayerController player = GetPlayer(playerId);

        GameUI.instance.SetWinText(player.photonPlayer.NickName);

        Invoke("GoBackToMenu", 3.0f);
    }

    void GoBackToMenu()
    {
        PhotonNetwork.LeaveRoom();
        NetworkManager.instance.ChangeScene("Menu");
    }
}