using System;
using System.Collections.Generic;
using UnityEngine;

//[Serializable]
public class TreeViewItemEditor
{
    public string Header = string.Empty;
    public bool IsExpanded = true;
    public bool IsCheckBox = false;
    public bool IsChecked = false;
    public bool IsHover = false;
    public bool IsSelected = false;
    public List<TreeViewItemEditor> Items = new List<TreeViewItemEditor>();

    public TreeViewControlEditor ParentControlEditor = null;
    public TreeViewItemEditor Parent = null;
    public object DataContext = null;

	public int anid = 0; //additional integer flag


    public class ClickEventArgs : System.EventArgs
    {
    }
    public EventHandler Click = null;

    public class CheckedEventArgs : System.EventArgs
    {
    }
    public EventHandler Checked = null;

    public class UncheckedEventArgs : System.EventArgs
    {
    }
    public EventHandler Unchecked = null;

    public class SelectedEventArgs : System.EventArgs
    {
    }
    public EventHandler Selected = null;

    public class UnselectedEventArgs : System.EventArgs
    {
    }
    public EventHandler Unselected = null;

    /// <summary>
    /// The distance to the hover item
    /// </summary>
    float m_hoverTime = 0f;

    public TreeViewItemEditor(TreeViewControlEditor parentControlEditor, TreeViewItemEditor parent)
    {
        ParentControlEditor = parentControlEditor;
        Parent = parent;

        if (null == parentControlEditor)
        {
            return;
        }
    }

    public TreeViewItemEditor AddItem(string header)
    {
        TreeViewItemEditor itemEditor = new TreeViewItemEditor(ParentControlEditor, this) { Header = header };
        Items.Add(itemEditor);
        return itemEditor;
    }

    public TreeViewItemEditor AddItem(string header, bool isExpanded)
    {
        TreeViewItemEditor itemEditor = new TreeViewItemEditor(ParentControlEditor, this) { Header = header, IsExpanded = isExpanded };
        Items.Add(itemEditor);
        return itemEditor;
    }

    public TreeViewItemEditor AddItem(string header, bool isExpanded, bool isChecked)
    {
        TreeViewItemEditor itemEditor = new TreeViewItemEditor(ParentControlEditor, this) { Header = header, IsExpanded = isExpanded, IsCheckBox = true, IsChecked = isChecked };
        Items.Add(itemEditor);
        return itemEditor;
    }

    public bool HasChildItems()
    {
        if (null == Items ||
            Items.Count == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public enum SiblingOrder
    {
        FIRST_CHILD,
        MIDDLE_CHILD,
        LAST_CHILD,
    }

    public enum TextureIcons
    {
        BLANK,
        GUIDE,
        LAST_SIBLING_COLLAPSED,
        LAST_SIBLING_EXPANDED,
        LAST_SIBLING_NO_CHILD,
        MIDDLE_SIBLING_COLLAPSED,
        MIDDLE_SIBLING_EXPANDED,
        MIDDLE_SIBLING_NO_CHILD,
        NORMAL_CHECKED,
        NORMAL_UNCHECKED,
    }

    float CalculateHoverTime(Rect rect, Vector3 mousePos)
    {
        if (rect.Contains(mousePos))
        {
            return 0f;
        }
        float midPoint = (rect.yMin + rect.yMax) * 0.5f;
        float pointA = mousePos.y;
        return Mathf.Abs(midPoint - pointA) / 50f;
    }

    public void DisplayItem(int levels, SiblingOrder siblingOrder)
    {
		Rect lastRect;
		int offset = ParentControlEditor.m_textureGuide.width + ParentControlEditor.m_textureNormalChecked.width;
        if (null == ParentControlEditor)
        {
            return;
        }

        GUILayout.BeginHorizontal();

        for (int index = 0; index < levels; ++index)
        {
            ParentControlEditor.Button(TextureIcons.GUIDE);
        }

        if (!HasChildItems())
        {
            bool result;
            switch (siblingOrder)
            {
                case SiblingOrder.FIRST_CHILD:
                    result = ParentControlEditor.Button(TextureIcons.MIDDLE_SIBLING_NO_CHILD);
                    break;
                case SiblingOrder.MIDDLE_CHILD:
                    result = ParentControlEditor.Button(TextureIcons.MIDDLE_SIBLING_NO_CHILD);
                    break;
                case SiblingOrder.LAST_CHILD:
                    result = ParentControlEditor.Button(TextureIcons.LAST_SIBLING_NO_CHILD);
                    break;
                default:
                    result = false;
                    break;
            }
            if (result)
            {
                IsExpanded = !IsExpanded;
            }
        }
        else
        {
            if (IsExpanded)
            {
                bool result;
                switch (siblingOrder)
                {
                    case SiblingOrder.FIRST_CHILD:
                        result = ParentControlEditor.Button(TextureIcons.MIDDLE_SIBLING_EXPANDED);
                        break;
                    case SiblingOrder.MIDDLE_CHILD:
                        result = ParentControlEditor.Button(TextureIcons.MIDDLE_SIBLING_EXPANDED);
                        break;
                    case SiblingOrder.LAST_CHILD:
                        result = ParentControlEditor.Button(TextureIcons.LAST_SIBLING_EXPANDED);
                        break;
                    default:
                        result = false;
                        break;
                }
                if (result)
                {
                    IsExpanded = !IsExpanded;
                }
            }
            else
            {
                bool result;
                switch (siblingOrder)
                {
                    case SiblingOrder.FIRST_CHILD:
                        result = ParentControlEditor.Button(TextureIcons.MIDDLE_SIBLING_COLLAPSED);
                        break;
                    case SiblingOrder.MIDDLE_CHILD:
                        result = ParentControlEditor.Button(TextureIcons.MIDDLE_SIBLING_COLLAPSED);
                        break;
                    case SiblingOrder.LAST_CHILD:
                        result = ParentControlEditor.Button(TextureIcons.LAST_SIBLING_COLLAPSED);
                        break;
                    default:
                        result = false;
                        break;
                }
                if (result)
                {
                    IsExpanded = !IsExpanded;
                }
            }
        }

        // display the text for the tree view
		//also compute the size max>
        if (!string.IsNullOrEmpty(Header))
        {
            bool isSelected;
            if (ParentControlEditor.SelectedItemEditor == this &&
                !ParentControlEditor.m_forceDefaultSkin)
            {
                //use selected skin
                GUI.skin = ParentControlEditor.m_skinSelected;
                isSelected = true;
            }
            else
            {
                isSelected = false;
            }

            if (IsCheckBox)
            {
                if (IsChecked &&
                    ParentControlEditor.Button(TextureIcons.NORMAL_CHECKED))
                {
                    IsChecked = false;
                    if (ParentControlEditor.SelectedItemEditor != this)
                    {
                        ParentControlEditor.SelectedItemEditor = this;
                        this.IsSelected = true;
                        if (null != Selected)
                        {
                            Selected.Invoke(this, new SelectedEventArgs());
                        }
                    }
                    if (null != Unchecked)
                    {
                        Unchecked.Invoke(this, new UncheckedEventArgs());
                    }
                }
                else if (!IsChecked &&
                    ParentControlEditor.Button(TextureIcons.NORMAL_UNCHECKED))
                {
                    IsChecked = true;
                    if (ParentControlEditor.SelectedItemEditor != this)
                    {
                        ParentControlEditor.SelectedItemEditor = this;
                        this.IsSelected = true;
                        if (null != Selected)
                        {
                            Selected.Invoke(this, new SelectedEventArgs());
                        }
                    }
                    if (null != Checked)
                    {
                        Checked.Invoke(this, new CheckedEventArgs());
                    }
                }

                ParentControlEditor.Button(TextureIcons.BLANK);
            }

            if (ParentControlEditor.IsHoverEnabled)
            {
                GUISkin oldSkin = GUI.skin;
                if (isSelected)
                {
                    GUI.skin = ParentControlEditor.m_skinSelected;
                }
                else if (IsHover)
                {
                    GUI.skin = ParentControlEditor.m_skinHover;
                }
                else
                {
                    GUI.skin = ParentControlEditor.m_skinUnselected;
                }
                if (ParentControlEditor.IsHoverAnimationEnabled)
                {
                    GUI.skin.button.fontSize = (int)Mathf.Lerp(12f, 12f, m_hoverTime);
                }
                GUI.SetNextControlName("toggleButton"); //workaround to dirty GUI
				//var size : Vector2 = GUI.skin.GetStyle("ProgressBarText").CalcSize(GUIContent(label));
				Vector2 size = GUI.skin.label.CalcSize(new GUIContent(Header));
				if (size.x+(levels*offset)+15 > ParentControlEditor.maxWidth) ParentControlEditor.maxWidth = (int)size.x+(levels*offset)+15;
				//Debug.Log (size.x.ToString()+" size and level*offset "+(levels*offset).ToString());
                if (GUILayout.Button(Header))
                {
                    GUI.FocusControl("toggleButton"); //workaround to dirty GUI
                    if (ParentControlEditor.SelectedItemEditor != this)
                    {
                        ParentControlEditor.SelectedItemEditor = this;
                        this.IsSelected = true;
                        if (null != Selected)
                        {
                            Selected.Invoke(this, new SelectedEventArgs());
                        }
                    }
                    if (null != Click)
                    {
                        Click.Invoke(this, new ClickEventArgs());
                    }
                }
                GUI.skin = oldSkin;
            }
            else
            {
                GUI.SetNextControlName("toggleButton"); //workaround to dirty GUI
				Vector2 size = GUI.skin.label.CalcSize(new GUIContent(Header));
				if (size.x+(levels*offset)+15 > ParentControlEditor.maxWidth) ParentControlEditor.maxWidth = (int)size.x+(levels*offset)+15;
				if (GUILayout.Button(Header))
                {
					GUI.FocusControl("toggleButton"); //workaround to dirty GUI
                    if (ParentControlEditor.SelectedItemEditor != this)
                    {
                        ParentControlEditor.SelectedItemEditor = this;
                        this.IsSelected = true;
                        if (null != Selected)
                        {
                            Selected.Invoke(this, new SelectedEventArgs());
                        }
                    }
                    if (null != Click)
                    {
                        Click.Invoke(this, new ClickEventArgs());
                    }
                }
            }

            if (isSelected &&
                !ParentControlEditor.m_forceDefaultSkin)
            {
                //return to default skin
                GUI.skin = ParentControlEditor.m_skinUnselected;
            }
        }

        GUILayout.EndHorizontal();

        if (ParentControlEditor.IsHoverEnabled)
        {
            if (null != Event.current &&
                Event.current.type == EventType.Repaint)
            {
                Vector2 mousePos = Event.current.mousePosition;
                if (ParentControlEditor.HasFocus(mousePos))
                {
                    lastRect = GUILayoutUtility.GetLastRect();
                    if (lastRect.Contains(mousePos))
                    {
                        IsHover = true;
                        ParentControlEditor.HoverItemEditor = this;
                    }
                    else
                    {
                        IsHover = false;
                    }
                    if (ParentControlEditor.IsHoverEnabled &&
                        ParentControlEditor.IsHoverAnimationEnabled)
                    {
                        m_hoverTime = CalculateHoverTime(lastRect, Event.current.mousePosition);
                    }
                }
            }
        }

        if (HasChildItems() &&
            IsExpanded)
        {
            for (int index = 0; index < Items.Count; ++index)
            {
                TreeViewItemEditor child = Items[index];
                child.Parent = this;
                if ((index + 1) == Items.Count)
                {
                    child.DisplayItem(levels + 1, SiblingOrder.LAST_CHILD);
                }
                else if (index == 0)
                {
                    child.DisplayItem(levels + 1, SiblingOrder.FIRST_CHILD);
                }
                else
                {
                    child.DisplayItem(levels + 1, SiblingOrder.MIDDLE_CHILD);
                }
            }
        }

        if (IsSelected &&
            ParentControlEditor.SelectedItemEditor != this)
        {
            if (null != Unselected)
            {
                Unselected.Invoke(this, new UnselectedEventArgs());
            }
        }
        IsSelected = ParentControlEditor.SelectedItemEditor == this;
    }
}