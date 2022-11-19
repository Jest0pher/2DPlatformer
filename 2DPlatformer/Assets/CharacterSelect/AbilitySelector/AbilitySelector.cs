using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum AbilityType { 
    Model,
    Light,
    Heavy,
    Passive
}

public enum ModelAbility { 
    Warrior,
    Knight,
    COUNT
}

public enum LightAbility { 
    Light,
    Light2,
    COUNT
}

public enum HeavyAbility { 
    Heavy,
    Heavy2,
    COUNT
}

public enum PassiveAbility { 
    Passive,
    Passive2,
    COUNT
}

public class AbilitySelector : Selector
{
    public TextMeshPro text;
    public SpriteRenderer leftArrow;
    public SpriteRenderer rightArrow;

    [SerializeField] private AbilityType abilityType;
    [SerializeField] private ModelAbility modelAbility;
    [SerializeField] private LightAbility lightAbility;
    [SerializeField] private HeavyAbility heavyAbility;
    [SerializeField] private PassiveAbility passiveAbility;
    public CharacterPreview characterPreview;
    public ModelAbility Model { get { return modelAbility; } set { modelAbility = (ModelAbility)(((int)value + (int)ModelAbility.COUNT) % (int)ModelAbility.COUNT); } }
    public LightAbility Light { get { return lightAbility; } set { lightAbility = (LightAbility)(((int)value + (int)LightAbility.COUNT) % (int)LightAbility.COUNT); } }
    public HeavyAbility Heavy { get { return heavyAbility; } set { heavyAbility = (HeavyAbility)(((int)value + (int)HeavyAbility.COUNT) % (int)HeavyAbility.COUNT); } }
    public PassiveAbility Passive { get { return passiveAbility; } set { passiveAbility = (PassiveAbility)(((int)value + (int)PassiveAbility.COUNT) % (int)PassiveAbility.COUNT); } }
    private void Start()
    {
        switch (abilityType)
        {
            case AbilityType.Model:
                if (GameManager.playerInfo.Count > 1)
                {
                    SetNextModel(true);
                }
                GameManager.playerInfo[characterSelector.playerID].model = Model;
                text.text = Model.ToString();
                break;
            case AbilityType.Light:
                GameManager.playerInfo[characterSelector.playerID].light = Light;
                text.text = Light.ToString();
                break;
            case AbilityType.Heavy:
                GameManager.playerInfo[characterSelector.playerID].heavy = Heavy;
                text.text = Heavy.ToString();
                break;
            case AbilityType.Passive:
                GameManager.playerInfo[characterSelector.playerID].passive = Passive;
                text.text = Passive.ToString();
                break;
            default:
                break;
        }
    }
    public void CycleLeft() {
        switch (abilityType)
        {
            case AbilityType.Model:
                SetNextModel(false);
                GameManager.playerInfo[characterSelector.playerID].model = Model;
                text.text = Model.ToString();
                break;
            case AbilityType.Light:
                Light--;
                GameManager.playerInfo[characterSelector.playerID].light = Light;
                text.text = Light.ToString();
                break;
            case AbilityType.Heavy:
                Heavy--;
                GameManager.playerInfo[characterSelector.playerID].heavy = Heavy;
                text.text = Heavy.ToString();
                break;
            case AbilityType.Passive:
                Passive--;
                GameManager.playerInfo[characterSelector.playerID].passive = Passive;
                text.text = Passive.ToString();
                break;
            default:
                break;
        }
        StartCoroutine(ArrowAnim(leftArrow));
    }

    public void CycleRight() {
        switch (abilityType)
        {
            case AbilityType.Model:
                SetNextModel(true);
                GameManager.playerInfo[characterSelector.playerID].model = Model;
                text.text = Model.ToString();
                break;
            case AbilityType.Light:
                Light++;
                GameManager.playerInfo[characterSelector.playerID].light = Light;
                text.text = Light.ToString();
                break;
            case AbilityType.Heavy:
                Heavy++;
                GameManager.playerInfo[characterSelector.playerID].heavy = Heavy;
                text.text = Heavy.ToString();
                break;
            case AbilityType.Passive:
                Passive++;
                GameManager.playerInfo[characterSelector.playerID].passive = Passive;
                text.text = Passive.ToString();
                break;
            default:
                break;
        }
        StartCoroutine(ArrowAnim(rightArrow));
    }

    IEnumerator ArrowAnim(SpriteRenderer arrow) { 
        arrow.color = new Color(leftArrow.color.r, leftArrow.color.g, leftArrow.color.b, 1f);
        yield return new WaitForSeconds(.1f);
        arrow.color = new Color(leftArrow.color.r, leftArrow.color.g, leftArrow.color.b, 0f);
    }

    void SetNextModel(bool right) {
        ModelAbility startModel = Model;

        if (right)
        {
            Model++;
        }
        else
        {
            Model--;
        }

        List<ModelAbility> currentModels = new List<ModelAbility>(GameManager.playerInfo.Count);
        for (int i = 0; i < GameManager.playerInfo.Count; i++) {
            currentModels.Add(GameManager.playerInfo[i].model);
        }
        for(int i = 0; i < currentModels.Count; i++) 
        {
            if (currentModels.Contains(Model))
            {
                if (right)
                {
                    Model++;
                }
                else
                {
                    Model--;
                }
            }
            else { 
                characterPreview.ChangeCharacter((int)Model);
                GameManager.playerInfo[characterSelector.playerID].model = Model;
                return;
            }
        }
        Model = startModel;
    }
}
