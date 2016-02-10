﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ArrowScript : MonoBehaviour, IPointerClickHandler
{
    public bool Folded = false;
    public bool Visible = true;

    public float Size = 0;
    public float Alpha = 0;

	public Sprite ArrowClosed;
	public Sprite ArrowOpen;

    //public void Init()
    //{
    //    SetState(true);
    //    //SetEnabled(Visible);
    //    SetAlpha(0.5f);
    //    //SetSize(Size);
    //}

    public void SetState(bool value)
    {
        var im = this.transform.GetComponent<Image>();
        im.overrideSprite = (value == false) ? ArrowOpen : ArrowClosed;
        Folded = value;
    }

    public void SetEnabled(bool value)
    {
        gameObject.SetActive(value);
        Visible = value;
    }

    public void SetAlpha(float value)
    {
        var img = this.transform.GetComponent<CanvasRenderer>();
        img.SetAlpha(value);
        Alpha = value;
    }

    //public void SetSize(float value)
    //{
    //    var rt = this.gameObject.GetComponent<RectTransform>();
    //    rt.pivot = new Vector2(.5f, .5f);
    //    rt.localPosition = new Vector3(value / 2, -(value / 2), 0);
    //    rt.sizeDelta = new Vector2(value, value);
    //    Size = value;
    //}

    public delegate void DropDownClick(bool open);
    public event DropDownClick DropDownToggle;

    public void OnPointerClick(PointerEventData ped)
	{
        if (ped.pointerId == -1 && Visible)
        {
            SetState(!Folded);
            if (DropDownToggle != null) DropDownToggle(Folded);
        }
	}
}
