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
    [SerializeField] BoxCollider2D groundBox;
    public PlayerInput pInput;

    public Players PlayerID;
    static int numPlayersLoaded = 0;
    [Space]
    [Header("Input")]
    [SerializeField] private float horizontalInput;
    public float HorizontalInput { get { return horizontalInput; } set { horizontalInput = Mathf.Clamp(value, -1, 1); } }
    [SerializeField] private bool jumpInput;
    public bool JumpInput { get { return jumpInput; } set { jumpInput = value; } }
    [SerializeField] private bool downInput;
    bool prevDownInput;
    public bool DownInput { get { return downInput; } set { downInput = value; } }
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
    [Header("Slamming")]
    public bool slamming;

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
    [Space]
    public bool groundAbove;
    public bool inGround;
    ContactFilter2D groundContact;

    [Space(50)]
    [Header("Health")]
    [SerializeField] SpriteRenderer healthBar;
    Material healthBarMat;
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
    ContactFilter2D playerRaycastContact;

    [Space(50)]
    [Header("Sprite")]
    [SerializeField] Animator spriteAnim;

    [Space(50)]
    [Header("Transforms")]
    [SerializeField] GameObject frontTransform;
    [SerializeField] GameObject backTransform;
    [SerializeField] GameObject feetTransform;
    [SerializeField] GameObject topTransform;
    [SerializeField] LayerMask groundMask;
    [SerializeField] LayerMask playerMask;
    [SerializeField] LayerMask outerBorder;

    [Space(50)]
    [Header("Debug")]
    [SerializeField] private bool debug;
    // Start is called before the first frame update
    void Start()
    {
        PlayerID = (Players)numPlayersLoaded++;
        GameManager.Instance.players.Add(this);

        r = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
        healthBarMat = healthBar.material;
        jumpCountRuntime = jumps;
        wallJumpCountRuntime = wallJumps;
        attackTimer = attackTime;
        HP = maxHP;
        frontTransform.transform.position = box.transform.position + (Vector3)box.size / 2.0f;
        backTransform.transform.position = box.transform.position - (Vector3)box.size / 2.0f;
        playerRaycastContact = new ContactFilter2D();
        playerRaycastContact.useDepth = false;
        playerRaycastContact.useNormalAngle = false;
        playerRaycastContact.useOutsideDepth = false;
        playerRaycastContact.useOutsideNormalAngle = false;
        playerRaycastContact.useTriggers = false;
        playerRaycastContact.layerMask = playerMask;
        playerRaycastContact.useLayerMask = true;
        groundContact = new ContactFilter2D();
        groundContact.useDepth = false;
        groundContact.useNormalAngle = false;
        groundContact.useOutsideDepth = false;
        groundContact.useOutsideNormalAngle = false;
        groundContact.useTriggers = false;
        groundContact.layerMask = groundMask;
        groundContact.useLayerMask = true;
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
            HealthBar();
            if (isDamaged) {
                damageTimer -= Time.deltaTime;
                if (damageTimer <= 0) {
                    isDamaged = false;
                }
            }
            moveDir = Vector2.zero;
            CheckSurroundings();
            InGround();
            Attack();
            Jump();
            GroundAbove();
            Move();
            SetAnim();
            r.velocity = moveDir;
            Flip();
            prevVel = r.velocity;
            prevDownInput = DownInput;
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

        groundAbove = Physics2D.Raycast(topTransform.transform.position, Vector3.up, surroundDistance, groundMask) ||
                      Physics2D.Raycast(topTransform.transform.position + Vector3.right * box.size.x / 3, Vector3.up, surroundDistance, groundMask) ||
                      Physics2D.Raycast(topTransform.transform.position + Vector3.left * box.size.x / 3, Vector3.up, surroundDistance, groundMask);
        Debug.DrawRay(topTransform.transform.position, Vector3.up * surroundDistance, Color.red);
        Debug.DrawRay(topTransform.transform.position + Vector3.right * box.size.x / 3, Vector3.up * surroundDistance, Color.red);
        Debug.DrawRay(topTransform.transform.position + Vector3.left * box.size.x / 3, Vector3.up * surroundDistance, Color.red);

        if (isGrounded)
        {
            jumpCountRuntime = jumps;
            wallJumpCountRuntime = wallJumps;
        }
        bool isOnOuter = Physics2D.Raycast(feetTransform.transform.position, Vector3.down, surroundDistance, outerBorder) ||
                     Physics2D.Raycast(feetTransform.transform.position + Vector3.right * box.size.x / 2, Vector3.down, surroundDistance, outerBorder) ||
                     Physics2D.Raycast(feetTransform.transform.position + Vector3.left * box.size.x / 2, Vector3.down, surroundDistance, outerBorder);
        if (isOnOuter) {
            groundBox.enabled = true;
        }

        if (groundBox.enabled)
        {
            againstLeftWall = facingRight ? Physics2D.OverlapCircle(backTransform.transform.position, surroundDistance, groundMask) : Physics2D.OverlapCircle(frontTransform.transform.position, surroundDistance, groundMask);
            againstRightWall = facingRight ? Physics2D.OverlapCircle(frontTransform.transform.position, surroundDistance, groundMask) : Physics2D.OverlapCircle(backTransform.transform.position, surroundDistance, groundMask);

            againstWall = false;
            if (againstLeftWall || againstRightWall)
            {
                againstWall = true;
            }
        }
        else {
            againstLeftWall = false;
            againstRightWall = false;
            againstWall = false;
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
        if (DownInput && !prevDownInput) {
            if (isGrounded)
            {
                isGrounded = false;
                groundBox.enabled = false;
            }
            else
            {
                //Slam
            }
        }
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
    void HealthBar() { 
        healthBarMat.SetFloat("_HealthPercentage", HP/maxHP);
        healthBar.flipX = !facingRight;
    }
    void InGround() { 
        List<Collider2D> colliders = new List<Collider2D>();
        int num = r.OverlapCollider(groundContact, colliders);
        print(num);
        foreach (Collider2D c in colliders) {
            print(c.name);
        }
        if (num > 0)
        {
            inGround = true;
        }
        else
        {
            inGround = false;
        }
        
    }
    void GroundAbove() { 

        if (groundAbove && inGround) {
            groundBox.enabled = false;
        }

        if (!groundAbove && !inGround) {
            groundBox.enabled = true;
        }

        if(groundBox.enabled == false)
        {
            isGrounded = false;
        }
        
    }
    #endregion


    #region Event Functions
    public void AttackRaycast() {
        RaycastHit2D[] hits = new RaycastHit2D[3];
        int hitNum = Physics2D.BoxCast(boxRaycast.transform.position, boxRaycast.size, 0, Vector2.zero, playerRaycastContact, hits);
        foreach (RaycastHit2D hit in hits){
            if (hit.collider != null) {
                if (hit.collider.tag == "Player") {
                    Player player = hit.collider.GetComponent<Player>();
                    player.TakeDamage(10);
                }
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
        healthBar.gameObject.SetActive(false);
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
        GameManager.Instance.players.Remove(this);
        Destroy(gameObject);
    }
    #endregion


    #region Player Input Events
    public void ReadHorizontal(InputAction.CallbackContext obj)
    {
        HorizontalInput = obj.ReadValue<float>();
    }

    public void ReadJump(InputAction.CallbackContext obj)
    {
        JumpInput = obj.ReadValueAsButton();
    }

    public void ReadDown(InputAction.CallbackContext obj) {
        DownInput = obj.ReadValueAsButton();
    }

    public void ReadAttack(InputAction.CallbackContext obj)
    {
        AttackInput = obj.ReadValueAsButton();
    }
    #endregion
}
