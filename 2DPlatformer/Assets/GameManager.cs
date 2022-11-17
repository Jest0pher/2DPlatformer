using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int MaxPlayerCount = 4;
    public int currentPlayersCount;

    public Player[] players;
    private void Awake()
    {
        Instance = this;
        players = new Player[MaxPlayerCount];
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
