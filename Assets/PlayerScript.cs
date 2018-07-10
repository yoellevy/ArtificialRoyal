using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{

    [SerializeField]
    float speed = 5;

    [SerializeField]
    Transform bullet;
    [SerializeField]
    float bulletBaseSpeed = 7;
    [SerializeField]
    float shotInterval = 0.25f;
    [SerializeField]
    float bulletLifeTime = 2;

    float timePass = 0;
    Animator m_animator;
    Rigidbody2D m_rigibody;
    // Use this for initialization
    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_rigibody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        movePlayer();
        shootBullet();
    }

    void movePlayer()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        y = (Input.GetKey("w") ? 1 : Input.GetKey("s") ? -1 : 0) * speed;
        x = (Input.GetKey("d") ? 1 : Input.GetKey("a") ? -1 : 0) * speed;

        bool isLeft = x < 0, isRight = x > 0, isUp = y > 0, isDown = y < 0;

        m_rigibody.velocity = new Vector2(x, y);

        m_animator.SetBool("isUp", isUp);
        m_animator.SetBool("isDown", isDown);
        m_animator.SetBool("isRight", isRight);
        m_animator.SetBool("isLeft", isLeft);
    }

    void shootBullet()
    {
        timePass += Time.deltaTime;
        if (shotInterval - timePass > 0)
            return;
        float x = Input.GetAxis("HorizontalShot"),
              y = Input.GetAxis("VerticalShot");
        if (x != 0 || y != 0)
        {
            timePass = 0;
            Transform currBullet = Instantiate(bullet, transform.position, Quaternion.identity);
            currBullet.GetComponent<Rigidbody2D>().velocity = calculateBulletSpeed(x, y);
            Destroy(currBullet.gameObject, bulletLifeTime);
        }
    }

    private Vector2 calculateBulletSpeed(float x, float y)
    {
        Vector2 speed = ((new Vector2(x, y).normalized) * bulletBaseSpeed) + m_rigibody.velocity; ;

        if (speed.SqrMagnitude() < bulletBaseSpeed)
            speed = speed.normalized * bulletBaseSpeed;
        return speed;
    }
}
