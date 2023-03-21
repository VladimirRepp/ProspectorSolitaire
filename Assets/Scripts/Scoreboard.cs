using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scoreboard : MonoBehaviour
{
    public static Scoreboard S;

    [Header("Settings")]
    public GameObject _prefabFloatingScore;

    [Header("Parameters")]
    [SerializeField] private int _score = 0;
    [SerializeField] private string _scoreString;

    private Transform _canvasTrans;

    public int score
    {
        get
        {
            return _score;
        }
        set
        {
            _score = value;
            scoreString = _score.ToString("N0");
        }
    }

    public string scoreString
    {
        get
        {
            return _scoreString;
        }
        set
        {
            _scoreString = value;
            GetComponent<Text>().text = _scoreString;
        }
    }

    private void Awake()
    {
        if(S == null)
        {
            S = this;
        }
        else
        {
            Debug.LogError("ERROR: Scoreboard.Awake(): S is already set!");
        }
        _canvasTrans = transform.parent;
    }

    public void FSCallback(FloatingScore fs)
    {
        score += fs.score;
    }

    public FloatingScore CreateFloatingScore(int amt, List<Vector2> pts)
    {
        GameObject go = Instantiate<GameObject>(_prefabFloatingScore);
        go.transform.SetParent(_canvasTrans);
        FloatingScore fs = go.GetComponent<FloatingScore>();
        fs.score = amt;
        fs._reportFinishTo = this.gameObject;
        fs.Init(pts);
        return fs;
    }
}
