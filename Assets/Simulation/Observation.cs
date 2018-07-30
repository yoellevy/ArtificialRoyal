using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AngleQuant { None = 0, Right = 1, UpRight = 2, Up = 3, UpLeft = 4, Left = 5, DownLeft = 6, Down = 7, DownRight = 8 }

public class Observation : MonoBehaviour
{
    public double maxObservationDistance = 7;
    public static Observation Instant = null;
    List<PlayerScript> players = null;
    GameObject[] bullets = null;

    int outputSectionSize = 8;
    int outputSize;

    class DistanceAndAngle
    {
        public double distance;
        /// <summary>
        ///         
        /// </summary>
        public AngleQuant angle;

        public override string ToString()
        {
            return "dis: " + distance.ToString() + ", angle: " + angle.ToString();
        }
    }

    Dictionary<int, double[]> PlayerToPlayerObservation;
    Dictionary<int, double[]> PlayerToBulletObservation;
    Dictionary<int, double[]> PlayerToWallObservation;
    //todo - for player to wall distance maybe we can use "ColliderDistance2D"

    DistanceAndAngle[,] PlayerToPlayerDistance;
    DistanceAndAngle[,] PlayerToBulletDistance;
    DistanceAndAngle[,] PlayerToWallDistance;

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
            outputSize += outputSectionSize;
        if (observePlayers)
            outputSize += outputSectionSize;
        if (observeWalls)
            outputSize += 4;
        //InitMaps();
    }

    bool flag = true;
    // Update is called once per frame
    void Update()
    {
        if (flag)
        {
            flag = false;
            //InitMaps(); //must to be after start
        }
        
        if (observePlayers)
            calculateDistanceFromPlayers();
        if (observeBullets)
        {
            bullets = GameObject.FindGameObjectsWithTag("Bullet"); //GameManager.Instance.bullets;
            calculateDistanceFromBullet();
        }
        if (observeWalls)
            calculateDistanceFromWall();
    }

    public void InitMaps()
    {
        //players = GameObject.FindGameObjectsWithTag("Player");
        players = GameManager.Instance.players;
        PlayerToPlayerObservation = new Dictionary<int, double[]>();
        PlayerToBulletObservation = new Dictionary<int, double[]>();
        PlayerToWallObservation = new Dictionary<int, double[]>();
        for (int i = 0; i < players.Count; i++)
        {
            int currId = players[i].id;
            PlayerToPlayerObservation[currId] = new double[outputSectionSize];
            PlayerToBulletObservation[currId] = new double[outputSectionSize];
            PlayerToWallObservation[currId] = new double[4];
        }
        PlayerToPlayerDistance = new DistanceAndAngle[players.Count, players.Count];
    }

    //private void OnGUI()
    //{
    //    for (int i = 0; i < 8; i++)
    //    {
    //        GUI.Label(new Rect(10, 10 + i * 20, 100, 20), PlayerToPlayerObservation[0][i].ToString(), new GUIStyle { fontSize = 20 });
    //    }
    //    for (int i = 0; i < 8; i++)
    //    {
    //        GUI.Label(new Rect(300, 10 + i * 20, 100, 20), PlayerToBulletObservation[0][i].ToString(), new GUIStyle { fontSize = 20 });
    //    }
    //}

    private void calculateDistanceFromPlayers()
    {
        foreach (var item in PlayerToPlayerObservation)
            for (int i = 0; i < outputSectionSize; i++)
                item.Value[i] = 0;

        for (int i = 0; i < players.Count; i++)
        {
            int p1id = players[i].id;
            for (int j = i + 1; j < players.Count; j++)
            {
                int p2id = players[j].id;

                double currDistance = Vector2.Distance(players[i].transform.position, players[j].transform.position);
                if (currDistance < maxObservationDistance)
                {
                    AngleQuant currAngle = CalculateAngle(players[i].transform.position, players[j].transform.position);
                    AngleQuant opAngle = oppositeAngle(currAngle);
                    double p2pDist = (maxObservationDistance - currDistance) / maxObservationDistance;
                    PlayerToPlayerObservation[p1id][(int)currAngle - 1] = p2pDist;
                    PlayerToPlayerObservation[p2id][(int)opAngle - 1] = p2pDist;
                }
            }
        }
    }

    private void calculateDistanceFromBullet()
    {
        foreach (var item in PlayerToBulletObservation)
            for (int i = 0; i < outputSectionSize; i++)
                item.Value[i] = 0;

        for (int i = 0; i < players.Count; i++)
        {
            int p1id = players[i].id;
            foreach (var item in bullets)
            {
                //todo - each player needs to ignore his bullets. Or at least we must to set bullet direction somwehere
                if (item.GetComponent<BulletData>().playerScript.id != players[i].id) //todo - should I comper the id or the script without the id?
                {
                    double currDistance = Vector2.Distance(item.transform.position, players[i].transform.position);
                    if (currDistance < maxObservationDistance)
                    {
                        AngleQuant currAngle = CalculateAngle(players[i].transform.position, item.transform.position);

                        double p2bDist = (maxObservationDistance - currDistance) / maxObservationDistance;
                        PlayerToBulletObservation[p1id][(int)currAngle - 1] = Math.Max(p2bDist, PlayerToBulletObservation[p1id][(int)currAngle - 1]); //todo - (from Omer to Yoel) - where is the minimum of bunch of bullets? I'm not sure that I'm understand this code.
                    }
                }
            }
        }
    }

    private void calculateDistanceFromWall()
    {
        //"ColliderDistance2D"
        for (int i = 0; i < players.Count; i++)
        {
            int p1id = players[i].id;
            double dist;
            double p2wDist;
            dist = Math.Min(players[i].GetComponent<Collider2D>().Distance(GameManager.Instance._northWall.GetComponent<Collider2D>()).distance, maxObservationDistance);
            p2wDist = (maxObservationDistance - dist) / maxObservationDistance;
            PlayerToWallObservation[p1id][0] = p2wDist;
            dist = Math.Min(players[i].GetComponent<Collider2D>().Distance(GameManager.Instance._eastWall.GetComponent<Collider2D>()).distance, maxObservationDistance);
            p2wDist = (maxObservationDistance - dist) / maxObservationDistance;
            PlayerToWallObservation[p1id][1] = p2wDist;
            dist = Math.Min(players[i].GetComponent<Collider2D>().Distance(GameManager.Instance._southWall.GetComponent<Collider2D>()).distance, maxObservationDistance);
            p2wDist = (maxObservationDistance - dist) / maxObservationDistance;
            PlayerToWallObservation[p1id][2] = p2wDist;
            dist = Math.Min(players[i].GetComponent<Collider2D>().Distance(GameManager.Instance._westWall.GetComponent<Collider2D>()).distance, maxObservationDistance);
            p2wDist = (maxObservationDistance - dist) / maxObservationDistance;
            PlayerToWallObservation[p1id][3] = p2wDist;

        }
    }

    private AngleQuant oppositeAngle(AngleQuant currAngle)
    {
        switch (currAngle)
        {
            case AngleQuant.Right:
                return AngleQuant.Left;
            case AngleQuant.UpRight:
                return AngleQuant.DownLeft;
            case AngleQuant.Up:
                return AngleQuant.Down;
            case AngleQuant.UpLeft:
                return AngleQuant.DownRight;
            case AngleQuant.Left:
                return AngleQuant.Right;
            case AngleQuant.DownLeft:
                return AngleQuant.UpRight;
            case AngleQuant.Down:
                return AngleQuant.Up;
            case AngleQuant.DownRight:
                return AngleQuant.UpLeft;
            case AngleQuant.None:
            default:
                return AngleQuant.None;
        }
    }

    private AngleQuant CalculateAngle(Vector3 position1, Vector3 position2)
    {
        double angle = MathHelper.GetAngle(Vector3.right, position2 - position1);

        if (angle <= 22.5f || angle >= 337.5f)
            return AngleQuant.Right;
        double prevAngle = 22.5f;
        int i = 2;
        for (double currAngle = prevAngle; currAngle < 360; currAngle += 45, i++)
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
    public double[] GetObservationOfPlayerId(int id)
    {
        
        //for now only player observation 
        //TODO: extend to all observation
        double[] outputObservation = new double[outputSize];
        int idx = 0;
        if (observePlayers)
        {
            PlayerToPlayerObservation[id].CopyTo(outputObservation, idx);
            idx += outputSectionSize;
        }

        if (observeBullets)
        {
            PlayerToBulletObservation[id].CopyTo(outputObservation, idx);
            idx += outputSectionSize;
        }

        if (observeWalls)
        {
            PlayerToWallObservation[id].CopyTo(outputObservation, idx);
            idx += 4;
        }
        return outputObservation;
    }
}
