using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : Weapon
{
    public bool isSwinging;
    public float swingSpeed;
    public float swingReset;
    
    public override void Use() {
        if (!isSwinging) {
            //StartCoroutine(SwingSword());
        }
    }

}
