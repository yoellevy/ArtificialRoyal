using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    #region Members
    public static GameManager Instance
    {
        get;
        private set;
    }

    public int playerAmount = 20;
    public int humanPlayerAmount = 0;
    public GameObject player;

    public int gameTime = 2 * 60;
    public Text timeText;

    public GameObject wall;
    public float horizontalSize = 160;
    public float verticalSize = 90;

    private GameObject northWall;
    private GameObject eastWall;
    private GameObject southWall;
    private GameObject westWall;

    private GameObject border;
    private List<GameObject> players = new List<GameObject>();
    #endregion

    private void createBorder()
    {
        border = new GameObject("border");
        float hv = verticalSize / 2;
        float hh = horizontalSize / 2;

        northWall = GameObject.Instantiate(wall, new Vector3(0, hv, 0), wall.transform.rotation, border.transform);
        northWall.transform.localScale = new Vector3(northWall.transform.localScale.x * horizontalSize, northWall.transform.localScale.y, northWall.transform.localScale.z);

        eastWall = GameObject.Instantiate(wall, new Vector3(-hh, 0, 0), wall.transform.rotation, border.transform);
        eastWall.transform.localScale = new Vector3(eastWall.transform.localScale.x, eastWall.transform.localScale.y * verticalSize, eastWall.transform.localScale.z);

        southWall = GameObject.Instantiate(wall, new Vector3(0, -hv, 0), wall.transform.rotation, border.transform);
        southWall.transform.localScale = new Vector3(southWall.transform.localScale.x * horizontalSize, southWall.transform.localScale.y, southWall.transform.localScale.z);

        westWall = GameObject.Instantiate(wall, new Vector3(hh, 0, 0), wall.transform.rotation, border.transform);
        westWall.transform.localScale = new Vector3(westWall.transform.localScale.x, westWall.transform.localScale.y * verticalSize, westWall.transform.localScale.z);
    }

    private void makePlayers()
    {
        float hv = verticalSize / 2 - 1;
        float hh = horizontalSize / 2 - 1;

        int aiAmount = playerAmount - humanPlayerAmount;

        for (int i = 0; i < aiAmount; i++)
        {
            Vector3 position = new Vector3(Random.Range(-hh, hh), Random.Range(-hv, hv));
            GameObject playerClone = GameObject.Instantiate(player);
            playerClone.transform.position = position;

            //TODO: add ai controller script
            players.Add(playerClone);
        }

        for (int i = 0; i < humanPlayerAmount; i++)
        {
            Vector3 position = new Vector3(Random.Range(-hh, hh), Random.Range(-hv, hv));
            GameObject playerClone = GameObject.Instantiate(player);
            playerClone.transform.position = position;

            //TODO: add human #i controller script
            //todo: make camera for each player.
            players.Add(playerClone);
        }
    }

    private void Awake()
    {
        createBorder();
        makePlayers();
    }

    private void OnGUI()
    {
        timeText.text = ((int)(gameTime - Time.timeSinceLevelLoad)).ToString();
    }
}
