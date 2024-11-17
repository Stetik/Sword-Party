using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Runtime.CompilerServices;

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
            // Start charging or normal attack
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                StartCharging();
            }

            // Release attack (either normal or charged)
            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                ReleaseAttack();
            }

            // Defense mode
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                isDefending = true;
            }
            if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                isDefending = false;
            }

            if (isDashing)
            {
                return;
            }

            // Update animator parameters for animations
            animator.SetFloat("Speed", Mathf.Abs(horizontal)); // Update Speed for run animation

            // Check if player is grounded to update jump state
            if (IsGrounded())
            {
                animator.SetBool("IsJumping", false); // If grounded, not jumping
            }
            else
            {
                animator.SetBool("IsJumping", true); // If in air, jumping
            }

            // Movement logic
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

            if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
            {
                StartCoroutine(Dash());
            }

            flip();
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private void flip()
    {
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
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

    // Start charging for an attack
    private void StartCharging()
    {
        isCharging = true;
        chargeStartTime = Time.time;
    }

    // Release attack, performing either a normal or charged attack
    private void ReleaseAttack()
    {
        if (isCharging)
        {
            isCharging = false;
            float elapsedChargeTime = Time.time - chargeStartTime;

            // Check if the charge duration meets or exceeds the required charge time
            if (elapsedChargeTime >= chargeTime)
            {
                Attack(chargedDamage); // Perform a charged attack
            }
            else
            {
                Attack(normalDamage); // Perform a normal attack
            }
        }
    }

    // Attack method with a damage parameter for normal or charged attacks
    private void Attack(int damage)
    {
        // Trigger attack animation
        animator.SetTrigger("Attack");

        // Detect enemies in range of the attack
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, EnemyLayers);

        // Deal damage to enemies
        foreach (Collider2D enemy in hitEnemies)
        {
            // Call TakeDamage on the enemy's PhotonView using RPC
            enemy.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, damage);
            Debug.Log("We hit " + enemy.name + " with damage: " + damage);
        }
    }


    [PunRPC]
    public void TakeDamage(int damageAmount)
    {
        if (!pv.IsMine) return;

        // If player was charging an attack when hit, cancel charge and reduce health
        if (isCharging)
        {
            CancelCharge();
            health -= 1;  // Penalty for being hit while charging
            Debug.Log("Charge canceled! Player lost 1 health. Remaining health: " + health);
        }

        // Check if the player is defending
        if (isDefending)
        {
            if (damageAmount == normalDamage)
            {
                Debug.Log("Attack blocked! No damage taken.");
                return; // Block normal attack, no damage taken
            }
            else if (damageAmount == chargedDamage)
            {
                damageAmount = 1; // Reduce charged attack damage to 1
                Debug.Log("Blocked a charged attack! Took reduced damage: 1");
            }
        }

        health -= damageAmount;
        Debug.Log("Player " + pv.ViewID + " Remaining Health: " + health);

        if (health <= 0)
        {
            pv.RPC("Die", RpcTarget.AllBuffered);
        }
    }

    // Cancels charging without attacking
    private void CancelCharge()
    {
        isCharging = false;
    }

    [PunRPC]
    private void Die()
    {
        Debug.Log("Player " + pv.ViewID + " has died.");
        gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
        {
            return;
        }
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
