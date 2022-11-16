using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public InputActionMap input;
    [SerializeField] private float horizontalInput;
    [SerializeField] private bool jumpInput;

    public float HorizontalInput { get { return horizontalInput; } set { horizontalInput = Mathf.Clamp(value, -1, 1); } }
    public bool JumpInput { get { return jumpInput; } set { jumpInput = value; } }

    [field: SerializeField]
    public float mobileInput { get; set; }
    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //HorizontalInput = Input.GetAxisRaw("Horizontal") + mobileInput;
        //HorizontalInput = input.Player1.Horizontal.ReadValue<float>();
        JumpInput = Input.GetAxisRaw("Jump") > 0;
    }
}
