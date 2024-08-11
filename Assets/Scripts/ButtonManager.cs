using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    protected static int playerCount = 2;
    protected static bool[] playerIn = new bool[4] { true, true, false, false };
    protected static int curPlayer;
    public TextMeshProUGUI playerText;
    protected static int rainPh = 5;
    protected static int rain2Ph = 3;
    protected static int turnCount = 5;
    protected static int turn = 1;

    protected enum Difficulty { Easy, Normal, Hard };
    protected static Difficulty curDifficulty;
    public TextMeshProUGUI difficultyText;
    protected static bool boardState; //Dice or move

    public GameObject canvas;
    public GameObject[] playerPresets = new GameObject[4];
    private Vector3 central = new Vector3(11, -1, 120);
    protected static GameObject[] players;

    public GameObject toggleGroup;
    public GameObject rulesCanvas;

    protected static bool end = false;

    void Start() { curDifficulty = (Difficulty) 1; }

    public void ChangeCount()
    {
        playerCount++;
        if (playerCount > 4) { playerCount = 2; }
        playerText.text = playerCount.ToString();
        for (int i = 0; i < 4; i++) { playerIn[i] = i < playerCount; }
    }

    public void ChangeDifficulty()
    {
        curDifficulty++;
        if ((int)curDifficulty > 2) { curDifficulty = 0; }
        difficultyText.text = curDifficulty.ToString();

        switch((int)curDifficulty)
        {
            case 2:
                rainPh = 2;
                rain2Ph = 1;
                break;
            case 1:
                rainPh = 5;
                rain2Ph = 3;
                break;
            case 0:
                rainPh = 6;
                rain2Ph = 6;
                break;
        }
        if (curDifficulty == 0) { turnCount = 6; }
        else { turnCount = 5; }
    }

    public void Begin()
    {
        players = new GameObject[playerCount];
        for (int i = 0; i < playerCount; i++) { players[i] = Instantiate(playerPresets[i], central, Quaternion.identity); }
        canvas.gameObject.SetActive(false);
    }

    public void ToggleRules()
    {
        toggleGroup.SetActive(!toggleGroup.activeSelf);
        rulesCanvas.SetActive(!rulesCanvas.activeSelf);
        if (toggleGroup.activeSelf) { foreach (GameObject player in players) { player.SetActive(true); } }
    }

    public void Restart()
    {
        playerCount = 2;
        curDifficulty = Difficulty.Normal;
        curPlayer = 0;
        boardState = false;
        playerIn = new bool[4] { true, true, false, false };
        rainPh = 5;
        rain2Ph = 3;
        turnCount = 5;
        turn = 1;
        end = false;
        SceneManager.LoadScene("Main");
    }
}
