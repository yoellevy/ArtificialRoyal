using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameData : MonoBehaviour {

    public static GameData instance = null;

    public const string SAVE_DATA_DIRECTORY = "saves";

    public const string GENOTYPES_FOLDER_NAME = "genotypes";
    public const string GENOTYPE_SUFFIX = "genotype";
    public const string NEURAL_NETWORK_SUFFIX = "nnd";
    public const string COMPARE_BATTLE_GROUPS_FOLDER_NAME = "CompareBattle";
    public const string GROUP_A_GENOTYPES_FOLDER_NAME = "Group_A";
    public const string GROUP_B_GENOTYPES_FOLDER_NAME = "Group_B";

    public class GroupData
    {
        public List<Genotype> genotypes = new List<Genotype>();
        public bool useRNN = true;
        public uint[] Topology;
    }

    public GroupData group_A_data = new GroupData();
    public GroupData group_B_data = new GroupData();

    public bool toAddHumanPlayer = false;

    private void Awake()
    {
        //Check if instance already exists
        if (instance == null)

            //if not, set instance to this
            instance = this;

        //If instance already exists and it's not this:
        else if (instance != this)

            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);
    }

    public void LoadGroupData()
    {
        LoadSomeGenotypes(GENOTYPES_FOLDER_NAME, group_A_data);
        LoadNNDFile(GENOTYPES_FOLDER_NAME, group_A_data);
    }

    private void LoadSomeGenotypes(string genotype_folder, GroupData groupData)
    {
        if (!Directory.Exists(genotype_folder)) throw new DirectoryNotFoundException(string.Format("Can't find {0} Directory", GENOTYPES_FOLDER_NAME));

        string[] genotype_files = Directory.GetFiles(genotype_folder, "*." + GENOTYPE_SUFFIX);

        if (genotype_files.Length == 0) throw new FileNotFoundException(string.Format("Can't find files with suffix \"{0}\"", GENOTYPE_SUFFIX));

        foreach (string file in genotype_files)
        {
            groupData.genotypes.Add(Genotype.LoadFromFile(file));
        }
    }

    private void LoadNNDFile(string nnd_folder, GroupData groupData)
    {
        string[] nnd_files = Directory.GetFiles(nnd_folder, "*." + NEURAL_NETWORK_SUFFIX);

        if (nnd_files.Length == 0) throw new FileNotFoundException(string.Format("Can't find file with suffix \"{0}\"", NEURAL_NETWORK_SUFFIX));
        if (nnd_files.Length > 1) throw new FileNotFoundException(string.Format("Too many files with suffix \"{0}\"", NEURAL_NETWORK_SUFFIX));

        string data = File.ReadAllText(nnd_files[0]);

        List<float> parameters = new List<float>();
        string[] paramStrings = data.Split(';');

        groupData.useRNN = paramStrings[0].Equals("1");

        List<uint> topology_list = new List<uint>();

        for (int i = 1; i < paramStrings.Length; i++)
        {
            uint parsed;
            if (!uint.TryParse(paramStrings[i], out parsed)) throw new ArgumentException("The file at given file path does not contain a valid topology serialisation.");
            topology_list.Add(parsed);
        }

        groupData.Topology = topology_list.ToArray();
    }

    public void LoadCompareBattleData()
    {
        
        string group_A_location = COMPARE_BATTLE_GROUPS_FOLDER_NAME + "\\" + GROUP_A_GENOTYPES_FOLDER_NAME;
        string group_B_location = COMPARE_BATTLE_GROUPS_FOLDER_NAME + "\\" + GROUP_B_GENOTYPES_FOLDER_NAME;
        LoadSomeGenotypes(group_A_location, group_A_data);
        LoadNNDFile(group_A_location, group_A_data);
        LoadSomeGenotypes(group_B_location, group_B_data);
        LoadNNDFile(group_B_location, group_B_data);
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1;
        toAddHumanPlayer = false;
        SceneManager.LoadScene("MainMenu");
    }
}
