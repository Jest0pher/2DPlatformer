using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Rigidbody2D r;
    BoxCollider2D box;
    [Header("Surrounding")]
    public bool facingRight;
    [Space]
    public bool againstWall;
    public bool againstLeftWall;
    public bool againstRightWall;
    [Space]
    public bool isGrounded;

    [Space(50)]
    [Header("Movement")]
    public Vector2 moveDir;
    public float speed;
    public float maxHorSpeed;
    public float maxVertSpeed;
    private Vector2 prevVel = Vector2.zero;

    [Space(50)]
    [Header("Jumping")]
    public bool jump;
    [SerializeField]private bool prevJump;

    [Space]
    public int jumps = 2;
    [SerializeField] private int jumpCountRuntime;
    public float jumpSpeed;
    [Space]
    public int wallJumps = 3;
    [SerializeField] private int wallJumpCountRuntime;
    public float wallJumpSpeed;
    public float gravScaleOnWall;

    [Space(50)]
    [Header("Weapon")]

    [Space(50)]
    [Header("Sprite")]
    [SerializeField] Animator spriteAnim;

    [Space(50)]
    [Header("Transforms")]
    [SerializeField] GameObject frontTransform;
    [SerializeField] GameObject backTransform;
    [SerializeField] GameObject feetTransform;
    [SerializeField] LayerMask groundMask;

    [Space(50)]
    [Header("Debug")]
    [SerializeField] private bool debug;
    // Start is called before the first frame update
    void Start()
    {
        r = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
        jumpCountRuntime = jumps;
        wallJumpCountRuntime = wallJumps;

        frontTransform.transform.position = box.transform.position + (Vector3)box.size / 2.0f;
        backTransform.transform.position = box.transform.position - (Vector3)box.size / 2.0f;
    }

    private void OnDrawGizmos()
    {
        //Gizmos.DrawWireCube(box.transform.position, box.size);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        moveDir = Vector2.zero;
        CheckSurroundings();
        float adjustedHor = isGrounded ? 1 : .5f;
        moveDir += Vector2.right * InputManager.Instance.HorizontalInput * adjustedHor * speed;
        Flip();
        Jump();

        if (againstWall && r.velocity.y < 0)
        {
            r.gravityScale = gravScaleOnWall;
        }
        else {
            r.gravityScale = 1;
        }
        moveDir += Vector2.up * r.velocity;
        moveDir.Set(Mathf.Clamp(moveDir.x, -maxHorSpeed, maxHorSpeed), Mathf.Clamp(moveDir.y, -maxVertSpeed, maxVertSpeed));
        r.velocity = moveDir;

        SetAnim();

        prevVel = r.velocity;
        if (debug)
        {
            Debug.DrawLine(transform.position - Vector3.one * .5f, transform.position + Vector3.left*.5f + Vector3.up*.5f, Color.red);
            Debug.DrawLine(transform.position + Vector3.one * .5f, transform.position + Vector3.right * .5f + Vector3.down * .5f, Color.red);
            Debug.DrawLine(transform.position + Vector3.left * .5f + Vector3.up * .5f, transform.position + Vector3.one * .5f, Color.red);
            Debug.DrawLine(transform.position + Vector3.down * .5f + Vector3.right * .5f, transform.position - Vector3.one * .5f, Color.red);
        }
    }

    void CheckSurroundings() {
        float surroundDistance = .1f;
        isGrounded = Physics2D.Raycast(feetTransform.transform.position, Vector3.down, surroundDistance, groundMask) ||
                     Physics2D.Raycast(feetTransform.transform.position + Vector3.right * box.size.x / 2, Vector3.down, surroundDistance, groundMask) || 
                     Physics2D.Raycast(feetTransform.transform.position + Vector3.left * box.size.x / 2, Vector3.down, surroundDistance, groundMask);
        Debug.DrawRay(feetTransform.transform.position, Vector3.down * surroundDistance, Color.red);
        Debug.DrawRay(feetTransform.transform.position + Vector3.right * box.size.x / 2, Vector3.down * surroundDistance, Color.red);
        Debug.DrawRay(feetTransform.transform.position + Vector3.left * box.size.x / 2, Vector3.down * surroundDistance, Color.red);

        if (isGrounded)
        {
            jumpCountRuntime = jumps;
            wallJumpCountRuntime = wallJumps;
        }

        againstLeftWall =  facingRight ? Physics2D.OverlapCircle(backTransform.transform.position, surroundDistance, groundMask) : Physics2D.OverlapCircle(frontTransform.transform.position, surroundDistance, groundMask); 
        againstRightWall = facingRight ? Physics2D.OverlapCircle(frontTransform.transform.position, surroundDistance, groundMask) : Physics2D.OverlapCircle(backTransform.transform.position, surroundDistance, groundMask);

        againstWall = false;
        if (againstLeftWall || againstRightWall)
        {
            againstWall = true;
        }
    }

    void Flip() {
        if (moveDir.x > 0 && !facingRight || moveDir.x < 0 && facingRight) {
            facingRight = !facingRight;
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }

    void Jump() {
        jump = InputManager.Instance.JumpInput;

        if ((jump && !prevJump)) {
            if (againstWall)
            {
                if (wallJumpCountRuntime > 0)
                {
                    wallJumpCountRuntime--;
                    moveDir += Vector2.up * wallJumpSpeed;
                }
            }
            else {
                if (jumpCountRuntime > 0) {
                    jumpCountRuntime--;
                    moveDir += Vector2.up * jumpSpeed;
                }
            }
        }

        prevJump = jump;
    }

    void SetAnim() {
        spriteAnim.SetBool("IsGrounded", isGrounded);
        spriteAnim.SetBool("AgainstWall", againstWall);
        spriteAnim.SetFloat("yVelocity", r.velocity.y);
        if (isGrounded)
        {
            if (InputManager.Instance.HorizontalInput != 0)
            {
                spriteAnim.SetBool("Running", true);
            }
            else
            {
                spriteAnim.SetBool("Running", false);
            }
        }
        else {

            if ((againstLeftWall && facingRight) || (againstRightWall && !facingRight))
            {
                spriteAnim.SetBool("FacingAway", true);
            }
            else if ((againstLeftWall && !facingRight) || (againstRightWall && facingRight)) 
            {
                spriteAnim.SetBool("FacingAway", false);
            }
        }
    }
}
