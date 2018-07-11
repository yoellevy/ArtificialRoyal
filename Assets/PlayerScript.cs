using System;
using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    private static int idGenerator = 0;
    private static int NextID
    {
        get { return idGenerator++; }
    }

    public int id;
    [SerializeField]
    PlayerControllerScriptable controller;

    [SerializeField]
    float speed = 5;

    [SerializeField] private Transform _bullet;
    [SerializeField]
    float bulletBaseSpeed = 7;
    [SerializeField]
    float shotInterval = 0.25f;
    [SerializeField]
    float bulletLifeTime = 2;

    float timePass = 0;
    Animator m_animator;
    Rigidbody2D m_rigibody;
    /// <summary>
    /// The underlying AI agent of this car.
    /// </summary>
    public Agent Agent
    {
        get;
        set;
    }
    
    
    
    // Evaluation functions.
    public EvaluationFunctions EvaluationFunctions { get; set; }
    public float[] Weights { get; set; }
    public int Rank { get; set; }
    public int KillCount { get; set; }
    public float SurvivelTime { get; set; }

    
    // Use this for initialization
    private void Awake()
    {
<<<<<<< HEAD
        id = NextID;
    }

    void Start()
    {    
=======
        Weights = new[]{1f/3f,1f/3f,1f/3f};
>>>>>>> 4b6fbd5d939e307772fa9539a5a2bffbc6bc71a3
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
        m_rigibody.velocity = controller.getMove().normalized * speed;

        bool isLeft = m_rigibody.velocity.x < 0,
            isRight = m_rigibody.velocity.x > 0,
            isUp = m_rigibody.velocity.y > 0,
            isDown = m_rigibody.velocity.y < 0;

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
        Vector2 shotSpeed = controller.getShot();

        if (shotSpeed.x != 0 || shotSpeed.y != 0)
        {
            timePass = 0;
            Transform currBullet = Instantiate(_bullet, transform.position, Quaternion.identity);
            currBullet.GetComponent<Rigidbody2D>().velocity = calculateBulletSpeed(shotSpeed.x, shotSpeed.y);
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
