using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class BoardManager : ButtonManager
{
    public GameObject parentPosition;
    protected static Vector3[] positions = new Vector3[30];

    public GameObject canvas2;
    public GameObject collectCanvas;

    public TextMeshProUGUI statText;

    protected static bool raining;

    void Start()
    {
        int i = 0;
        foreach (Transform childTransform in parentPosition.GetComponentsInChildren<Transform>())
        {
            if (childTransform == parentPosition.transform) { continue; }
            positions[i] = childTransform.position;
            i++;
        }
    }

    void Update()
    {
        if (!raining)
        {
            if (!Input.GetKey(KeyCode.Z) && !Input.GetKey(KeyCode.X)) { canvas2.SetActive(!canvas.activeSelf); }
            else { canvas2.SetActive(false); }

            if (!canvas.activeSelf)
            {
                for (int i = 0; i < players.Length; i++) { players[i].SetActive(playerIn[i]); }

                if (Input.GetKey(KeyCode.Z) || rulesCanvas.activeSelf || Input.GetKey(KeyCode.X)) { foreach (GameObject player in players) { player.SetActive(false); } }
                else { for (int i = 0; i < players.Length; i++) { players[i].SetActive(playerIn[i]); } }

                if (Input.GetKey(KeyCode.X)) { collectCanvas.SetActive(true); }
                else { collectCanvas.SetActive(false); }

                foreach (GameObject player in players)
                {
                    if (player == players[curPlayer]) { player.GetComponent<Renderer>().material = Resources.Load<Material>($"Materials/P{curPlayer + 1}"); }
                    else { player.GetComponent<Renderer>().material = Resources.Load<Material>($"Materials/P{Array.IndexOf(players, player) + 1} Transparent"); }
                }

                statText.text = $"Inside rain at strength {rainPh}.\nOutside rain at strength {rain2Ph}." +
                $"\nTime until rain: {turnCount - turn + 1}\nDifficulty: {curDifficulty}";
            }
        }
        else { canvas2.SetActive(false); }
    }
}
