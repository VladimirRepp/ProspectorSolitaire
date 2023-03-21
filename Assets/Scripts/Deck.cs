using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    [Header("Settings")]
    public bool _startFaceUp = false;

    [Header("Suits")]
    public Sprite _suitClub;
    public Sprite _suitDiamond;
    public Sprite _suitHeart;
    public Sprite _suitSpade;

    [Header("Others sprite")]
    public Sprite[] _faceSprites;
    public Sprite[] _rankSprites;

    [Header("Cards")]
    public Sprite _cardBack;
    public Sprite _cardBackGold;
    public Sprite _cardFront;
    public Sprite _cardFrontGold;

    [Header("Prefabs")]
    public GameObject _prefabCard;
    public GameObject _prefabSprite;

    [Header("Parameters")]
    public PT_XMLReader _xmlReader;
    public List<string> _cardNames;
    public List<Card> _cards;
    public List<Decorator> _decorators;
    public List<CardDefinition> _cardDefinitions;
    public Transform _deckAnchor;
    public Dictionary<string, Sprite> _dictSuits;

    public void InitDeck(string deckXMLText)
    {
        if (GameObject.Find("Deck") == null)
        {
            GameObject anchorGO = new GameObject("Deck");
            _deckAnchor = anchorGO.transform;
        }

        _dictSuits = new Dictionary<string, Sprite>()
        {
            {"C", _suitClub },
            {"D", _suitDiamond },
            {"H", _suitHeart },
            {"S", _suitSpade },
        };

        ReadDeck(deckXMLText);
        MakeCards();
    }

    public void ReadDeck(string deckXMLText)
    {
        _xmlReader = new PT_XMLReader();
        _xmlReader.Parse(deckXMLText);

        string s = "xml[0] decorator[0]";
        s += "type=" + _xmlReader.xml["xml"][0]["decorator"][0].att("type");
        s += "x=" + _xmlReader.xml["xml"][0]["decorator"][0].att("x");
        s += "y=" + _xmlReader.xml["xml"][0]["decorator"][0].att("y");
        s += "scale=" + _xmlReader.xml["xml"][0]["decorator"][0].att("scale");

       _decorators = new List<Decorator>();

        /*
            float.Parse("1.25", CultureInfo.InvariantCulture)
            CultureInfo.InvariantCulture - for regional setting of the float number representation
        */

        PT_XMLHashList xDexos = _xmlReader.xml["xml"][0]["decorator"];
        Decorator deco;
        for(int i = 0; i< xDexos.Count; i++)
        {
            deco = new Decorator();
            deco.type = xDexos[i].att("type");
            deco.flip = (xDexos[i].att("flip") == "1");
            deco.scale = float.Parse(xDexos[i].att("scale"), CultureInfo.InvariantCulture);
            deco.loc.x = float.Parse(xDexos[i].att("x"), CultureInfo.InvariantCulture);
            deco.loc.y = float.Parse(xDexos[i].att("y"), CultureInfo.InvariantCulture);
            deco.loc.z = float.Parse(xDexos[i].att("z"), CultureInfo.InvariantCulture);

           _decorators.Add(deco);
        }

        CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        ci.NumberFormat.CurrencyDecimalSeparator = ".";

        _cardDefinitions = new List<CardDefinition>();
        PT_XMLHashList xCardDefs = _xmlReader.xml["xml"][0]["card"];
        for(int i = 0; i<xCardDefs.Count; i++)
        {
            CardDefinition cDef = new CardDefinition();

            cDef.rank = int.Parse(xCardDefs[i].att("rank"));

            PT_XMLHashList xPips = xCardDefs[i]["pip"];
            if(xPips != null)
            {
                for(int j = 0; j<xPips.Count; j++)
                {
                    deco = new Decorator();

                    deco.type = "pip";
                    deco.flip = (xPips[j].att("flip") == "1");
                    deco.loc.x = float.Parse(xPips[j].att("x"), CultureInfo.InvariantCulture);
                    deco.loc.y = float.Parse(xPips[j].att("y"), CultureInfo.InvariantCulture);
                    
                    deco.loc.z = float.Parse(xPips[j].att("z"), CultureInfo.InvariantCulture);
                    
                    if (xPips[j].HasAtt("scale"))
                    {
                        deco.scale = float.Parse(xPips[j].att("scale"), NumberStyles.Any, ci);
                    }

                    cDef.pips.Add(deco);
                }
            }

            if (xCardDefs[i].HasAtt("face"))
            {
                cDef.face = xCardDefs[i].att("face");
            }

            _cardDefinitions.Add(cDef);
        }
    }

    public CardDefinition GetCardDefinitionByRank(int rank)
    {
        foreach(CardDefinition cd in _cardDefinitions)
        {
            if(cd.rank == rank)
            {
                return cd;
            }
        }

        return null;    
    }

    public void MakeCards()
    {
        _cardNames = new List<string>();
        string[] letters = new string[] { "C", "D", "H", "S" };
        foreach(string s in letters)
        {
            for(int i = 0; i<13; i++)
            {
                _cardNames.Add(s + (i + 1));
            }
        }

        _cards = new List<Card>();
        for(int i = 0; i<_cardNames.Count; i++)
        {
            _cards.Add(MakeCard(i));
        }
    }

    private Card MakeCard(int cNum)
    {
        GameObject cgo = Instantiate(_prefabCard) as GameObject;
        cgo.transform.parent = _deckAnchor;
        Card card = cgo.GetComponent<Card>();

        // Arrange in a neat row
        cgo.transform.localPosition = new Vector3((cNum % 13) * 3, cNum / 13 * 4, 0);

        card.name = _cardNames[cNum];
        card._suit = card.name[0].ToString();
        card._rank = int.Parse(card.name.Substring(1));
        if(card._suit == "D" || card._suit == "H")
        {
            card._colS = "Red";
            card._color = Color.red;
        }
        else
        {
            card._colS = "Black";
            card._color = Color.black;
        }

        card._def = GetCardDefinitionByRank(card._rank);

        AddDecoratots(card);
        AddPips(card);
        AddFace(card);
        AddBack(card);

        return card;
    }

    private Sprite _tSp = null;
    private GameObject _tGO = null;
    private SpriteRenderer _tSR = null;

    private void AddDecoratots(Card card)
    {
        foreach(Decorator dec in _decorators)
        {
            if(dec.type == "suit")
            {
                _tGO = Instantiate<GameObject>(_prefabSprite);
                _tSR = _tGO.GetComponent<SpriteRenderer>();
                _tSR.sprite = _dictSuits[card._suit];
            }
            else
            {
                _tGO = Instantiate<GameObject>(_prefabSprite);
                _tSR = _tGO.GetComponent<SpriteRenderer>();
                _tSp = _rankSprites[card._rank];
                _tSR.sprite = _tSp;
                _tSR.color = card._color;
            }

            _tSR.sortingOrder = 1; // place the sprite above the card
            _tGO.transform.SetParent(card.transform);
            _tGO.transform.localPosition = dec.loc;

            if (dec.flip)
            {
                _tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
            }

            if(dec.scale != 1)
            {
                _tGO.transform.localScale = Vector3.one * dec.scale;
            }

            _tGO.name = dec.type;
            card._decoGOs.Add(_tGO);
        }
    }

    private void AddPips(Card card)
    {
        foreach(Decorator pip in card._def.pips)
        {
            _tGO = Instantiate<GameObject>(_prefabSprite);
            _tGO.transform.SetParent(card.transform);
            _tGO.transform.localPosition = pip.loc;

            if (pip.flip)
            {
                _tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
            }

            if(pip.scale != 0)
            {
                _tGO.transform.localScale = Vector3.one * pip.scale;
            }
            else
            {
                _tGO.transform.localScale = Vector3.one;
            }

            _tGO.name = "pip";
            _tSR = _tGO.GetComponent<SpriteRenderer>();
            _tSR.sprite = _dictSuits[card._suit];
            _tSR.sortingOrder = 1; // display order on top of the card
            card._pipGOs.Add(_tGO);
        }
    }

    private void AddFace(Card card)
    {
        if(card._def.face == "")
        {
            return;
        }

        _tGO = Instantiate<GameObject>(_prefabSprite);
        _tSR = _tGO.GetComponent<SpriteRenderer>();
        _tSp = GetFace(card._def.face + card._suit);
        _tSR.sprite = _tSp;
        _tSR.sortingOrder = 1; // display order on top of the card
        _tGO.transform.SetParent(card.transform);
        _tGO.transform.localPosition = Vector3.zero;
        _tGO.name = "face";
    }

    private Sprite GetFace(string faceS)
    {
        foreach(Sprite _tSP in _faceSprites)
        {
            if(_tSP.name == faceS)
            {
                return _tSP;    
            }
        }

        return null;
    }

    private void AddBack(Card card)
    {
        _tGO = Instantiate<GameObject>(_prefabSprite);
        _tSR = _tGO.GetComponent<SpriteRenderer>();
        _tSR.sprite = _cardBack;
        _tGO.transform.SetParent(card.transform);
        _tGO.transform.localPosition = Vector3.zero;

        _tSR.sortingOrder = 2;  // display order on top of the card 
        _tGO.name = "back";
        card._back = _tGO;

        card.faceUp = _startFaceUp; 
    }

    static public void Shuffle(ref List<Card> oCards)
    {
        List<Card> tCards = new List<Card>();

        int index;
        while(oCards.Count > 0)
        {
            index = Random.Range(0, oCards.Count);
            tCards.Add(oCards[index]);
            oCards.RemoveAt(index);
        }

        oCards = tCards;
    }
}
