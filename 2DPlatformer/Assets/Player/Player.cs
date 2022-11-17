using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public enum Players { 
    Player1,
    Player2,
    Player3,
    Player4
}
public class Player : MonoBehaviour
{
    Rigidbody2D r;
    BoxCollider2D box;
    public PlayerInput pInput;

    public Players PlayerID;

    [Space]
    [Header("Input")]
    [SerializeField] private float horizontalInput;
    public float HorizontalInput { get { return horizontalInput; } set { horizontalInput = Mathf.Clamp(value, -1, 1); } }
    [SerializeField] private bool jumpInput;
    public bool JumpInput { get { return jumpInput; } set { jumpInput = value; } }
    [SerializeField] private bool attackInput;
    public bool AttackInput { get { return attackInput; } set { attackInput = value; } }
    
    [Space(50)]
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
    [Header("Health")]
    public float maxHP = 100;
    [SerializeField] private float hp;
    public float HP { get { return hp; } set { hp = Mathf.Clamp(value, 0, maxHP);} }
    public bool isDead = false;
    public bool isDamaged = false;
    public float damageTime;
    float damageTimer;

    [Space(50)]
    [Header("Weapon")]
    public bool isAttacking;
    public float attackTime;
    float attackTimer;
    [SerializeField] BoxCollider2D boxRaycast;

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
        GameManager.Instance.players[GameManager.Instance.currentPlayersCount] = this;
        GameManager.Instance.currentPlayersCount++;
        r = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
        jumpCountRuntime = jumps;
        wallJumpCountRuntime = wallJumps;
        attackTimer = attackTime;
        HP = maxHP;
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
        if (!isDead)
        {
            if (isDamaged) {
                damageTimer -= Time.deltaTime;
                if (damageTimer <= 0) {
                    isDamaged = false;
                }
            }
            moveDir = Vector2.zero;
            CheckSurroundings();
            Attack();
            Jump();
            Move();
            SetAnim();
            r.velocity = moveDir;
            Flip();
            prevVel = r.velocity;
            if (debug)
            {
                Debug.DrawLine(transform.position - Vector3.one * .5f, transform.position + Vector3.left * .5f + Vector3.up * .5f, Color.red);
                Debug.DrawLine(transform.position + Vector3.one * .5f, transform.position + Vector3.right * .5f + Vector3.down * .5f, Color.red);
                Debug.DrawLine(transform.position + Vector3.left * .5f + Vector3.up * .5f, transform.position + Vector3.one * .5f, Color.red);
                Debug.DrawLine(transform.position + Vector3.down * .5f + Vector3.right * .5f, transform.position - Vector3.one * .5f, Color.red);
            }
        }
    }
    #region Update Functions
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
        jump = JumpInput;

        if ((jump && !prevJump) && !isAttacking && !isDamaged) {
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
    private void Move()
    {
        float adjustedHor = 1;
        if (isGrounded && (isAttacking || isDamaged))
        {
            adjustedHor = 0;
        }
        else if (!isGrounded && (!isAttacking || !isDamaged))
        {
            adjustedHor = .5f;
        }
        else if (!isGrounded && (isAttacking || isDamaged)) {
            adjustedHor = .25f;
        }
        moveDir += Vector2.right * HorizontalInput * adjustedHor * speed;
        if (againstWall && r.velocity.y < 0)
        {
            r.gravityScale = gravScaleOnWall;
        }
        else
        {
            r.gravityScale = 1;
        }
        moveDir += Vector2.up * r.velocity;
        moveDir.Set(Mathf.Clamp(moveDir.x, -maxHorSpeed, maxHorSpeed), Mathf.Clamp(moveDir.y, -maxVertSpeed, maxVertSpeed));
    }
    void Attack() {
        if (!isAttacking && !isDamaged)
        {
            if (AttackInput)
            {
                isAttacking = true;
                attackTimer = attackTime;
            }
        }
        else if(isAttacking){
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0) {
                isAttacking = false;
            }
        }
    }
    void SetAnim() {
        spriteAnim.SetBool("IsGrounded", isGrounded);
        spriteAnim.SetBool("AgainstWall", againstWall);
        spriteAnim.SetBool("IsAttacking", isAttacking);
        spriteAnim.SetBool("IsDamaged", isDamaged);
        spriteAnim.SetFloat("yVelocity", r.velocity.y);
        if (isGrounded)
        {
            if (HorizontalInput != 0)
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
    #endregion


    #region Event Functions
    public void AttackRaycast() {
        RaycastHit2D hit = Physics2D.BoxCast(boxRaycast.transform.position, boxRaycast.size, 0, Vector2.zero);
        if (hit.collider != null) {
            if (hit.collider.tag == "Player") {
                Player player = hit.collider.GetComponent<Player>();
                player.TakeDamage(10);
            }
        }
    }
    public void TakeDamage(float _hp) {
        if (!isDamaged)
        {
            HP -= _hp;
            if (HP <= 0) { Die(); }
            isDamaged = true;
            damageTimer = damageTime;
        }

    }
    void Die() {
        isDead = true;
        box.enabled = false;
        r.gravityScale = 0;
        spriteAnim.SetBool("IsDead", true);
    }
    public void PostDeath() {
        StartCoroutine(DeathFade());
        spriteAnim.enabled = false;
    }
    IEnumerator DeathFade() {
        SpriteRenderer sprRender = spriteAnim.GetComponent<SpriteRenderer>();
        Material sprMat = sprRender.material;
        float percentage = 1;
        while (percentage > 0) {
            yield return null;
            percentage -= Time.deltaTime;
            sprMat.SetFloat("_Percent", percentage);
        }
        yield return new WaitForSeconds(1);
        this.enabled = false;
    }
    #endregion


    #region Player Input Events
    public void ReadHorizontal(InputAction.CallbackContext obj)
    {
        HorizontalInput = obj.ReadValue<float>();
    }

    public void ReadJump(InputAction.CallbackContext obj)
    {
        JumpInput = obj.ReadValue<float>() > 0;
    }

    public void ReadAttack(InputAction.CallbackContext obj)
    {
        AttackInput = obj.ReadValue<float>() > 0;
    }
    #endregion
}
