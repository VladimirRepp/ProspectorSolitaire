using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private static ScoreManager S;

    static public int SCORE_FROM_PREV_ROUND = 0;
    static public int HIGH_SCORE = 0;

    [Header("Parameters")]
    public int _chain = 0;
    public int _scoreRun = 0;
    public int _score = 0;

    static public int CHAIN
    {
        get
        {
            return S._chain;
        }
    }
    static public int SCORE 
    {
        get
        {
            return S._score;
        }
    }
    static public int SCORE_RUN
    {
        get
        {
            return S._scoreRun;
        }
    }

    private void Awake()
    {
        if (S == null)
            S = this;
        else
            Debug.LogError("ERROR: ScoreManager.Awake(): S is alreasy set!");

        if (PlayerPrefs.HasKey("ProspectorHighScore"))
        {
            HIGH_SCORE = PlayerPrefs.GetInt("ProspectorHighScore");
        }

        _scoreRun += SCORE_FROM_PREV_ROUND;
        SCORE_FROM_PREV_ROUND = 0;
    }

    static public void EVENT(EScoreEvent evt)
    {
        try
        {
            S.Event(evt);
        }
        catch (System.NullReferenceException nre)
        {
            Debug.LogError("ScoreManager.EVENT() called while S=null.\n" + nre);
        }
    }

    void Event(EScoreEvent evt)
    {
        switch (evt)
        {
            case EScoreEvent.draw:
            case EScoreEvent.gameWin:
            case EScoreEvent.gameLoss:
                _chain = 0;
                _score += _scoreRun;
                _scoreRun = 0;
                break;

            case EScoreEvent.mine:
                _chain++;
                _scoreRun += _chain;
                break;
        }

        switch (evt)
        {
            case EScoreEvent.gameWin:
                SCORE_FROM_PREV_ROUND = _score;
                Debug.Log("You won this round! Round score: " + _score);
                break;

            case EScoreEvent.gameLoss:
                if(HIGH_SCORE <= _score)
                {
                    Debug.Log("You got the high score! High score: " + _score);
                    HIGH_SCORE = _score;
                    PlayerPrefs.SetInt("ProspectorHighScore", _score);
                }
                else
                {
                    Debug.Log("Your final score for the game was: " + _score);
                }
                break;

            default:
                Debug.Log($"Score: {_score} ScoreRun: {_scoreRun} Chain: {_chain}");
                break;
        }
    }

}

public enum EScoreEvent
{
    draw = 0,
    mine,
    mineGold,
    gameWin,
    gameLoss
}