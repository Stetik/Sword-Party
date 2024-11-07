using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Runtime.CompilerServices;

public class PlayerController : MonoBehaviour
{
    // Start Movement variables
    [SerializeField] public int speed;
    [SerializeField] public int JumpForce;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    private float horizontal;
    private bool isFacingRight = true;

    private bool canDash = true;
    private bool isDashing;
    private float dashingPower = 24f;
    private float dashingTime = 0.2f;
    private float dashingCooldown = 1f;
    // End Movement variables

    // Start Attack Variables
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask EnemyLayers;
    // End Attack Variables

    // Health and Damage variables
    private int health = 3; // Each player starts with 3 health points
    private int damage = 1; // Each attack deals 1 damage point

    private PhotonView pv;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (pv.IsMine)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Attack();
            }

            if (isDashing)
            {
                return;
            }

            horizontal = Input.GetAxisRaw("Horizontal");
            rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);

            if (Input.GetButtonDown("Jump") && IsGrounded())
            {
                rb.velocity = new Vector2(rb.velocity.x, JumpForce);
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
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(transform.localScale.x * dashingPower, 0f);
        yield return new WaitForSeconds(dashingTime);
        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    void Attack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, EnemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            // Call TakeDamage on the enemy's PhotonView using RPC
            enemy.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, damage);
            Debug.Log("We hit " + enemy.name);
        }
    }

    [PunRPC]
    public void TakeDamage(int damageAmount)
    {
        if (!pv.IsMine) return; 

        health -= damageAmount;
        Debug.Log("Player " + pv.ViewID + " Remaining Health: " + health);

        if (health <= 0)
        {
            
            pv.RPC("Die", RpcTarget.AllBuffered);
        }
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