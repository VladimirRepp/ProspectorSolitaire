using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Prospector : MonoBehaviour
{
    static public Prospector S;

    [Header("Settings")]
    public TextAsset _deckXML;
    public TextAsset _layoutXML;
    public float _xOffset = 3;
    public float _yOffset = -2.5f;
    public Vector3 _layoutCenter;
    public Vector2 _fsPosMid = new Vector2(0.5f, 0.90f);
    public Vector2 _fsPosRun = new Vector2(0.5f, 0.75f);
    public Vector2 _fsPosMid2 = new Vector2(0.4f, 1.0f);
    public Vector2 _fsPosEnd = new Vector2(0.5f, 0.95f);
    public float _reloadDelay = 2f;

    [Header("Parameters")]
    public Deck _deck;
    public Layout _layout;
    public List<CardProspector> _drawPile;
    public Transform _layoutAnchor;
    public CardProspector _target;
    public List<CardProspector> _tableau;
    public List<CardProspector> _discardPile;
    public FloatingScore _fsRun;
    public Text _gameOverText, _roundResultText, _highScoreText;

    private void Awake()
    {
        S = this;
        SetUpUITexts();
    }

    private void Start()
    {
        Scoreboard.S.score = ScoreManager.SCORE;

        _deck = GetComponent<Deck>();
        _deck.InitDeck(_deckXML.text);
        Deck.Shuffle(ref _deck._cards);

        /*Card c;
        for(int i = 0; i<_deck._cards.Count; i++)
        {
            c = _deck._cards[i];
            c.transform.localPosition = new Vector3((i % 13) * 3, i / 13 * 4, 0);
        }*/

        _layout = GetComponent<Layout>();
        _layout.ReadLayout(_layoutXML.text);
        _drawPile = ConvertListCardToListCardProspector(_deck._cards);

        LayoutGame();
    }

    void SetUpUITexts()
    {
        GameObject go = GameObject.Find("HighScore");

        if(go != null)
        {
            _highScoreText = go.GetComponent<Text>();
        }

        int highScore = ScoreManager.HIGH_SCORE;
        string hScore = "Лучший счет: " + Utils.AddCommasToNumber(highScore);
        go.GetComponent<Text>().text = hScore;

        go = GameObject.Find("GameOver");
        if(go != null)
        {
            _gameOverText = go.GetComponent<Text>();
        }

        go = GameObject.Find("RoundResult");
        if(go != null)
        {
            _roundResultText = go.GetComponent<Text>();
        }

        ShowResultsUI(false);
    }

    void ShowResultsUI(bool show)
    {
        _gameOverText.gameObject.SetActive(show);
        _roundResultText.gameObject.SetActive(show);
    }

    List<CardProspector> ConvertListCardToListCardProspector(List<Card> lCD)
    {
        List<CardProspector> lCP = new List<CardProspector>();
        CardProspector tCP;

        foreach(Card tCD in lCD)
        {
            tCP = tCD as CardProspector;
            lCP.Add(tCP);
        }

        return lCP;
    }

    CardProspector Draw()
    {
        CardProspector cd = _drawPile[0];
        _drawPile.RemoveAt(0);
        return cd;
    }

    void LayoutGame()
    {
        if(_layoutAnchor == null)
        {
            GameObject tGO = new GameObject("LayoutAnchor");
            _layoutAnchor = tGO.transform;
            _layoutAnchor.transform.position = _layoutCenter;
        }

        CardProspector cp;
        foreach(SlotDef sd in _layout._slotDefs)
        {
            cp = Draw();
            cp.faceUp = sd.faceUp;
            cp.transform.parent = _layoutAnchor;
            cp.transform.localPosition = new Vector3(_layout._multiplier.x * sd.x, _layout._multiplier.y * sd.y, -sd.layerID);
            cp.layoutID = sd.id;
            cp.slotDef = sd;
            cp.state = ECardState.tableau;
            cp.SetSortingLayerName(sd.layerName);

            _tableau.Add(cp);
        }

        foreach(CardProspector tCP in _tableau)
        {
            foreach(int hid in tCP.slotDef.hiddenBy)
            {
                cp = FindCardByLayoutID(hid);
                tCP.hiddenBy.Add(cp);
            }
        }

        MoveToTarget(Draw());
        UpdateDrawPile();
    }

    CardProspector FindCardByLayoutID(int layoutID)
    {
        foreach(CardProspector tCP in _tableau)
        {
            if(tCP.layoutID == layoutID)
            {
                return tCP;
            }
        }

        return null;
    }

    void SetTableauFaces()
    {
        foreach(CardProspector cd in _tableau)
        {
            bool faceUp = true;

            foreach(CardProspector cover in cd.hiddenBy)
            {
                if(cover.state == ECardState.tableau)
                {
                    faceUp = false;
                }
            }

            cd.faceUp = faceUp;
        }
    }

    void MoveToDiscard(CardProspector cd)
    {
        cd.state = ECardState.discard;
        _discardPile.Add(cd);
        cd.transform.parent = _layoutAnchor;

        // Move to discard position
        cd.transform.localPosition = new Vector3(_layout._multiplier.x * _layout._discardPile.x, _layout._multiplier.y * _layout._discardPile.y, _layout._discardPile.layerID + 0.5f);
        
        cd.faceUp = true;
        cd.SetSortingLayerName(_layout._discardPile.layerName);
        cd.SetSortOrder(-100 + _discardPile.Count);
    }

    void MoveToTarget(CardProspector cd)
    { 
        if(_target != null)
        {
            MoveToDiscard(_target);
        }

        _target = cd;
        cd.state = ECardState.target;
        cd.transform.parent = _layoutAnchor;

        // Move to target position
        cd.transform.localPosition = new Vector3(_layout._multiplier.x * _layout._discardPile.x, _layout._multiplier.y * _layout._discardPile.y, -_layout._discardPile.layerID);

        cd.faceUp = true;
        cd.SetSortingLayerName(_layout._discardPile.layerName);
        cd.SetSortOrder(0);
    }

    void UpdateDrawPile()
    {
        CardProspector cd;
        for(int i = 0; i<_drawPile.Count; i++)
        {
            cd = _drawPile[i];
            cd.transform.parent = _layoutAnchor;

            // Position based on offset _layout._drawPile.stagger
            Vector2 dpStagger = _layout._drawPile.stagger;
            cd.transform.localPosition = new Vector3(_layout._multiplier.x * (_layout._drawPile.x + i * dpStagger.x), _layout._multiplier.y * (_layout._drawPile.y + i * dpStagger.y), -_layout._drawPile.layerID + 0.1f * i);

            cd.faceUp = false;
            cd.state = ECardState.drawpile;
            cd.SetSortingLayerName(_layout._drawPile.layerName);
            cd.SetSortOrder(-10 * i);
        }
    }

    public void CardClicked(CardProspector cd)
    {
        switch (cd.state)
        {
            case ECardState.target:
                break;

            case ECardState.drawpile:
                MoveToDiscard(_target);
                MoveToTarget(Draw());
                UpdateDrawPile();
                ScoreManager.EVENT(EScoreEvent.draw);
                FloatingScoreHandler(EScoreEvent.draw);
                break;

            case ECardState.tableau:
                bool validMatch = true;

                if (!cd.faceUp)
                {
                    validMatch = false;
                }

                if (!AdjacentRank(cd, _target))
                {
                    validMatch = false;
                }

                if (!validMatch)
                    return;

                _tableau.Remove(cd);
                MoveToTarget(cd);
                SetTableauFaces();

                ScoreManager.EVENT(EScoreEvent.mine);
                FloatingScoreHandler(EScoreEvent.mine);
                break;
        }

        CheckForGaveOver();
    }

    void CheckForGaveOver()
    {
        if (_tableau.Count == 0)
        {
            GameOver(true);
            return;
        }

        if(_drawPile.Count > 0)
        {
            return;
        }

        // Checking possible moves
        foreach (CardProspector cd in _tableau)
        {
            if(AdjacentRank(cd, _target))
            {
                return;
            }
        }

        GameOver(false);
    }

    void GameOver(bool won)
    {
        int score = ScoreManager.SCORE;
        if (_fsRun != null)
            score += _fsRun.score;

        if (won)
        {
            _gameOverText.text = "Конец раунда!";
            _roundResultText.text = "Победа в этом раунде!\nЛучший счет: " + score;
            ShowResultsUI(true);

            ScoreManager.EVENT(EScoreEvent.gameWin);
            FloatingScoreHandler(EScoreEvent.gameWin);
        }
        else
        {
            _gameOverText.text = "Конец игры!";
            if(ScoreManager.HIGH_SCORE <= score)
            {
                _roundResultText.text = "Набран лучший счет!\nЛучший счет: " + score;
            }
            else
            {
                _roundResultText.text = "Набранный счет: " + score;
            }
            ShowResultsUI(true);

            ScoreManager.EVENT(EScoreEvent.gameLoss);
            FloatingScoreHandler(EScoreEvent.gameLoss);
        }

        Invoke("ReloadLevel", _reloadDelay);
    }

    void ReloadLevel()
    {
        SceneManager.LoadScene("__Prospector_Scene_0");
    }

    void FloatingScoreHandler(EScoreEvent evt)
    {
        List<Vector2> fsPts;

        switch (evt)
        {
            case EScoreEvent.draw:
            case EScoreEvent.gameWin:
            case EScoreEvent.gameLoss:
                if(_fsRun != null)
                {
                    fsPts = new List<Vector2>();
                    fsPts.Add(_fsPosRun);
                    fsPts.Add(_fsPosMid2);
                    fsPts.Add(_fsPosEnd);

                    _fsRun._reportFinishTo = Scoreboard.S.gameObject;
                    _fsRun.Init(fsPts, 0, 1);

                    _fsRun._fontSizes = new List<float>(new float[] { 28, 36, 4});
                    _fsRun = null;
                }
                break;

            case EScoreEvent.mine:
                FloatingScore fs;

                Vector2 p0 = Input.mousePosition;
                p0.x /= Screen.width;
                p0.y /= Screen.height;

                fsPts = new List<Vector2>();
                fsPts.Add(p0);
                fsPts.Add(_fsPosMid);
                fsPts.Add(_fsPosRun);

                fs = Scoreboard.S.CreateFloatingScore(ScoreManager.CHAIN, fsPts);
                fs._fontSizes = new List<float>(new float[] { 4, 50, 28});

                if (_fsRun == null)
                {
                    _fsRun = fs;
                    _fsRun._reportFinishTo = null;
                }
                else
                {
                    fs._reportFinishTo = _fsRun.gameObject;
                }
                break;
        }
    }

    public bool AdjacentRank(CardProspector c0, CardProspector c1)
    {
        if (!c0.faceUp || !c1.faceUp)
            return false;

        if(Mathf.Abs(c0._rank - c1._rank) == 1)
        {
            return true;
        }

        if (c0._rank == 1 && c1._rank == 13)
            return true;

        if (c0._rank == 13 && c1._rank == 1)
            return true;

        return false;
    }
}
