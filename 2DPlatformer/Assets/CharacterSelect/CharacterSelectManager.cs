using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterSelectManager : MonoBehaviour
{
    public static CharacterSelectManager Instance;
    [SerializeField] GameObject P1Obj;
    [SerializeField] GameObject P2Obj;
    [SerializeField] GameObject P3Obj;
    [SerializeField] GameObject P4Obj;
    [SerializeField] GameObject button;

    List<CharacterSelector> selectors;

    private void Start()
    {
        selectors = new List<CharacterSelector>();
    }
    public void PlayerConnect(CharacterSelector player, InputDevice device) {
        switch (GameManager.playerInfo.Count) {
            case 0:
                P1Obj.SetActive(false);
                player.transform.position = P1Obj.transform.position;
                button.SetActive(true);
                break;
            case 1:
                P2Obj.SetActive(false);
                player.transform.position = P2Obj.transform.position;
                break;
            case 2:
                P3Obj.SetActive(false);
                player.transform.position = P3Obj.transform.position;
                break;
            case 3:
                P4Obj.SetActive(false);
                player.transform.position = P4Obj.transform.position;
                break;
            default:
                break;
        }
        player.playerID = selectors.Count;
        selectors.Add(player);
        GameManager.playerInfo.Add(new PlayerInfo(device));
    }

    public void PlayerDisconnect(CharacterSelector player) {
        GameManager.playerInfo.RemoveAt(player.playerID);
        selectors.RemoveAt(player.playerID);
        for (int i = 0; i < selectors.Count; i++) {
            selectors[i].playerID = i;
            switch (i)
            {
                case 0:
                    selectors[i].transform.position = P1Obj.transform.position;
                    break;
                case 1:
                    selectors[i].transform.position = P2Obj.transform.position;
                    break;
                case 2:
                    selectors[i].transform.position = P3Obj.transform.position;
                    break;
                case 3:
                    selectors[i].transform.position = P4Obj.transform.position;
                    break;
                default:
                    break;
            }
        }
        switch (GameManager.playerInfo.Count)
        {
            case 0:
                P1Obj.SetActive(true);
                button.SetActive(false);
                break;
            case 1:
                P2Obj.SetActive(true);
                break;
            case 2:
                P3Obj.SetActive(true);
                break;
            case 3:
                P4Obj.SetActive(true);
                break;
            default:
                break;
        }
    }
    private void Awake()
    {
        Instance = this;
    }

}
