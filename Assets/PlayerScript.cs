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
    public PlayerControllerScriptable controller;

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
    /// The underlying AI agent of this player.
    /// </summary>
    public Agent PlayerAgent
    {
        get;
        set;
    }
    
    
    
    // Evaluation functions.
    public EvaluationFunctions EvaluationFunction { get; set; }
    public float[] Weights { get; set; }
    public int Rank { get; set; }
    public int KillCount { get; set; }
    public float SurvivelTime { get; set; }
    public bool isAlive;

    [SerializeField]
    private bool isHumanHelper = false;

    
    // Use this for initialization
    private void Awake()
    {
        EvaluationFunction = EvaluationFunctions.LinearComposition; //todo (Ori/Reshef, please say something - from Omer)
        id = NextID;
    }

    void Start()
    {    

        Weights = new[]{1f/3f,1f/3f,1f/3f};

        m_animator = GetComponent<Animator>();
        m_rigibody = GetComponent<Rigidbody2D>();

        isAlive = true;
        controller.Init(this);
    }

    private void FixedUpdate()
    {
        controller.CalculateNextAction();
        movePlayer();
        shootBullet();
    }

    void movePlayer()
    {
        m_rigibody.velocity = controller.GetMove().normalized * speed;

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
        Vector2 shotSpeed = controller.GetShot();

        if (shotSpeed.x != 0 || shotSpeed.y != 0)
        {
            timePass = 0;
            Transform currBullet = Instantiate(_bullet, transform.position, Quaternion.identity, GameManager.Instance.transform);
            
            currBullet.GetComponent<Rigidbody2D>().velocity = calculateBulletSpeed(shotSpeed.x, shotSpeed.y);
            BulletData bdata = currBullet.GetComponent<BulletData>();
            if (bdata == null)
            {
                bdata = currBullet.gameObject.AddComponent<BulletData>();
            }
            currBullet.GetComponent<BulletData>().playerScript = this;
            //GameManager.Instance.bullets.Add(bdata);
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

    public void EvalSelf()
    {
        UpdatePlayerData();
        PlayerAgent.Genotype.Evaluation = EvaluationFunctionsImplementaion.EvalPlayer(this);
    }

    private void UpdatePlayerData()
    {
        GameManager.EvaluationData ed = GameManager.Instance.GetPlayerEvaluationData();
        Rank = ed.rankInGame;
        SurvivelTime = ed.timeSurvived;
    }

    private void Die(PlayerScript otherPlayer = null)
    {
        if (otherPlayer != null)
        {
            //update data for the shooter:
            otherPlayer.KillCount++;
        }

        if (!isHumanHelper)
        {
            //remove this player
            this.gameObject.SetActive(false);

            //update data for this player:
            isAlive = false;
            EvalSelf();
            PlayerAgent.Kill();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            // TODO this is causes bugs.
            PlayerScript otherPlayer = collision.GetComponent<BulletData>().playerScript;
            if (otherPlayer != this)
            {
                //remove bullet
                Destroy(collision.gameObject);

                Die(otherPlayer);               
            }
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            Die();
        }
    }

    public void Restart()
    {
        isAlive = true;
        PlayerAgent.Reset();
        this.gameObject.SetActive(true);
    }

    public double[] getPlayerObservation()
    {
        double[] observation = Observation.Instant.GetObservationOfPlayerId(id);
        double[] myObservation = new double[observation.Length + 1];
        observation.CopyTo(myObservation, 0);
        myObservation[myObservation.Length - 1] = shotInterval - timePass;
        return myObservation;
    }
}
