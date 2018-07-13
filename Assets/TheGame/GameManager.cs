using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    #region Members

    public static GameManager Instance { get; private set; }

    public int playerAmount = 20;
    public int humanPlayerAmount = 0;
    public GameObject player;
    public PlayerControllerScriptable aiController;
    public PlayerControllerScriptable humenController;

    public int gameTime = 2 * 60;
    private float startTime;
    public Text timeText;
    private float timeRemain;

    public GameObject wall;
    public float horizontalSize = 160;
    public float verticalSize = 90;

    private GameObject _northWall;
    private GameObject _eastWall;
    private GameObject _southWall;
    private GameObject _westWall;

    private GameObject border;
    public List<PlayerScript> players = new List<PlayerScript>();
    private int aliveCount;

    #endregion

    public struct EvaluationData
    {
        public int rankInGame;
        public float timeRemain;
        public float timeSurvived;

        public EvaluationData(int rankInGame, float timeRemain, float timeSurvived)
        {
            this.rankInGame = rankInGame;
            this.timeRemain = timeRemain;
            this.timeSurvived = timeSurvived;
        }
    }

    public EvaluationData GetPlayerEvaluationData()
    {
        return new EvaluationData(aliveCount, timeRemain, gameTime - timeRemain);
    }

    public void RemovePlayer(PlayerScript player)
    {
        players.Remove(player);
    }

    private void CreateBorder()
    {
        border = new GameObject("border");
        float hv = verticalSize / 2;
        float hh = horizontalSize / 2;

        _northWall = GameObject.Instantiate(wall, new Vector3(0, hv, 0), wall.transform.rotation, border.transform);
        _northWall.transform.localScale = new Vector3(_northWall.transform.localScale.x * horizontalSize,
            _northWall.transform.localScale.y, _northWall.transform.localScale.z);

        _eastWall = GameObject.Instantiate(wall, new Vector3(-hh, 0, 0), wall.transform.rotation, border.transform);
        _eastWall.transform.localScale = new Vector3(_eastWall.transform.localScale.x,
            _eastWall.transform.localScale.y * verticalSize, _eastWall.transform.localScale.z);

        _southWall = GameObject.Instantiate(wall, new Vector3(0, -hv, 0), wall.transform.rotation, border.transform);
        _southWall.transform.localScale = new Vector3(_southWall.transform.localScale.x * horizontalSize,
            _southWall.transform.localScale.y, _southWall.transform.localScale.z);

        _westWall = GameObject.Instantiate(wall, new Vector3(hh, 0, 0), wall.transform.rotation, border.transform);
        _westWall.transform.localScale = new Vector3(_westWall.transform.localScale.x,
            _westWall.transform.localScale.y * verticalSize, _westWall.transform.localScale.z);
    }

    public void SetPlayerAmount(int amount)
    {
        playerAmount = amount;
        aliveCount = amount;
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
                GameObject playerClone = Instantiate(player);
                playerClone.transform.position = new Vector3(Random.Range(-hh, hh), Random.Range(-hv, hv));
                PlayerControllerScriptable controller = Instantiate(aiController);
                PlayerScript playerScript = playerClone.GetComponent<PlayerScript>();
                playerScript.controller = controller;
                playerClone.SetActive(true);
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
        //Observation.Instant.InitMaps();
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
            player.Restart();
            player.transform.position = new Vector3(Random.Range(-hh, hh), Random.Range(-hv, hv));
        }

        startTime = Time.timeSinceLevelLoad;

        Observation.Instant.InitMaps();
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
        if (Instance != null)
        {
            // TODO check why :( DO NOT remove from scene.
            Debug.LogError("More than one GameManager in the Scene.");
            return;
        }
        Instance = this;

        CreateBorder();
        if (EvolutionManager.Instance == null) //todo
        {
            SetPlayerAmount(playerAmount);
        }
        //makePlayers();
    }

    private void Start()
    {
        startTime = Time.timeSinceLevelLoad;
    }

    private void LateUpdate()
    {
        timeRemain = (gameTime - (Time.timeSinceLevelLoad - startTime));
        if (timeRemain <= 0)
        {
            EvolutionManager.Instance.EndTheGame();
        }
    }

    private void OnGUI()
    {
        timeText.text = ((int) (timeRemain)).ToString();
    }
}