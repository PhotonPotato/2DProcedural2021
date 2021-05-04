using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public bool StartAtTop = false;
    public WorldGenerator WorldGenScript;

    //Movement modifiers
    public float speed;
    public float jumpForce;
    public float drag;
    public float gravityForce = 1;
    public float maxFallSpeed = 5;
    public float jumpGravity = .8f;
    public bool touchingGround = false;
    public bool canJump = false;

    float xInput = 0;

    public float xVel = 0;
    float yVel = 0;
    float gravityVel = 0;

    Rigidbody2D playerBody;

    void Start()
    {
        playerBody = this.gameObject.GetComponent<Rigidbody2D>();

        xVel = 0;
        yVel = 0;
        gravityVel = 0;
    }

    void FixedUpdate()
    {
        //Sideways movement
        xInput = Input.GetAxis("Horizontal");
        xVel = xInput * speed;
        xVel = Mathf.Clamp(xVel, -.3f, .3f);

        //Slow down if no input.
        if (xInput < .1f && xInput > -.1f) xVel *= drag;

        if (touchingGround == false)
        {
            gravityVel += gravityForce * Time.deltaTime;
            if (yVel > 0) gravityVel *= jumpGravity;
        }
        else
        {
            gravityVel = 0;
            yVel = 0;
        }

        yVel -= gravityVel;

        //Jump check
        canJump = touchingGround;
        if (Input.GetAxis("Jump") > .5f && canJump)
        {
            yVel = jumpForce;
        }

        if (yVel < maxFallSpeed) yVel = maxFallSpeed;

        playerBody.MovePosition(new Vector3(transform.position.x + xVel, transform.position.y + yVel, 0));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Ground") touchingGround = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground") touchingGround = false;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground") touchingGround = true;
    }
}
