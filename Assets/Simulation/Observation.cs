using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AngleQuant { None = 0, Right = 1, UpRight = 2, Up = 3, UpLeft = 4, Left = 5, DownLeft = 6, Down = 7, DownRight = 8 }

public class Observation : MonoBehaviour
{
    public float maxObservationDistance = 7;
    public static Observation Instant = null;
    GameObject[] players = null;
    GameObject[] bullets = null;
    int outputSize;

    class DistanceAndAngle
    {
        public float distance;
        /// <summary>
        ///         
        /// </summary>
        public AngleQuant angle;

        public override string ToString()
        {
            return "dis: " + distance.ToString() + ", angle: " + angle.ToString();
        }
    }

    Dictionary<int, float[]> PlayerToPlayerObservation;

    DistanceAndAngle[,] PlayerToPlayerDistance;
    DistanceAndAngle[,] PlayerToBulletDistance;

    #region Observation Flags
    public bool observeBullets = true, observePlayers = true, observeWalls = false;
    #endregion

    private void Awake()
    {
        if (Instant == null)
        {
            Instant = this;
        }
        else if (Instant != this)
        {
            Destroy(this.gameObject);
        }
    }
    // Use this for initialization
    void Start()
    {
        outputSize = 0;
        if (observeBullets)
            outputSize += 8;
        if (observePlayers)
            outputSize += 8;
        if (observeWalls)
            outputSize += 8;
    }

    bool flag = true;
    // Update is called once per frame
    void Update()
    {
        //TODO:
        if (flag)
        {
            players = GameObject.FindGameObjectsWithTag("PlayerTag");
            PlayerToPlayerObservation = new Dictionary<int, float[]>();
            for (int i = 0; i < players.Length; i++)               
                PlayerToPlayerObservation[players[i].GetComponent<PlayerScript>().id] = new float[8];

            PlayerToPlayerDistance = new DistanceAndAngle[players.Length, players.Length];
            flag = false;
        }
        bullets = GameObject.FindGameObjectsWithTag("Bullet");
        calculateDistanceFromPlayers();
        //calculateDistanceFromBullet();
    }
    private void OnGUI()
    {
        //for (int i = 0; i < 8; i++)
        //{
        //    GUI.Label(new Rect(10, 10+i*20, 100, 20), PlayerToPlayerObservation[0][i].ToString(), new GUIStyle { fontSize = 20 });
        //}
        ////GUI.Label(new Rect(10, 10, 100, 20), PlayerToPlayerObservation[0].ToString(), new GUIStyle { fontSize = 20 });
        //GUI.Label(new Rect(150, 50, 100, 20), MathHelper.GetAngle(Vector3.right, players[1].transform.position - players[0].transform.position).ToString(), new GUIStyle { fontSize = 20 });
    }

    private void calculateDistanceFromPlayers()
    {
        foreach (var item in PlayerToPlayerObservation)     
            for (int i = 0; i < 8; i++)            
                item.Value[i] = maxObservationDistance;
          
        for (int i = 0; i < players.Length; i++)
        {
            int p1id = players[i].GetComponent<PlayerScript>().id;
            for (int j = i+1; j < players.Length; j++)
            {
                int p2id = players[j].GetComponent<PlayerScript>().id;

                float currDistance = Vector2.Distance(players[i].transform.position, players[j].transform.position);
                if (currDistance < maxObservationDistance)
                {                    
                    AngleQuant currAngle = calculateAngle(players[i].transform.position, players[j].transform.position);
                    AngleQuant opAngle = oppositeAngle(currAngle);
                    PlayerToPlayerObservation[i][(int)currAngle - 1] = currDistance;
                    PlayerToPlayerObservation[j][(int)opAngle - 1] = currDistance;
                    if (i == 0)
                        print(currAngle);
                }
            }
        }
    }

    private void calculateDistanceFromBullet()
    {
        for (int i = 0; i < players.Length; i++)
        {
            for (int j = 0; j < bullets.Length; j++)
            { PlayerToBulletDistance[i, j] = new DistanceAndAngle { distance = maxObservationDistance, angle = 0 }; }
        }

        for (int i = 0; i < players.Length; i++)
        {
            for (int j = 0; j < bullets.Length; j++)
            {
                float currDistance = Vector2.Distance(players[i].transform.position, bullets[j].transform.position);

                AngleQuant currAngle = calculateAngle(players[i].transform.position, bullets[j].transform.position);
                PlayerToBulletDistance[i, j] = new DistanceAndAngle { distance = currDistance, angle = currAngle };
            }
        }
    }

    private AngleQuant oppositeAngle(AngleQuant currAngle)
    {
        switch (currAngle)
        {
            case AngleQuant.Right:
                return AngleQuant.Left;
                break;
            case AngleQuant.UpRight:
                return AngleQuant.DownLeft;
                break;
            case AngleQuant.Up:
                return AngleQuant.Down;
                break;
            case AngleQuant.UpLeft:
                return AngleQuant.DownRight;
                break;
            case AngleQuant.Left:
                return AngleQuant.Right;
                break;
            case AngleQuant.DownLeft:
                return AngleQuant.UpRight;
                break;
            case AngleQuant.Down:
                return AngleQuant.Up;
                break;
            case AngleQuant.DownRight:
                return AngleQuant.UpLeft;
                break;
            case AngleQuant.None:
            default:
                return AngleQuant.None;
                break;
        }
    }

    private AngleQuant calculateAngle(Vector3 position1, Vector3 position2)
    {
        float angle = MathHelper.GetAngle(Vector3.right, position2 - position1);

        if (angle <= 22.5f || angle >= 337.5f)
            return AngleQuant.Right;
        float prevAngle = 22.5f;
        int i = 2;
        for (float currAngle = prevAngle; currAngle < 360; currAngle += 45, i++)
            if (angle >= currAngle && angle <= currAngle + 45)
            {
                return (AngleQuant)Enum.ToObject(typeof(AngleQuant), i);
            }

        return AngleQuant.None;
    }

    /// <summary>
    /// compute and return observation of player with specific id
    /// the size of the obbservation depends on the flags: observeBullets, observePlayers, observeWalls.
    /// 
    /// 
    /// </summary>
    /// <param name="id">id of player to return the observation of</param>
    /// <returns>obseration </returns>
    public float[] getObservationOfPlayerID(int id)
    {
        //for now only player observation 
        //TODO: extend to all observation
        return PlayerToPlayerObservation[id];
    }
}
