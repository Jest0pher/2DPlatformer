using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class SpawnPlayers : MonoBehaviour
{
    PlayerInputManager pInputManager;
    public List<GameObject> prefabs;
    // Start is called before the first frame update
    void Start()
    {

        pInputManager = GetComponent<PlayerInputManager>();
        //Load Unique Player Data with a passed struct
        for (int i = 0; i < GameManager.playerInfo.Count; i++) {
            pInputManager.playerPrefab = prefabs[(int)GameManager.playerInfo[i].model];
            pInputManager.JoinPlayer(i, -1, null, GameManager.playerInfo[i].pInput);
        }
        Destroy(this);
    }
}
