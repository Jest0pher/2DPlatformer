using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class SpawnPlayers : MonoBehaviour
{
    PlayerInputManager pInputManager;
    [SerializeField] GameObject warriorPrefab;
    [SerializeField] GameObject knightPrefab;
    // Start is called before the first frame update
    void Start()
    {

        pInputManager = GetComponent<PlayerInputManager>();
        //Load Unique Player Data with a passed struct
        for (int i = 0; i < GameManager.currentPlayersCount; i++) {
            pInputManager.playerPrefab = i == 0 ? warriorPrefab : knightPrefab;
            pInputManager.JoinPlayer(i, -1, null, GameManager.playerInputs[i]);
        }
        Destroy(this);
    }
}
