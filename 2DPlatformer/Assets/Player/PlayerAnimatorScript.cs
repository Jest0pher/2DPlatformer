using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimatorScript : MonoBehaviour
{
    public Player player;

    public void Attack() {
        player.AttackRaycast();
    }

    public void Death() {
        player.PostDeath();
    }
}
