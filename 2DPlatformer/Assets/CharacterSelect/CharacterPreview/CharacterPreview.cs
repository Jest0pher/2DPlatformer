using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPreview : MonoBehaviour
{
    public List<GameObject> previews;

    public AbilitySelector abilitySelector;

    ModelAbility prevModelAbility;

    private void Start()
    {
        foreach (GameObject gm in previews) {
            gm.SetActive(false);
        }
        previews[(int)abilitySelector.Model].SetActive(true);
        prevModelAbility = abilitySelector.Model;
    }

    public void ChangeCharacter(int index) {
        previews[(int)prevModelAbility].SetActive(false);
        previews[index].SetActive(true);
        prevModelAbility = (ModelAbility)index;
    }
}
