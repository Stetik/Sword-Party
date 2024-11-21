using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviour
{
    // Movement variables
    [SerializeField] public int speed;
    [SerializeField] public int JumpForce;
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
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask EnemyLayers;
    private int normalDamage = 1;      // Normal attack damage
    private int chargedDamage = 2;     // Charged attack damage
    private bool isCharging = false;   // Tracks if the player is charging an attack
    private float chargeTime = 1.0f;   // Time required for a full charge
    private float chargeStartTime;

    // Defense variables
    private bool isDefending = false;  // Tracks if the player is in defense mode

    // Health and damage variables
    private int health = 3; // Each player starts with 3 health points

    private PhotonView pv;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (pv.IsMine)
        {
            HandleMovement();

            // Allow all actions in both Lobby and Gameplay states
            if (Input.GetKeyDown(KeyCode.Mouse0)) // Start charging or attack
            {
                StartCharging();
            }
            if (Input.GetKeyUp(KeyCode.Mouse0)) // Release attack
            {
                ReleaseAttack();
            }
            if (Input.GetKeyDown(KeyCode.Mouse1)) // Start defense
            {
                isDefending = true;
                animator.SetBool("IsDefending", true);
            }
            if (Input.GetKeyUp(KeyCode.Mouse1)) // Stop defense
            {
                isDefending = false;
                animator.SetBool("IsDefending", false);
            }

            // Dash logic
            if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
            {
                StartCoroutine(Dash());
            }
        }
    }

    private void HandleMovement()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);

        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, JumpForce);
            animator.SetBool("IsJumping", true); // Set jumping to true when jump starts
        }

        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }

        FlipCharacter();
        animator.SetFloat("Speed", Mathf.Abs(horizontal));
        animator.SetBool("IsJumping", !IsGrounded());
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
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        animator.SetBool("IsDashing", true);

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(transform.localScale.x * dashingPower, 0f);
        yield return new WaitForSeconds(dashingTime);

        animator.SetBool("IsDashing", false);

        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    private void StartCharging()
    {
        isCharging = true;
        chargeStartTime = Time.time;
        animator.SetBool("IsCharging", true); // Activate charging animation
    }

    private void ReleaseAttack()
    {
        if (!isCharging) return;

        isCharging = false;
        animator.SetBool("IsCharging", false);
        float elapsedChargeTime = Time.time - chargeStartTime;

        if (elapsedChargeTime >= chargeTime)
        {
            animator.SetTrigger("ChargedAttack");
            Attack(chargedDamage);
        }
        else
        {
            animator.SetTrigger("Attack");
            Attack(normalDamage);
        }
    }

    private void Attack(int damage)
    {
        Debug.Log($"Current GameState in Attack: {GameManager.Instance.CurrentState}");

        // Prevent dealing damage in the lobby
        if (GameManager.Instance.CurrentState == GameManager.GameState.Lobby)
        {
            Debug.Log("Attacks in the lobby do not deal damage.");
            return; // Skip dealing damage in the lobby
        }

        // Detect enemies in range of the attack
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, EnemyLayers);

        // Deal damage in gameplay
        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<PhotonView>()?.RPC("TakeDamage", RpcTarget.AllBuffered, damage);
            Debug.Log($"Hit {enemy.name} for {damage} damage");
        }
    }

    [PunRPC]
    public void TakeDamage(int damageAmount)
    {
        if (!pv.IsMine) return;

        // Prevent taking damage in the lobby
        if (GameManager.Instance.CurrentState == GameManager.GameState.Lobby)
        {
            Debug.Log("Damage is disabled in the lobby.");
            return;
        }

        health -= damageAmount;
        Debug.Log($"Player {pv.ViewID} took {damageAmount} damage. Remaining health: {health}");

        if (health <= 0)
        {
            pv.RPC("Die", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    private void Die()
    {
        Debug.Log($"Player {pv.ViewID} has died.");
        gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}
