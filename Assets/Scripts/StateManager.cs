using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class StateManager : BoardManager
{
    public TextMeshProUGUI diceText;
    private int result;
    public GameObject moveButtonPreset;
    private bool instantiated;

    public GameObject collectGroup;
    public TextMeshProUGUI collectDisPlayerText;
    public TextMeshProUGUI[] collectDisText;
    public TextMeshProUGUI collectReqText;
    public GameObject atoms;
    private string[] atomNames = new string[20];
    private string atom;

    public GameObject rain;

    public GameObject resultsCanvas;
    public TextMeshProUGUI resultsText;
    public GameObject lightSource;
    private int resultPlayer = 1;

    private readonly string[] answers = new string[12]
    {
        "KHC",
        "NaOH",
        "NaCH",
        "CHOK",
        "CHONa",
        "MgSiO",
        "MgCO",
        "MgOH",
        "NaCO",
        "KPO",
        "NaSiO",
        "NaClO"
    };
    private readonly int[] phs = new int[12]
    { 8, 8, 8, 9, 9, 9, 9, 10, 10, 11, 12, 12 };

    private void Start()
    {
        int i = 0;
        foreach (TextMeshPro text in atoms.GetComponentsInChildren<TextMeshPro>())
        {
            atomNames[i] = text.text;
            i++;
        }
    }

    void Update()
    {
        if (boardState && !instantiated)
        {
            CalculatePos();

            foreach (GameObject g in GameObject.FindGameObjectsWithTag("preset"))
            {
                if (g.name.Contains("Clone"))
                {
                    g.transform.parent = toggleGroup.transform;
                    g.GetComponent<Button>().onClick.AddListener(delegate { Move(g.transform.position); });
                }
            }
            instantiated = true;
        }

        for (int i = 0; i < 4; i++)
        {
            if (i == curPlayer) { collectDisText[i].gameObject.SetActive(true); }
            else { collectDisText[i].gameObject.SetActive(false); }
        }
    }

    public void RollDice()
    {
        if (!boardState)
        {
            result = UnityEngine.Random.Range(1, 7);
            diceText.text = result.ToString();
            boardState = true;
            instantiated = false;
        }
    }

    public void YesAction()
    {
        if (collectDisText[curPlayer].text == "None") 
        {
            if (atom == "HO") { collectDisText[curPlayer].text = "H\nO"; }
            else { collectDisText[curPlayer].text = atom; }
        }
        else 
        {
            if (atom == "HO") { collectDisText[curPlayer].text += $"\nH\nO"; }
            else { collectDisText[curPlayer].text += $"\n{atom}"; }
        }

        NoAction(); //They do the same thing anyway
    }
    public void NoAction() 
    {
        if (curPlayer != playerCount - 1) { curPlayer++; } else { curPlayer = 0; turn++; }
        while (!playerIn[curPlayer]) { if (curPlayer != playerCount - 1) { curPlayer++; } else { curPlayer = 0; turn++; } }

        collectDisPlayerText.text = $"P{curPlayer + 1}";
        boardState = false;
        instantiated = false;
        collectGroup.SetActive(false);

        if (turn > turnCount) { StartCoroutine("Rain"); }
    }

    public void Move(Vector3 coords)
    {
        players[curPlayer].transform.position = coords;
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("preset")) { if (g.name.Contains("Clone")) { UnityEngine.Object.Destroy(g); } }

        int curPos = 0;
        for (int i = 0; i < 29; i++) { if (Vector3.Distance(coords, positions[i]) < 0.1) { curPos = i; break; } }

        if (curPos < 20)
        {
            atom = atomNames[curPos];
            collectGroup.SetActive(true);
            collectReqText.text = $"{atom}?";
        }
        else { NoAction(); } //They do the same thing anyway
    }

    IEnumerator Rain()
    {
        //canvas2.SetActive(false) happens in other script
        raining = true;
        rain.SetActive(true);

        float alpha = 0f;
        while (alpha < 0.8f)
        {
            rain.GetComponent<Renderer>().material.color = new Color(1f, 1f, 1f, alpha);
            alpha += 0.01f;
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(2.5f);
        while (alpha > 0f)
        {
            rain.GetComponent<Renderer>().material.color = new Color(1f, 1f, 1f, alpha);
            alpha -= 0.01f;
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(0.5f);
        foreach (GameObject player in players) { player.SetActive(false); }
        resultsCanvas.SetActive(true);
        lightSource.SetActive(false);

        yield return null;
    }

    public void ContinueResult()
    {
        if (resultPlayer > playerCount) 
        {
            if (end) { resultPlayer = 1; raining = false; Restart(); return; }
            string winner = "";
            for (int i = 0; i < 4; i++) { if (playerIn[i]) { winner += $"P{i+1}"; } }
            if (winner == "") { resultsText.text = "It's a draw between the remaining people!"; end = true; }
            else if (winner.Length == 2) { resultsText.text = $"The winner is {winner}!"; end = true; }
            else
            {
                for (int i = 0; i < players.Length; i++) { players[i].SetActive(playerIn[i]); }
                resultsCanvas.SetActive(false);
                lightSource.SetActive(true);
                raining = false;

                resultsText.text = "...";
                resultPlayer = 1;
                turn = 1;
                rainPh--;
                rain2Ph--;
                if (rainPh < 2) { rainPh = 2; }
                if (rain2Ph < 0) { rain2Ph = 0; }
                foreach (TextMeshProUGUI text in collectDisText) { text.text = "None"; }
            }
            return;
        }

        List<char> atomList = collectDisText[resultPlayer-1].text.Replace("\n", "").ToCharArray().ToList();
        atomList.Sort();
        string atomString = new string(atomList.ToArray());

        string variance = "";
        foreach (string answer in answers)
        {
            List<char> temp = answer.ToCharArray().ToList();
            temp.Sort();
            string str = new string(temp.ToArray());

            if (str == atomString)
            {
                int curPos = 0;
                for (int i = 0; i < 29; i++) { if (Vector3.Distance(players[resultPlayer-1].transform.position, positions[i]) < 0.1) { curPos = i; break; } }
                int tempPh;
                if (curPos < 20) { tempPh = rain2Ph; }
                else { tempPh = rainPh; }

                int val = phs[Array.IndexOf(answers, answer)];
                if (tempPh + val >= 14) { variance = "survived"; } else { variance = "died"; playerIn[resultPlayer - 1] = false; }
                break;
            }
        }
        if (variance == "") { variance = "died"; playerIn[resultPlayer - 1] = false; }

        string[] atomResults = collectDisText[resultPlayer - 1].text.Split('\n');
        if (atomResults[0] != "None")
        {
            resultsText.text = $"Player {resultPlayer} {variance} with ";
            foreach (string atomResult in atomResults) { resultsText.text += $"{atomResult}, "; }
            resultsText.text = resultsText.text.Substring(0, resultsText.text.Length - 2);
            resultsText.text += ".";
        }
        else { resultsText.text = $"Player {resultPlayer} {variance} with nothing."; }

        resultPlayer++;
        if (resultPlayer <= playerCount)
        { for (int i = 0; i < 4; i++) { if (!playerIn[resultPlayer - 1]) { resultPlayer++; if (resultPlayer > playerCount) break; } else break; } }
    }

    void CalculatePos()
    {
        int curPos = 0;
        for (int i = 0; i < 29; i++) { if (Vector3.Distance(players[curPlayer].transform.position, positions[i]) < 0.1) { curPos = i; break; } }

        //Centre cases
        if (curPos == 20)
        {
            if (result < 3)
            {
                for (int i = 0; i < 4; i++)
                { Instantiate(moveButtonPreset, positions[20 + 2 * i + result], Quaternion.identity); }
            }
            else if (result == 3)
            {
                for (int i = 0; i < 4; i++)
                { Instantiate(moveButtonPreset, positions[3 + 5 * i], Quaternion.identity); }
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    if (result != 4 && i == 3) { Instantiate(moveButtonPreset, positions[result - 5], Quaternion.identity); }
                    else { Instantiate(moveButtonPreset, positions[5 * i + result], Quaternion.identity); }
                    Instantiate(moveButtonPreset, positions[6 + 5 * i - result], Quaternion.identity);
                }
            }

            return;
        }

        //Inner cases
        if (curPos > 20)
        {
            //First half
            if (curPos % 2 == 1)
            {
                //Stay inside
                if (result == 1)
                {
                    Instantiate(moveButtonPreset, positions[curPos + 1], Quaternion.identity);
                    Instantiate(moveButtonPreset, positions[20], Quaternion.identity);
                }
                else //Go outside
                {
                    int prevent = 0;

                    switch (curPos)
                    {
                        case 21:
                            if (result > 3) { Instantiate(moveButtonPreset, positions[result - 4], Quaternion.identity); }
                            else { Instantiate(moveButtonPreset, positions[16 + result], Quaternion.identity); }
                            if (result != 2) { Instantiate(moveButtonPreset, positions[20 - result], Quaternion.identity); }

                            prevent = 1;
                            break;

                        case 23:
                            if (result == 6) { Instantiate(moveButtonPreset, positions[19], Quaternion.identity); }
                            else { Instantiate(moveButtonPreset, positions[5 - result], Quaternion.identity); }
                            if (result != 2) { Instantiate(moveButtonPreset, positions[result + 1], Quaternion.identity); }

                            prevent = 2;
                            break;

                        case 25:
                            Instantiate(moveButtonPreset, positions[6 + result], Quaternion.identity);
                            if (result != 2) { Instantiate(moveButtonPreset, positions[10 - result], Quaternion.identity); }

                            prevent = 3;
                            break;

                        case 27:
                            Instantiate(moveButtonPreset, positions[11 + result], Quaternion.identity);
                            if (result != 2) { Instantiate(moveButtonPreset, positions[15 - result], Quaternion.identity); }

                            prevent = 4;
                            break;
                    }

                    //Act like we moved to centre
                    result -= 1;

                    if (result < 3)
                    {
                        for (int i = 0; i < 4; i++)
                        { if (prevent != i + 1) { Instantiate(moveButtonPreset, positions[20 + 2 * i + result], Quaternion.identity); } }
                    }
                    else if (result == 3)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (prevent == 1 && i == 3) { break; }
                            if (prevent != i + 2) { Instantiate(moveButtonPreset, positions[3 + 5 * i], Quaternion.identity); }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (prevent == 1 && i == 3) { break; }
                            if (prevent == i + 2) { continue; }
                            if (result != 4 && i == 3) { Instantiate(moveButtonPreset, positions[result - 5], Quaternion.identity); }
                            else { Instantiate(moveButtonPreset, positions[5 * i + result], Quaternion.identity); }
                            Instantiate(moveButtonPreset, positions[6 + 5 * i - result], Quaternion.identity);
                        }
                    }
                }
            }
            else //Second half
            {
                //Stay inside
                if (result == 1) { Instantiate(moveButtonPreset, positions[curPos - 1], Quaternion.identity); }
                if (result == 2) { Instantiate(moveButtonPreset, positions[20], Quaternion.identity); }

                int prevent = 0;

                //Go outside
                switch (curPos)
                {
                    case 22:
                        if (result > 2) { Instantiate(moveButtonPreset, positions[result - 3], Quaternion.identity); }
                        else { Instantiate(moveButtonPreset, positions[17 + result], Quaternion.identity); }
                        if (result != 1) { Instantiate(moveButtonPreset, positions[19 - result], Quaternion.identity); }

                        prevent = 1;
                        break;

                    case 24:
                        if (result > 4) { Instantiate(moveButtonPreset, positions[24 - result], Quaternion.identity); }
                        else { Instantiate(moveButtonPreset, positions[4 - result], Quaternion.identity); }
                        if (result != 1) { Instantiate(moveButtonPreset, positions[result + 2], Quaternion.identity); }

                        prevent = 2;
                        break;

                    case 26:
                        Instantiate(moveButtonPreset, positions[7 + result], Quaternion.identity);
                        if (result != 1) { Instantiate(moveButtonPreset, positions[9 - result], Quaternion.identity); }

                        prevent = 3;
                        break;

                    case 28:
                        Instantiate(moveButtonPreset, positions[12 + result], Quaternion.identity);
                        if (result != 1) { Instantiate(moveButtonPreset, positions[14 - result], Quaternion.identity); }

                        prevent = 4;
                        break;
                }

                //Act like we moved to centre
                result -= 2;

                if (result <= 0) { return; }
                if (result < 3)
                {
                    for (int i = 0; i < 4; i++)
                    { if (prevent != i + 1) { Instantiate(moveButtonPreset, positions[20 + 2 * i + result], Quaternion.identity); } }
                }
                else if (result == 3)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (prevent == 1 && i == 3) { break; }
                        if (prevent != i + 2) { Instantiate(moveButtonPreset, positions[3 + 5 * i], Quaternion.identity); }
                    }
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (prevent == 1 && i == 3) { break; }
                        if (prevent == i + 2) { continue; }
                        if (result != 4 && i == 3) { Instantiate(moveButtonPreset, positions[result - 5], Quaternion.identity); }
                        else { Instantiate(moveButtonPreset, positions[5 * i + result], Quaternion.identity); }
                        Instantiate(moveButtonPreset, positions[6 + 5 * i - result], Quaternion.identity);
                    }
                }
            }

            return;
        }

        //Outer cases
        if (curPos - result < 0) { Instantiate(moveButtonPreset, positions[20 - result + curPos], Quaternion.identity); }
        else { Instantiate(moveButtonPreset, positions[curPos - result], Quaternion.identity); }
        Instantiate(moveButtonPreset, positions[(curPos + result) % 20], Quaternion.identity);

        //Checking individual paths that connect outer and inner

        //Possible to go inner?
        int tempPos = curPos;
        int tempResult = result;
        if (curPos <= result - 2) { tempPos += 20;  }
        if (result - Math.Abs(18 - tempPos) > 0) 
        {
            tempResult -= Math.Abs(18 - tempPos) + 1;

            //Act like in inner
            if (tempResult < 3) { Instantiate(moveButtonPreset, positions[22 - tempResult], Quaternion.identity); }
            else
            {
                tempResult -= 2;

                //Act like in centre
                if (tempResult == 0) { return; }
                if (tempResult < 3)
                {
                    for (int i = 1; i < 4; i++) { Instantiate(moveButtonPreset, positions[20 + 2 * i + tempResult], Quaternion.identity); }
                }
                else
                {
                    for (int i = 0; i < 3; i++) { Instantiate(moveButtonPreset, positions[3 + 5 * i], Quaternion.identity); }
                }
            }
        }

        //Possible to go inner?
        tempPos = curPos;
        tempResult = result;
        if (curPos > 23 - result) { tempPos -= 20; }
        if (result - Math.Abs(3 - tempPos) > 0) 
        {
            tempResult -= Math.Abs(3 - tempPos) + 1;

            //Act like in inner
            if (tempResult < 2) { Instantiate(moveButtonPreset, positions[24 - tempResult], Quaternion.identity); }
            else if (tempResult == 2) { Instantiate(moveButtonPreset, positions[20], Quaternion.identity); }
            else
            {
                tempResult -= 2;

                //Act like in centre
                if (tempResult == 0) { return; }
                if (tempResult < 3)
                {
                    for (int i = 0; i < 4; i++)
                    { if (i != 1) { Instantiate(moveButtonPreset, positions[20 + 2 * i + tempResult], Quaternion.identity); } }
                }
                else
                {
                    for (int i = 1; i < 4; i++)
                    { Instantiate(moveButtonPreset, positions[3 + 5 * i], Quaternion.identity); }
                }
            }
        }

        //Possible to go inner?
        tempResult = result;
        if (result - Math.Abs(8 - curPos) > 0) 
        {
            tempResult -= Math.Abs(8 - curPos) + 1;

            //Act like in inner
            if (tempResult < 2) { Instantiate(moveButtonPreset, positions[26 - tempResult], Quaternion.identity); }
            else if (tempResult == 2) { Instantiate(moveButtonPreset, positions[20], Quaternion.identity); }
            else
            {
                tempResult -= 2;

                //Act like in centre
                if (tempResult == 0) { return; }
                if (tempResult < 3)
                {
                    for (int i = 0; i < 4; i++)
                    { if (i != 2) { Instantiate(moveButtonPreset, positions[20 + 2 * i + tempResult], Quaternion.identity); } }
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    { if (i != 1) { Instantiate(moveButtonPreset, positions[3 + 5 * i], Quaternion.identity); } }
                }
            }
        }

        //Possible to go inner?
        tempResult = result;
        if (result - Math.Abs(13 - curPos) > 0)
        {
            tempResult -= Math.Abs(13 - curPos) + 1;

            //Act like in inner
            if (tempResult < 2) { Instantiate(moveButtonPreset, positions[28 - tempResult], Quaternion.identity); }
            else if (tempResult == 2) { Instantiate(moveButtonPreset, positions[20], Quaternion.identity); }
            else
            {
                tempResult -= 2;

                //Act like in centre
                if (tempResult == 0) { return; }
                if (tempResult < 3)
                {
                    for (int i = 0; i < 3; i++) { Instantiate(moveButtonPreset, positions[20 + 2 * i + tempResult], Quaternion.identity); }
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    { if (i != 2) { Instantiate(moveButtonPreset, positions[3 + 5 * i], Quaternion.identity); } }
                }
            }
        }
    }
}
