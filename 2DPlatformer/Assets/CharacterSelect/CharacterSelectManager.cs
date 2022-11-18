using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectManager : MonoBehaviour
{
    public static CharacterSelectManager Instance;
    [SerializeField] GameObject P1Obj;
    [SerializeField] GameObject P2Obj;
    [SerializeField] GameObject P3Obj;
    [SerializeField] GameObject P4Obj;
    [SerializeField] GameObject button;

    public void PlayerConnect(GameObject player) {
        switch (GameManager.currentPlayersCount) {
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
        GameManager.currentPlayersCount++;
    }
    private void Awake()
    {
        Instance = this;
    }

}
