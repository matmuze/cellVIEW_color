using System.Collections.Generic;
using UnityEngine;

public class TreeViewControlEditor : MonoBehaviour
{
    /// <summary>
    /// Public properties
    /// </summary>
    public int X = 0;
    public int Y = 0;
    public int Width = 400;
    public int Height = 400;
    public bool IsExpanded = true;
    public bool IsHoverEnabled = true;
    public bool IsHoverAnimationEnabled = false;
    public bool DisplayOnGame = false;
    public bool DisplayOnScene = true;
    public bool DisplayInInspector = false;
	public int maxWidth=0;
    /// <summary>
    /// The root item
    /// </summary>
    public TreeViewItemEditor MRoomItemEditor = null;
    public TreeViewItemEditor RootItemEditor
    {
        get
        {
            if (null == MRoomItemEditor)
            {
                MRoomItemEditor = new TreeViewItemEditor(this, null) { Header = "Root item" };
            }
            return MRoomItemEditor;
        }
    }
	//callback
	public void toggleInGame(bool value){
		DisplayOnGame = value;
	}
    /// <summary>
    /// Accesses the root item header
    /// </summary>
    public string Header
    {
        get
        {
            return RootItemEditor.Header;
        }
        set
        {
            RootItemEditor.Header = value;
        }
    }

    /// <summary>
    /// Accesses the root data context
    /// </summary>
    public object DataContext
    {
        get
        {
            return RootItemEditor.DataContext;
        }
        set
        {
            RootItemEditor.DataContext = value;
        }
    }

    /// <summary>
    /// Accessor to the root items
    /// </summary>
    public List<TreeViewItemEditor> Items
    {
        get
        {
            return RootItemEditor.Items;
        }
        set
        {
            RootItemEditor.Items = value;
        }
    }

    
    /// <summary>
    /// Skin used by the tree view
    /// </summary>
    public GUISkin m_skinHover = null;
    public GUISkin m_skinUnselected = null;
    public GUISkin m_skinSelected = null;

    /// <summary>
    /// Texture skin references
    /// </summary>
    public Texture2D m_textureBlank = null;
	public Texture2D m_textureGuide = null;
    public Texture2D m_textureLastSiblingCollapsed = null;
    public Texture2D m_textureLastSiblingExpanded = null;
    public Texture2D m_textureLastSiblingNoChild = null;
    public Texture2D m_textureMiddleSiblingCollapsed = null;
    public Texture2D m_textureMiddleSiblingExpanded = null;
    public Texture2D m_textureMiddleSiblingNoChild = null;
	public Texture2D m_textureNormalChecked = null;
	public Texture2D m_textureNormalUnchecked = null;
	public Texture2D m_textureSelectedBackground = null;

    /// <summary>
    /// Force to use the button text
    /// </summary>
    public bool m_forceButtonText = false;

    /// <summary>
    /// Use the default skin
    /// </summary>
    public bool m_forceDefaultSkin = false;

    /// <summary>
    /// The selected item
    /// </summary>
    public TreeViewItemEditor HoverItemEditor = null;
	public TreeViewItemEditor SelectedItemEditor = null;
	
	/// <summary>
	/// Show the button texture 
	/// </summary>
	/// <param name="texture">
	/// A <see cref="Texture2D"/>
	/// </param>
	/// <returns>
	/// A <see cref="System.Boolean"/>
	/// </returns>
	bool ShowButtonTexture(Texture2D texture)
	{
		return GUILayout.Button(texture, GUILayout.MaxWidth(texture.width), GUILayout.MaxHeight(texture.height));
	}

    /// <summary>
    /// Find the button texture/text by enum
    /// </summary>
    /// <param name="item"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public bool Button(TreeViewItemEditor.TextureIcons item)
    {
        switch (item)
        {
            case TreeViewItemEditor.TextureIcons.BLANK:
                if (null == m_textureGuide ||
                    m_forceButtonText)
                {
                    GUILayout.Label("", GUILayout.MaxWidth(4));
                }
                else
                {
                    GUILayout.Label(m_textureBlank, GUILayout.MaxWidth(4), GUILayout.MaxHeight(16));
                }
				return false;
            case TreeViewItemEditor.TextureIcons.GUIDE:
                if (null == m_textureGuide ||
                    m_forceButtonText)
                {
                    GUILayout.Label("|", GUILayout.MaxWidth(16));
                }
                else
                {
                    GUILayout.Label(m_textureGuide, GUILayout.MaxWidth(16), GUILayout.MaxHeight(16));
                }
				return false;
            case TreeViewItemEditor.TextureIcons.LAST_SIBLING_COLLAPSED:
                if (null == m_textureLastSiblingCollapsed ||
                    m_forceButtonText)
                {
                    return GUILayout.Button("<", GUILayout.MaxWidth(16));
                }
                else
                {
                    return ShowButtonTexture(m_textureLastSiblingCollapsed);
                }
            case TreeViewItemEditor.TextureIcons.LAST_SIBLING_EXPANDED:
                if (null == m_textureLastSiblingExpanded ||
                    m_forceButtonText)
                {
                    return GUILayout.Button(">", GUILayout.MaxWidth(16));
                }
                else
                {
                    return ShowButtonTexture(m_textureLastSiblingExpanded);
                }
            case TreeViewItemEditor.TextureIcons.LAST_SIBLING_NO_CHILD:
                if (null == m_textureLastSiblingNoChild ||
                    m_forceButtonText)
                {
                    return GUILayout.Button("-", GUILayout.MaxWidth(16));
                }
                else
                {
                    return GUILayout.Button(m_textureLastSiblingNoChild, GUILayout.MaxWidth(16));
                }
            case TreeViewItemEditor.TextureIcons.MIDDLE_SIBLING_COLLAPSED:
                if (null == m_textureMiddleSiblingCollapsed ||
                    m_forceButtonText)
                {
                    return GUILayout.Button("<", GUILayout.MaxWidth(16));
                }
                else
                {
                    return ShowButtonTexture(m_textureMiddleSiblingCollapsed);
                }
            case TreeViewItemEditor.TextureIcons.MIDDLE_SIBLING_EXPANDED:
                if (null == m_textureMiddleSiblingExpanded ||
                    m_forceButtonText)
                {
                    return GUILayout.Button(">", GUILayout.MaxWidth(16));
                }
                else
                {
                    return GUILayout.Button(m_textureMiddleSiblingExpanded, GUILayout.MaxWidth(16));
                }
            case TreeViewItemEditor.TextureIcons.MIDDLE_SIBLING_NO_CHILD:
                if (null == m_textureMiddleSiblingNoChild ||
                    m_forceButtonText)
                {
                    return GUILayout.Button("-", GUILayout.MaxWidth(16));
                }
                else
                {
                    return ShowButtonTexture(m_textureMiddleSiblingNoChild);
                }
			case TreeViewItemEditor.TextureIcons.NORMAL_CHECKED:
                if (null == m_textureNormalChecked ||
                    m_forceButtonText)
                {
                    return GUILayout.Button("x", GUILayout.MaxWidth(16));
                }
                else
                {
                    return GUILayout.Button(m_textureNormalChecked, GUILayout.MaxWidth(16));
                }
			case TreeViewItemEditor.TextureIcons.NORMAL_UNCHECKED:
                if (null == m_textureNormalUnchecked ||
                    m_forceButtonText)
                {
                    return GUILayout.Button("o", GUILayout.MaxWidth(16));
                }
                else
                {
                    return ShowButtonTexture(m_textureNormalUnchecked);
                }
            default:
                return false;
        }
    }

    /// <summary>
    /// Handle the unity scrolling vector
    /// </summary>
    Vector2 m_scrollView = Vector2.zero;

    public enum DisplayTypes
    {
        NONE, //used by the inspector
        USE_SCROLL_VIEW, //used by panels
        USE_SCROLL_AREA, //used by gameview, sceneview
    }

    /// <summary>
    /// Called from OnGUI or EditorWindow.OnGUI
    /// </summary>
    public void DisplayTreeView(TreeViewControlEditor.DisplayTypes displayType)
    {
        if (!m_forceDefaultSkin)
        {
            GUI.skin = m_skinUnselected;
        }

        switch (displayType)
        {
            case TreeViewControlEditor.DisplayTypes.USE_SCROLL_VIEW:
                m_scrollView = GUILayout.BeginScrollView(m_scrollView, GUILayout.MaxWidth(Width), GUILayout.MaxHeight(Height));
                break;
            case TreeViewControlEditor.DisplayTypes.USE_SCROLL_AREA:
                GUILayout.BeginArea(new Rect(X, Y, Width, Height));
				m_scrollView = GUILayout.BeginScrollView(m_scrollView, GUIStyle.none,GUIStyle.none,GUILayout.MaxWidth(Width), GUILayout.MaxHeight(Height));
                break;
        }

        RootItemEditor.DisplayItem(0, TreeViewItemEditor.SiblingOrder.FIRST_CHILD);

        switch (displayType)
        {
            case TreeViewControlEditor.DisplayTypes.USE_SCROLL_VIEW:
                GUILayout.EndScrollView();
                break;
            case TreeViewControlEditor.DisplayTypes.USE_SCROLL_AREA:
                GUILayout.EndScrollView();
                GUILayout.EndArea();
                break;
        }

        GUI.skin = null;
    }
    
    public bool HasFocus(Vector2 mousePos)
    {
        Rect rect = new Rect(m_scrollView.x-30, m_scrollView.y-30, Width+60, Height+60);
        return rect.Contains(mousePos);
    }	

    void Start()
    {
        SelectedItemEditor = null;
    }
    
    void OnGUI()
    {
        if (!DisplayOnGame)
        {
            return;
        }
        DisplayTreeView(TreeViewControlEditor.DisplayTypes.USE_SCROLL_AREA);
    }
}