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
    public float prevInput;

    [Space(50)]
    [Header("Slamming")]
    [SerializeField] CircleCollider2D slamCircle;
    [SerializeField] ParticleSystem slamParticles;
    public bool slamming;
    public bool slammed;
    public float slamTime;
    float slamTimer;
    public float slamInAirLimit;
    float inAirTimer;
    public float slammingSpeed;
    public float slamImpactPushVel;

    [Space(50)]
    [Header("Hanging")]
    [SerializeField] BoxCollider2D hangBox;
    public bool isHanging;
    public bool canHang;
    public float hangDelayTime;
    float hangDelayTimer;
    Vector2 hangCorner;
    [SerializeField] GameObject playerHang;

    [Space(50)]
    [Header("Jumping")]
    public bool jump;
    [SerializeField] private bool prevJump;

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
    public float HP { get { return hp; } set { hp = Mathf.Clamp(value, 0, maxHP); } }
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
    bool prevAttackInput;

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
    [SerializeField] LayerMask hangPoint;

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
            Hang();
            Slamming();
            r.velocity = moveDir;
            SetAnim();
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
        isGrounded = (Physics2D.Raycast(feetTransform.transform.position, Vector3.down, surroundDistance, groundMask) ||
                     Physics2D.Raycast(feetTransform.transform.position + Vector3.right * box.size.x / 2, Vector3.down, surroundDistance, groundMask) || 
                     Physics2D.Raycast(feetTransform.transform.position + Vector3.left * box.size.x / 2, Vector3.down, surroundDistance, groundMask)) && r.velocity.y == 0;
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
            inAirTimer = 0;
            if (!isHanging )
            {
                jumpCountRuntime = jumps;
                wallJumpCountRuntime = wallJumps;
            }
        }
        else { 
            inAirTimer += Time.deltaTime;
        }


        bool isOnOuter = Physics2D.Raycast(feetTransform.transform.position, Vector3.down, surroundDistance, outerBorder) ||
                     Physics2D.Raycast(feetTransform.transform.position + Vector3.right * box.size.x / 2, Vector3.down, surroundDistance, outerBorder) ||
                     Physics2D.Raycast(feetTransform.transform.position + Vector3.left * box.size.x / 2, Vector3.down, surroundDistance, outerBorder);
        if (isOnOuter) {
            groundBox.enabled = true;
        }

        if (groundBox.enabled && canHang)
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
        if (slamming || slammed)
            return;

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
            if (isHanging) {
                isHanging = false;
                hangDelayTimer = hangDelayTime;
            }
        }

        prevJump = jump;
    }
    private void Move()
    {
        if (slamming || slammed)
            return;

        float adjustedHor = 1;
        if (isDamaged)
        {
            if (isGrounded) {
                adjustedHor = .95f;
            }
            moveDir += Vector2.right * ((r.velocity.x  <= .01 && r.velocity.x >= -.01)? 0 : r.velocity.x) * adjustedHor;
        }
        else
        {
            if (isGrounded && isAttacking)
            {
                adjustedHor = 0;
            }
            else if (!isGrounded && !isAttacking)
            {
                adjustedHor = .5f;
            }
            else if (!isGrounded && isAttacking)
            {
                adjustedHor = .25f;
            }
            moveDir += Vector2.right * HorizontalInput * adjustedHor * speed;
        }
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
                if(!isDamaged && inAirTimer > slamInAirLimit && !isHanging)
                    slamming = true;
            }
            if (isHanging) {
                isHanging = false;
                hangDelayTimer = hangDelayTime;
                jumpCountRuntime = Mathf.Clamp(jumpCountRuntime - 1, 0, jumps);
            }
        }
        moveDir.Set(Mathf.Clamp(moveDir.x, -maxHorSpeed, maxHorSpeed), Mathf.Clamp(moveDir.y, -maxVertSpeed, maxVertSpeed));
    }
    void Attack() {
        if (slamming || slammed || isHanging)
            return;

        if (!isAttacking && !isDamaged)
        {
            if (AttackInput && !prevAttackInput)
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
        prevAttackInput = AttackInput;
    }
    void SetAnim() {
        spriteAnim.SetBool("IsGrounded", isGrounded);
        spriteAnim.SetBool("AgainstWall", againstWall);
        spriteAnim.SetBool("IsAttacking", isAttacking);
        spriteAnim.SetBool("IsDamaged", isDamaged);
        spriteAnim.SetBool("IsSlamming", slamming || slammed);
        spriteAnim.SetBool("IsHanging", isHanging);
        spriteAnim.SetFloat("yVelocity", r.velocity.y);
        spriteAnim.SetFloat("SpeedMult", r.velocity.x / maxHorSpeed);
        spriteAnim.SetFloat("HitMult", 1/damageTime);
        spriteAnim.SetFloat("AttackMult", 1/attackTime);

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
        slamCircle.enabled = false;
        int num = r.OverlapCollider(groundContact, colliders);
        slamCircle.enabled = true;
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
    void Slamming() {
        if (slammed) {
            slamTimer -= Time.deltaTime;
            if (slamTimer <= 0) {
                slammed = false;
            }
        }
        if (slamming)
        {
            moveDir.Set(moveDir.x, -slammingSpeed);
            if (isGrounded)
            {
                slammed = true;
                slamTimer = slamTime;
                slamming = false;
                GroundSlam();
            }

        }
        if (isDamaged) {
            slamming = false;
            slammed = false;
        }
    }
    void Hang() {
        if (slamming || slammed || isAttacking)
            return;

        if (canHang)
        {
            RaycastHit2D hit = Physics2D.BoxCast(hangBox.transform.position, hangBox.size, 0, Vector2.zero, 0, hangPoint);
            if (hit.collider != null)
            {
                if (hit.collider.GetComponent<HangPoint>().rightSide != facingRight)
                {
                    jumpCountRuntime = Mathf.Clamp(jumpCountRuntime + 1, 0, jumps);
                    canHang = false;
                    isHanging = true;
                    hangCorner = (Vector2)hit.collider.bounds.center + ((facingRight ? Vector2.left : Vector2.right) * hit.collider.bounds.size.x / 2.0f) + (Vector2.up * hit.collider.bounds.size.y / 2.0f);
                }
            }
        }

        if (hangDelayTimer > 0) {
            hangDelayTimer -= Time.deltaTime;
            if (hangDelayTimer <= 0) {
                canHang = true;
            }
        }

        if (isDamaged) {
            isHanging = false;
            hangDelayTimer = hangDelayTime;
            jumpCountRuntime = Mathf.Clamp(jumpCountRuntime - 1, 0, jumps);
        }

        if (isHanging) {
            r.gravityScale = 0;
            r.velocity = Vector2.zero;
            moveDir = Vector2.zero;
            Vector2 translateVec = hangCorner - (Vector2)playerHang.transform.position;
            transform.position += (Vector3)translateVec;
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
                    player.TakeDamage(15);
                }
            }
        }
    }
    public void TakeDamage(float damage) {
        if (!isDamaged)
        {
            HP -= damage;
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
    void GroundSlam() {
        slamParticles.Play();
        List<Collider2D> colliders = new List<Collider2D>();
        int num = r.OverlapCollider(playerRaycastContact, colliders);
        foreach (Collider2D collider in colliders) {
            Player player = collider.GetComponent<Player>();
            player.TakeDamage(18);
            Vector2 forceVector = (player.transform.position - feetTransform.transform.position).normalized;
            player.r.velocity = forceVector * slamImpactPushVel;
        }
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
