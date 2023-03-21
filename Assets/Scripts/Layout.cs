using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class Layout : MonoBehaviour
{
    [Header("Parameters")]
    public PT_XMLReader _xmlr;
    public PT_XMLHashtable _xml;
    public Vector2 _multiplier;

    public List<SlotDef> _slotDefs;
    public SlotDef _drawPile;
    public SlotDef _discardPile;
    public string[] _sotringLayerNames = new string[] { "Row0", "Row1", "Row2", "Row3", "Discard", "Draw" };

    public void ReadLayout(string xmlText)
    {
        _xmlr = new PT_XMLReader();
        _xmlr.Parse(xmlText);
        _xml = _xmlr.xml["xml"][0];

        /*
            float.Parse("1.25", CultureInfo.InvariantCulture)
            CultureInfo.InvariantCulture - for regional setting of the float number representation
        */
        _multiplier.x = float.Parse(_xml["multiplier"][0].att("x"), CultureInfo.InvariantCulture);
        _multiplier.y = float.Parse(_xml["multiplier"][0].att("y"), CultureInfo.InvariantCulture);

        SlotDef tSD;
        PT_XMLHashList slotsX = _xml["slot"];

        for(int i = 0; i<slotsX.Count; i++)
        {
            tSD = new SlotDef();
            if (slotsX[i].HasAtt("type"))
            {
                tSD.type = slotsX[i].att("type");
            }
            else
            {
                tSD.type = "slot";
            }

            tSD.x = float.Parse(slotsX[i].att("x"), CultureInfo.InvariantCulture);
            tSD.y = float.Parse(slotsX[i].att("y"), CultureInfo.InvariantCulture);
            tSD.layerID = int.Parse(slotsX[i].att("layer"));
            tSD.layerName = _sotringLayerNames[tSD.layerID];

            switch(tSD.type)
            {
                case "slot":
                    tSD.faceUp = (slotsX[i].att("faceup") == "1");
                    tSD.id = int.Parse(slotsX[i].att("id"));

                    if (slotsX[i].HasAtt("hiddenby"))
                    {
                        string[] hiding = slotsX[i].att("hiddenby").Split(',');
                        foreach(string s in hiding)
                        {
                            tSD.hiddenBy.Add(int.Parse(s));
                        }
                    }
                    _slotDefs.Add(tSD);
                    break;

                case "drawpile":
                    tSD.stagger.x = float.Parse(slotsX[i].att("xstagger"), CultureInfo.InvariantCulture);
                    _drawPile = tSD;
                    break;

                case "discardpile":
                    _discardPile = tSD;
                    break;
            }
        }
    }
}

[System.Serializable]
public class SlotDef
{
    public float x;
    public float y;
    public bool faceUp = false;
    public string layerName = "Default";
    public int layerID = 0;
    public int id;
    public List<int> hiddenBy = new List<int>();
    public string type = "slot";
    public Vector2 stagger;
}