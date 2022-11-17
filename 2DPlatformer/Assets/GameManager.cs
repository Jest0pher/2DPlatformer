using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static int MaxPlayerCount = 4;
    public static int currentPlayersCount;
    public static InputDevice[] playerInputs = { null, null, null, null};

    public List<Player> players;
    public PlayerInputManager pInputManager;
    private void Awake()
    {
        Instance = this;
        players = new List<Player>(MaxPlayerCount);
        //playerInputs = new InputDevice[MaxPlayerCount];
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }
}
