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
    public PlayerControllerScriptable aiController;
    public PlayerControllerScriptable humenController;

    public int gameTime = 2 * 60;
    public Text timeText;
    private float timeRemain;

    public GameObject wall;
    public float horizontalSize = 160;
    public float verticalSize = 90;

    private GameObject northWall;
    private GameObject eastWall;
    private GameObject southWall;
    private GameObject westWall;

    private GameObject border;
    private List<PlayerScript> players = new List<PlayerScript>();
    #endregion

    public struct EvaluationData
    {
        int rankInGame;
        float timeRemain;
        float timeSurvived;

        public EvaluationData(int rankInGame, float timeRemain, float timeSurvived)
        {
            this.rankInGame = rankInGame;
            this.timeRemain = timeRemain;
            this.timeSurvived = timeSurvived;
        }
    }

    public EvaluationData GetPlayerEvaluationData(GameObject player)
    {
        return new EvaluationData(players.Capacity, timeRemain, gameTime - timeRemain);
    }

    public void RemovePlayer(PlayerScript player)
    {
        players.Remove(player);
    }

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

    public void SetPlayerAmount(int amount)
    {
        //Check arguments
        if (amount < 0) throw new System.Exception("Amount may not be less than zero.");

        if (amount == players.Count) return;

        if (amount > players.Count)
        {
            float hv = verticalSize / 2 - 1;
            float hh = horizontalSize / 2 - 1;
            //Add new players
            for (int toBeAdded = amount - players.Count; toBeAdded > 0; toBeAdded--)
            {
                GameObject playerClone = GameObject.Instantiate(player);
                playerClone.transform.position = new Vector3(Random.Range(-hh, hh), Random.Range(-hv, hv));
                PlayerControllerScriptable controller = aiController;
                PlayerScript playerScript = playerClone.GetComponent<PlayerScript>();
                playerScript.controller = aiController;
                players.Add(playerScript);
            }
        }
        else if (amount < players.Count)
        {
            //Remove existing players
            for (int toBeRemoved = players.Count - amount; toBeRemoved > 0; toBeRemoved--)
            {
                PlayerScript last = players[players.Count - 1];
                players.RemoveAt(players.Count - 1);

                Destroy(last.gameObject);
            }
        }
    }

    public IEnumerator<PlayerScript> GetPlayerEnumerator()
    {
        for (int i = 0; i < players.Count; i++)
            yield return players[i];
    }

    public void Restart()
    {
        float hv = verticalSize / 2 - 1;
        float hh = horizontalSize / 2 - 1;
        foreach (PlayerScript player in players)
        {
            player.gameObject.SetActive(true);
            player.transform.position = new Vector3(Random.Range(-hh, hh), Random.Range(-hv, hv));
        }
    }

    private void makePlayers()
    {
        //float hv = verticalSize / 2 - 1;
        //float hh = horizontalSize / 2 - 1;

        //int aiAmount = playerAmount - humanPlayerAmount;

        //for (int i = 0; i < aiAmount; i++)
        //{
        //    Vector3 position = new Vector3(Random.Range(-hh, hh), Random.Range(-hv, hv));
        //    GameObject playerClone = GameObject.Instantiate(player);
        //    playerClone.transform.position = position;

        //    playerClone.GetComponent<PlayerScript>().controller = aiController;

        //    //TODO: add ai controller script
        //    players.Add(playerClone);
        //}

        //for (int i = 0; i < humanPlayerAmount; i++)
        //{
        //    Vector3 position = new Vector3(Random.Range(-hh, hh), Random.Range(-hv, hv));
        //    GameObject playerClone = GameObject.Instantiate(player);
        //    playerClone.transform.position = position;

        //    playerClone.GetComponent<PlayerScript>().controller = humenController;

        //    //todo: make camera for each player.
        //    players.Add(playerClone);
        //}
    }

    private void Awake()
    {
        createBorder();
        SetPlayerAmount(playerAmount);
        //makePlayers();
    }

    private void Update()
    {
        timeRemain = (gameTime - Time.timeSinceLevelLoad);
    }

    private void OnGUI()
    {
        timeText.text = ((int)(timeRemain)).ToString();
    }
}
