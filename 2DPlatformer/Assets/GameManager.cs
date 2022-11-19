using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerInfo {
    public InputDevice pInput;
    public ModelAbility model;
    public LightAbility light;
    public HeavyAbility heavy;
    public PassiveAbility passive;

    public PlayerInfo() {
        pInput = null;
    }
    public PlayerInfo(InputDevice device) {
        pInput = device;
    }
}
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static int MaxPlayerCount = (int)ModelAbility.COUNT;
    public static List<PlayerInfo> playerInfo;
    public List<Player> players;
    public PlayerInputManager pInputManager;
    private void Awake()
    {
        Instance = this;
        players = new List<Player>(MaxPlayerCount);
        if (playerInfo == null) {
            playerInfo = new List<PlayerInfo>(MaxPlayerCount);
        }
        //playerInputs = new InputDevice[MaxPlayerCount];
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void PrintPlayerInfo()
    {
        if (playerInfo.Count == 0)
            print("Nothing");

        for (int i = 0; i < playerInfo.Count; i++) {
            print(playerInfo[i].pInput.name + "__" + 
                           playerInfo[i].model.ToString() + "__" + 
                           playerInfo[i].light.ToString() + "__" + 
                           playerInfo[i].heavy.ToString() + "__" + 
                         playerInfo[i].passive.ToString());
        }
    }
}
