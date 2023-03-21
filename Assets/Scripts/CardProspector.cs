using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardProspector : Card
{
    [Header("Parameters: CardProspector")]
    public ECardState state = ECardState.drawpile;
    public List<CardProspector> hiddenBy = new List<CardProspector>();
    public int layoutID;
    public SlotDef slotDef;

    public override void OnMouseUpAsButton()
    {
        Prospector.S.CardClicked(this);
        base.OnMouseUpAsButton();
    }
}

public enum ECardState
{
    drawpile,
    tableau,
    target,
    discard
}