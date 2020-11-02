using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float maxJumpHeightVeloxity = 10.0f;
    private bool grounded = true;
    public float jumpForce = 10.0f;
    public float moveSpeed = 5.0f;
    public float maxMoveSpeed = 3.0f;
    private Rigidbody2D m_rb;
    // Start is called before the first frame update
    void Start()
    {
        m_rb = this.GetComponent<Rigidbody2D>();

    }

    // Update is called once per frame
    void Update()
    {
        Vector2 translation = Vector2.zero;

        if (grounded)
        {
            if (Input.GetAxis("Vertical") > 0)
            {

                translation += new Vector2(0, jumpForce * Time.deltaTime * Input.GetAxis("Vertical"));
                grounded = false;
            }
        }
        if (Input.GetAxis("Horizontal") != 0)
        {

            translation += new Vector2(moveSpeed * Time.deltaTime * Input.GetAxis("Horizontal"), 0);
        }
        m_rb.velocity += translation;
        if (m_rb.velocity.y > maxJumpHeightVeloxity) m_rb.velocity = new Vector2(m_rb.velocity.x, maxJumpHeightVeloxity);
        if (m_rb.velocity.x > maxMoveSpeed) m_rb.velocity = new Vector2(maxMoveSpeed, m_rb.velocity.y);
        if (m_rb.velocity.magnitude < 0.01f) transform.hasChanged = false;
        // transform.position += new Vector3(translation.x, translation.y, 0);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        grounded = true;
    }
}
