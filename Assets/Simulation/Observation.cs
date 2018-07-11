using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AngleQuant { None = 0, Right = 1, UpRight = 2, Up = 3, UpLeft = 4, Left = 5, DownLeft = 6, Down = 7, DownRight = 8 }

public class Observation : MonoBehaviour
{
    public double maxObservationDistance = 7;
    public static Observation Instant = null;
    GameObject[] players = null;
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
            outputSize += outputSectionSize;
        if (observePlayers)
            outputSize += outputSectionSize;
        if (observeWalls)
            outputSize += outputSectionSize;
    }

    bool flag = true;
    // Update is called once per frame
    void Update()
    {
        //TODO:
        if (flag)
        {
            players = GameObject.FindGameObjectsWithTag("PlayerTag");
            PlayerToPlayerObservation = new Dictionary<int, double[]>();
            PlayerToBulletObservation = new Dictionary<int, double[]>();
            for (int i = 0; i < players.Length; i++)
            {
                int currId = players[i].GetComponent<PlayerScript>().id;
                PlayerToPlayerObservation[currId] = new double[outputSectionSize];
                PlayerToBulletObservation[currId] = new double[outputSectionSize];
            }

            PlayerToPlayerDistance = new DistanceAndAngle[players.Length, players.Length];
            flag = false;
        }
        bullets = GameObject.FindGameObjectsWithTag("Bullet");
        calculateDistanceFromPlayers();
        calculateDistanceFromBullet();
    }

    private void OnGUI()
    {
        for (int i = 0; i < 8; i++)
        {
            GUI.Label(new Rect(10, 10 + i * 20, 100, 20), PlayerToPlayerObservation[0][i].ToString(), new GUIStyle { fontSize = 20 });
        }
        for (int i = 0; i < 8; i++)
        {
            GUI.Label(new Rect(300, 10 + i * 20, 100, 20), PlayerToBulletObservation[0][i].ToString(), new GUIStyle { fontSize = 20 });
        }
        ////GUI.Label(new Rect(10, 10, 100, 20), PlayerToPlayerObservation[0].ToString(), new GUIStyle { fontSize = 20 });
        //GUI.Label(new Rect(150, 50, 100, 20), MathHelper.GetAngle(Vector3.right, players[1].transform.position - players[0].transform.position).ToString(), new GUIStyle { fontSize = 20 });
    }

    private void calculateDistanceFromPlayers()
    {
        foreach (var item in PlayerToPlayerObservation)
            for (int i = 0; i < outputSectionSize; i++)
                item.Value[i] = maxObservationDistance;

        for (int i = 0; i < players.Length; i++)
        {
            int p1id = players[i].GetComponent<PlayerScript>().id;
            for (int j = i + 1; j < players.Length; j++)
            {
                int p2id = players[j].GetComponent<PlayerScript>().id;

                double currDistance = Vector2.Distance(players[i].transform.position, players[j].transform.position);
                if (currDistance < maxObservationDistance)
                {
                    AngleQuant currAngle = calculateAngle(players[i].transform.position, players[j].transform.position);
                    AngleQuant opAngle = oppositeAngle(currAngle);
                    PlayerToPlayerObservation[p1id][(int)currAngle - 1] = currDistance;
                    PlayerToPlayerObservation[p2id][(int)opAngle - 1] = currDistance;
                }
            }
        }
    }

    private void calculateDistanceFromBullet()
    {
        foreach (var item in PlayerToBulletObservation)
            for (int i = 0; i < outputSectionSize; i++)
                item.Value[i] = maxObservationDistance;

        for (int i = 0; i < players.Length; i++)
        {
            int p1id = players[i].GetComponent<PlayerScript>().id;
            foreach (var item in bullets)
            {
                double currDistance = Vector2.Distance(item.transform.position, players[i].transform.position);
                if (currDistance < maxObservationDistance)
                {
                    AngleQuant currAngle = calculateAngle(players[i].transform.position, item.transform.position);
                    PlayerToBulletObservation[p1id][(int)currAngle - 1] = currDistance;
                }
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
    public double[] getObservationOfPlayerID(int id)
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

        return outputObservation;
    }
}
