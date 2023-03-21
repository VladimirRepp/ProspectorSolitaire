using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatingScore : MonoBehaviour
{
    [Header("Parameters")]
    public EFSState _state = EFSState.idle;

    [SerializeField]
    protected int _score = 0;
    public string _scoreString;

    public List<Vector2> _bezierPts;
    public List<float> _fontSizes;

    public float _timeStart = -1f;
    public float _timeDuration = 1f;
    public string _easingCurve = Easing.InOut;

    public GameObject _reportFinishTo = null;

    private RectTransform _rectTransform;
    private Text _txt;
         
    public int score
    {
        get
        {
            return _score;
        }
        set
        {
            _score = value;
            _scoreString = _score.ToString("N0"); // the "N0" argument requires adding dots to the number
            GetComponent<Text>().text = _scoreString;
        }
    }

    private void Update()
    {
        if (_state == EFSState.idle)
            return;

        // NaN,NaN
        float u = (Time.time - _timeStart) / _timeDuration;
        float uC = Easing.Ease(u, _easingCurve);

        if (u < 0)
        {
            _state = EFSState.pre;
        }
        else
        {
            if(u >= 1)
            {
                uC = 1;
                _state = EFSState.post;

                if(_reportFinishTo != null)
                {
                    _reportFinishTo.SendMessage("FSCallback", this);
                    Destroy(gameObject);
                }
                else
                {
                    _state = EFSState.idle;
                }
            }
            else
            {
                _state = EFSState.active;
                _txt.enabled = true;
            }

            Vector2 pos = Utils.Bezier(uC, _bezierPts); // NaN,NaN
            _rectTransform.anchorMin = _rectTransform.anchorMax = pos;

            if(_fontSizes != null && _fontSizes.Count > 0)
            {
                int size = 26;

                /*
                 if(pos.x > pos.y)
                    size = Mathf.RoundToInt(pos.x);
                else
                    size = Mathf.RoundToInt(pos.y);
                */
                //size = Mathf.RoundToInt(Utils.Bezier(uC, _bezierPts));
                GetComponent<Text>().fontSize = size;
            }
        }
    }

    public void Init(List<Vector2> ePts, float eTimeS = 0, float eTimeD = 1)
    {
        _rectTransform = GetComponent<RectTransform>();
        _rectTransform.anchoredPosition = Vector2.one;

        _txt = GetComponent<Text>();

        _bezierPts = new List<Vector2>(ePts);

        if(ePts.Count == 1)
        {
            transform.position = ePts[0];
            return;
        }

        if (eTimeS == 0)
            eTimeS = Time.time;

        _timeStart = eTimeS;
        _timeDuration = eTimeD;

        _state = EFSState.pre;
    }

    public void FSCallback(FloatingScore fs)
    {
        score += fs.score;
    }
}

public enum EFSState
{
    idle,
    pre,
    active,
    post
}