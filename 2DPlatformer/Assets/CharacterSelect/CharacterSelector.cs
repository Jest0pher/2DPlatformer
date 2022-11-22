using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class CharacterSelector : MonoBehaviour
{
    PlayerInput playerInput;
    public GameObject highlight;
    public Selector currentSelector;
    int selectedIndex;
    public List<Selector> selectors;
    public int playerID = 0;
    public bool isReady;

    int prevHorVal = 0;
    int prevVertVal = 0;
    bool prevConfirm = false;
    bool prevBack = false;

    bool started = false;
    // Start is called before the first frame update
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        CharacterSelectManager.Instance.PlayerConnect(this, playerInput.devices[0]);
        selectedIndex = 0;
        currentSelector = selectors[0];
        highlight.transform.position = currentSelector.transform.position + Vector3.back;
        started = true;
    }

    public void NavigationInput(InputAction.CallbackContext obj) {
        if (!started)
            return;

        Vector2 vec = obj.ReadValue<Vector2>();
        if (prevHorVal != (int)vec.x)
        {
            if (vec.x > 0)
            {
                RightInput();
            }
            else if (vec.x < 0)
            {
                LeftInput();
            }
        }
        if (prevVertVal != (int)vec.y)
        {
            if (vec.y > 0)
            {
                UpInput();
            }
            else if (vec.y < 0)
            {
                DownInput();
            }
        }
        prevHorVal = (int)vec.x;
        prevVertVal = (int)vec.y;

    }

    public void UpInput() {
        
        selectedIndex--;
        if (selectedIndex < 0)
            selectedIndex += selectors.Count;

        currentSelector = selectors[selectedIndex % selectors.Count];
        highlight.transform.position = currentSelector.transform.position + Vector3.back;
    }

    public void DownInput() {
        currentSelector = selectors[(++selectedIndex + selectors.Count) % selectors.Count];
        highlight.transform.position = currentSelector.transform.position + Vector3.back;
    }

    public void LeftInput()
    {
        if (currentSelector.selectorType == SelectorType.Ability)
        {
            AbilitySelector castedSelector = (AbilitySelector)currentSelector;
            castedSelector.CycleLeft();
        }
    }

    public void RightInput()
    {
        if (currentSelector.selectorType == SelectorType.Ability)
        {
            AbilitySelector castedSelector = (AbilitySelector)currentSelector;
            castedSelector.CycleRight();
        }
    }
    public void ConfirmInput(InputAction.CallbackContext obj) {
        if (!started)
            return;

        bool val = obj.ReadValueAsButton();
        if (val && val != prevConfirm) {
            if (currentSelector.selectorType == SelectorType.Button)
            {
                isReady = !isReady;
                ButtonSelector castedSelector = (ButtonSelector)currentSelector;
                castedSelector.SetButton(isReady);
            }
        }
        prevConfirm = val;
    }
    public void BackInput(InputAction.CallbackContext obj) {
        if (!started)
            return;

        bool val = obj.ReadValueAsButton();
        if (val && val != prevBack)
        {
            if (isReady)
            {
                isReady = false;
                if (currentSelector.selectorType == SelectorType.Button)
                {
                    ButtonSelector castedSelector = (ButtonSelector)currentSelector;
                    castedSelector.SetButton(false);
                }
            }
            else {
                if (GameManager.playerInfo.Count > 0 && started)
                {
                    CharacterSelectManager.Instance.PlayerDisconnect(this);
                    Destroy(gameObject);
                }
            }
        }
        prevBack = val;
    }
}
