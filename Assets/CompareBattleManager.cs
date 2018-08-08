using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CompareBattleManager : MonoBehaviour {

    public enum GroupName { None = 0, A = 1, B = 2}

    public static CompareBattleManager Instance
    {
        get;
        private set;
    }

    [SerializeField]
    float timeScale = 10;

    [SerializeField]
    int gameNumberToPlay = 10; //todo - change if necesery
    int gameCount = 0;

    [SerializeField]
    private Text group_A_victory_count_text;
    [SerializeField]
    private Text group_B_victory_count_text;

    [SerializeField]
    private Text GameCountText;

    public event System.Action EndOfGame;

    int group_A_victory_count;
    int group_B_victory_count;

    List<Genotype> group_A = new List<Genotype>();
    List<Genotype> group_B = new List<Genotype>();


    void Awake()
    {
        if (Instance != null)
        {
            // TODO check why...
            Debug.LogError("More than one CompareBattleManager in the Scene.");
            return;
        }
        Instance = this;

        //Load Game
        SceneManager.LoadScene("Game", LoadSceneMode.Additive);
    }


    private void Update()
    {
        Time.timeScale = timeScale;
    }

    private void Start()
    {
        group_A_victory_count = 0;
        group_B_victory_count = 0;
        group_A_victory_count_text.text = group_A_victory_count.ToString();
        group_B_victory_count_text.text = group_B_victory_count.ToString();
        gameCount = 1;
        GameCountText.text = gameCount.ToString();
        group_A = CreatGamePopulation(GameData.instance.group_A_data.genotypes, GameManager.Instance.playerAmount / 2);
        group_B = CreatGamePopulation(GameData.instance.group_B_data.genotypes, GameManager.Instance.playerAmount / 2);
        GameManager.Instance.RestartTheGame(group_A, group_B);
    }

    public void EndCompareGame()
    {
        //check which player win and increase victory_count
        foreach(PlayerScript player in GameManager.Instance.players)
        {
            if (player.isAlive)
            {
                if (player.group == GroupName.A)
                {
                    group_A_victory_count++;
                }
                else if (player.group == GroupName.B)
                {
                    group_B_victory_count++;
                }
            }
        }
        group_A_victory_count_text.text = group_A_victory_count.ToString();
        group_B_victory_count_text.text = group_B_victory_count.ToString();
        
        //increase game count
        gameCount++;
        GameCountText.text = gameCount.ToString();

        //if game count > games to play ==> save data to file end return to main menu
        if (gameCount > gameNumberToPlay)
        {
            SaveDataToFile();
            GameData.instance.BackToMainMenu();
        }

        GameManager.Instance.RestartTheGame(group_A, group_B);
    }

    private void SaveDataToFile()
    {
        string fileName = "CompareBetween2Groups" + System.DateTime.Now.ToString("yyyy_MM_dd_HH-mm-ss");

        string saveFolder = GameData.SAVE_DATA_DIRECTORY + "/";

        if (!Directory.Exists(saveFolder))
            Directory.CreateDirectory(saveFolder);

        string text = "Group A victory: " + group_A_victory_count + System.Environment.NewLine +
            "Group B victory: " + group_B_victory_count;

        File.WriteAllText(saveFolder + fileName + ".txt", text);
    }

    private List<Genotype> CreatGamePopulation(List<Genotype> group_from, int amount_to_create)
    {
        List<Genotype> group = new List<Genotype>();
        int i = 0;
        while (group.Count < amount_to_create)
        {
            group.Add(group_from[i]);
            i = (i + 1) % group_from.Count;
        }
        return group;
    }
}
