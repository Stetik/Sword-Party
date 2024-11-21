using System.Collections;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPun
{
    // Movement variables
    [SerializeField] private int speed = 5;
    [SerializeField] private int jumpForce = 10;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Animator animator;

    private float horizontal;
    private bool isFacingRight = true;

    private bool canDash = true;
    private bool isDashing;
    private float dashingPower = 24f;
    private float dashingTime = 0.2f;
    private float dashingCooldown = 1f;

    // Attack variables
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private LayerMask enemyLayers;
    private int normalDamage = 1;
    private int chargedDamage = 2;
    private bool isCharging = false;
    private float chargeTime = 1.0f;
    private float chargeStartTime;

    // Defense variables
    private bool isDefending = false;

    // Health variables
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    // Lobby and maps
    private GameObject lobbyObject;
    private GameObject[] maps;

    private PhotonView pv;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        currentHealth = maxHealth;
    }

    private void Start()
    {
        
        lobbyObject = GameObject.Find("Lobby");
        if (lobbyObject == null)
        {
            Debug.LogError("Lobby object not found in the scene!");
        }

        
        maps = GameObject.FindGameObjectsWithTag("Map");
        if (maps.Length == 0)
        {
            Debug.LogError("No maps found in the scene! Make sure maps have the 'Map' tag.");
        }
    }

    private void Update()
    {
        if (pv.IsMine)
        {
            HandleMovement();

            if (Input.GetKeyDown(KeyCode.Mouse0)) StartCharging();
            if (Input.GetKeyUp(KeyCode.Mouse0)) ReleaseAttack();

            if (Input.GetKeyDown(KeyCode.Mouse1)) StartDefending();
            if (Input.GetKeyUp(KeyCode.Mouse1)) StopDefending();

            if (Input.GetKeyDown(KeyCode.LeftShift) && canDash) StartCoroutine(Dash());
        }
    }

    private void HandleMovement()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);

        
        photonView.RPC("SetAnimatorFloat", RpcTarget.All, "Speed", Mathf.Abs(horizontal));

        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            photonView.RPC("SetAnimatorBool", RpcTarget.All, "IsJumping", true);
        }

        if (IsGrounded())
        {
            photonView.RPC("SetAnimatorBool", RpcTarget.All, "IsJumping", false);
        }

        FlipCharacter();
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private void FlipCharacter()
    {
        if ((isFacingRight && horizontal < 0f) || (!isFacingRight && horizontal > 0f))
        {
            isFacingRight = !isFacingRight;

            
            photonView.RPC("FlipNetwork", RpcTarget.AllBuffered, isFacingRight);
        }
    }

    [PunRPC]
    private void FlipNetwork(bool facingRight)
    {
        isFacingRight = facingRight;
        Vector3 localScale = transform.localScale;
        localScale.x = facingRight ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
        transform.localScale = localScale;
    }


    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        photonView.RPC("SetAnimatorBool", RpcTarget.All, "IsDashing", true);

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(transform.localScale.x * dashingPower, 0f);

        yield return new WaitForSeconds(dashingTime);

        rb.gravityScale = originalGravity;
        isDashing = false;
        photonView.RPC("SetAnimatorBool", RpcTarget.All, "IsDashing", false);

        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    private void StartCharging()
    {
        isCharging = true;
        chargeStartTime = Time.time;
        photonView.RPC("SetAnimatorBool", RpcTarget.All, "IsCharging", true);
    }

    private void ReleaseAttack()
    {
        if (!isCharging) return;

        isCharging = false;
        photonView.RPC("SetAnimatorBool", RpcTarget.All, "IsCharging", false);
        float elapsedChargeTime = Time.time - chargeStartTime;

        if (elapsedChargeTime >= chargeTime)
        {
            photonView.RPC("TriggerAnimator", RpcTarget.All, "ChargedAttack");
            Attack(chargedDamage);
        }
        else
        {
            photonView.RPC("TriggerAnimator", RpcTarget.All, "Attack");
            Attack(normalDamage);
        }
    }

    private void Attack(int damage)
    {
        if (GameManager.Instance.CurrentState == GameManager.GameState.Lobby) return;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<PhotonView>()?.RPC("TakeDamage", RpcTarget.AllBuffered, damage);
            Debug.Log($"Hit {enemy.name} for {damage} damage");
        }
    }

    private void StartDefending()
    {
        isDefending = true;
        photonView.RPC("SetAnimatorBool", RpcTarget.All, "IsDefending", true);
        Debug.Log("Started defending");
    }

    private void StopDefending()
    {
        isDefending = false;
        photonView.RPC("SetAnimatorBool", RpcTarget.All, "IsDefending", false);
        Debug.Log("Stopped defending");
    }

    [PunRPC]
    public void SetAnimatorBool(string parameter, bool value)
    {
        animator.SetBool(parameter, value);
    }

    [PunRPC]
    public void SetAnimatorFloat(string parameter, float value)
    {
        animator.SetFloat(parameter, value);
    }

    [PunRPC]
    public void TriggerAnimator(string trigger)
    {
        animator.SetTrigger(trigger);
    }

    [PunRPC]
    public void TakeDamage(int damageAmount)
    {
        if (!pv.IsMine) return;

        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            photonView.RPC(nameof(Die), RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    public void AddHealth(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        Debug.Log($"Current Health: {currentHealth}");
    }

    [PunRPC]
    public void Die()
    {
        Debug.Log($"Player {photonView.ViewID} has died.");
        gameObject.SetActive(false);

        photonView.RPC(nameof(RespawnInLobby), RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void RespawnInLobby()
    {
        Debug.Log("Respawning player in the lobby...");

        if (lobbyObject != null)
        {
            lobbyObject.SetActive(true);
        }
        else
        {
            Debug.LogError("Lobby object not found!");
        }

        foreach (GameObject map in maps)
        {
            if (map != null) map.SetActive(false);
        }

        currentHealth = maxHealth;
        transform.position = lobbyObject.transform.position;
        gameObject.SetActive(true);
        Debug.Log($"Player has respawned in the lobby.");
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}
