using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    [Header("Parameters")]
    public string _suit;
    public int _rank;
    public Color _color = Color.black;
    public string _colS = "Black";
    public List<GameObject> _decoGOs = new List<GameObject>();
    public List<GameObject> _pipGOs = new List<GameObject>();
    public GameObject _back;
    public CardDefinition _def;
    public SpriteRenderer[] _spriteRenderers;

    public bool faceUp
    {
        get
        {
            return !_back.activeSelf; 
        }
        set
        {
            _back.SetActive(!value);
        }
    }

    private void Start()
    {
        SetSortOrder(0);
    }

    public void PopulateSpriteRenderers()
    {
        if(_spriteRenderers == null || _spriteRenderers.Length == 0)
        {
            _spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        }
    }

    public void SetSortingLayerName(string tSLN)
    {
        PopulateSpriteRenderers();

        foreach(SpriteRenderer sr in _spriteRenderers)
        {
            sr.sortingLayerName = tSLN;
        }
    }

    public void SetSortOrder(int sOrd)
    {
        PopulateSpriteRenderers();

        foreach(SpriteRenderer sr in _spriteRenderers)
        {
            if(sr.gameObject == this.gameObject)
            {
                sr.sortingOrder = sOrd;
                continue;
            }

            switch (sr.gameObject.name)
            {
                case "back":
                    sr.sortingOrder = sOrd + 2;
                    break;

                case "face":
                default:
                    sr.sortingOrder = sOrd + 1;
                    break;
            }
        }
    }

    virtual public void OnMouseUpAsButton()
    {
        Debug.Log(name);
    }
}

[System.Serializable]
public class Decorator
{
    public string type;
    public Vector3 loc;
    public bool flip = false;
    public float scale;
}

[System.Serializable]
public class CardDefinition
{
    public string face;
    public int rank;
    public List<Decorator> pips = new List<Decorator>();
}
