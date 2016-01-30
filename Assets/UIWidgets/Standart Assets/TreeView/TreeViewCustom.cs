using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using System;
using System.Linq;

namespace UIWidgets {
	/// <summary>
	/// Tree view custom.
	/// </summary>
	public class TreeViewCustom<TComponent,TItem> : ListViewCustom<TComponent,ListNode<TItem>> where TComponent : ListViewItem
	{
		[System.Serializable]
		public class NodeEvent : UnityEvent<TreeNode<TItem>>
		{

		}

		[SerializeField]
		IObservableList<TreeNode<TItem>> nodes = new ObservableList<TreeNode<TItem>>();

		/// <summary>
		/// Gets or sets the nodes.
		/// </summary>
		/// <value>The nodes.</value>
		public IObservableList<TreeNode<TItem>> Nodes {
			get {
				return nodes;
			}
			set {
				if (nodes!=null)
				{
					nodes.OnChange -= NodesChanged;
				}
				nodes = value;
				SetScrollValue(0f);
				Refresh();
				if (nodes!=null)
				{
					nodes.OnChange += NodesChanged;
				}
			}
		}

		/// <summary>
		/// Gets the selected node.
		/// </summary>
		/// <value>The selected node.</value>
		public TreeNode<TItem> SelectedNode {
			get {
				var n = SelectedNodes.Count;
				if (n > 0)
				{
					return SelectedNodes[n - 1];
				}
				return null;
			}
		}

		/// <summary>
		/// NodeToggle event.
		/// </summary>
		public NodeEvent NodeToggle = new NodeEvent();

		/// <summary>
		/// NodeSelected event.
		/// </summary>
		public NodeEvent NodeSelected = new NodeEvent();

		/// <summary>
		/// NodeDeselected event.
		/// </summary>
		public NodeEvent NodeDeselected = new NodeEvent();

		/// <summary>
		/// Gets or sets the selected nodes.
		/// </summary>
		/// <value>The selected nodes.</value>
		public List<TreeNode<TItem>> SelectedNodes {
			get {
				if (SelectedIndex==-1)
				{
					return null;
				}
				return SelectedIndicies.ConvertAll(x => NodesList[x].Node);
			}
			set {
				SelectedIndicies = Nodes2Indicies(value);
			}
		}

		/// <summary>
		/// Opened nodes converted to list.
		/// </summary>
		protected ObservableList<ListNode<TItem>> NodesList = new ObservableList<ListNode<TItem>>();

		/// <summary>
		/// Awake this instance.
		/// </summary>
		protected override void Awake()
		{
			Start();
		}
		
		[System.NonSerialized]
		bool isStartedTreeViewCustom = false;
		
		/// <summary>
		/// Start this instance.
		/// </summary>
		public override void Start()
		{
			if (isStartedTreeViewCustom)
			{
				return ;
			}
			isStartedTreeViewCustom = true;

			setContentSizeFitter = false;

			base.Start();

			Refresh();

			OnSelect.AddListener(OnSelectNode);
			OnDeselect.AddListener(OnDeselectNode);
			KeepSelection = false;

			DataSource = NodesList;
		}

		/// <summary>
		/// Convert nodes tree to list.
		/// </summary>
		/// <returns>The list.</returns>
		/// <param name="sourceNodes">Source nodes.</param>
		/// <param name="depth">Depth.</param>
		/// <param name="list">List.</param>
		protected virtual int Nodes2List(IObservableList<TreeNode<TItem>> sourceNodes, int depth, ObservableList<ListNode<TItem>> list)
		{
			var added_nodes = 0;
			foreach (var node in sourceNodes)
			{
				if (!node.IsVisible)
				{
					continue ;
				}
				list.Add(new ListNode<TItem>(node, depth));
				if ((node.IsExpanded) && (node.Nodes!=null) && (node.Nodes.Count > 0))
				{
					var used = Nodes2List(node.Nodes, depth + 1, list);
					node.UsedNodesCount = used;
				}
				else
				{
					node.UsedNodesCount = 0;
				}
				added_nodes += 1;
			}
			return added_nodes;
		}

		/// <summary>
		/// Raises the toggle node event.
		/// </summary>
		/// <param name="index">Index.</param>
		protected void OnToggleNode(int index)
		{
			ToggleNode(index);
			NodeToggle.Invoke(NodesList[index].Node);
		}

		/// <summary>
		/// Raises the select node event.
		/// </summary>
		/// <param name="index">Index.</param>
		/// <param name="component">Component.</param>
		protected void OnSelectNode(int index, ListViewItem component)
		{
			NodeSelected.Invoke(NodesList[index].Node);
		}

		/// <summary>
		/// Raises the deselect node event.
		/// </summary>
		/// <param name="index">Index.</param>
		/// <param name="component">Component.</param>
		protected void OnDeselectNode(int index, ListViewItem component)
		{
			NodeDeselected.Invoke(NodesList[index].Node);
		}

		/// <summary>
		/// Toggles the node.
		/// </summary>
		/// <param name="index">Index.</param>
		protected void ToggleNode(int index)
		{
			var node = NodesList[index];

			if (node.Node.Nodes==null)
			{
				return ;
			}

			NodesList.BeginUpdate();
			if (node.Node.IsExpanded)
			{


				var range = node.Node.AllUsedNodesCount;
				if (range > 0)
				{
					MoveSelectedIndiciesUp(index, range);

					NodesList.RemoveRange(index + 1, range);
				}

				node.Node.PauseObservation = true;
				node.Node.IsExpanded = false;
				node.Node.PauseObservation = false;

				node.Node.UsedNodesCount = 0;
			}
			else
			{
				var sub_list = new ObservableList<ListNode<TItem>>();
				var used = Nodes2List(node.Node.Nodes, node.Depth + 1, sub_list);

				MoveSelectedIndiciesDown(index, sub_list.Count);

				NodesList.InsertRange(index + 1, sub_list);

				node.Node.PauseObservation = true;
				node.Node.IsExpanded = true;
				node.Node.PauseObservation = false;

				node.Node.UsedNodesCount = used;
			}
			NodesList.EndUpdate();
		}

		/// <summary>
		/// Moves the selected indicies up.
		/// </summary>
		/// <param name="index">Index.</param>
		/// <param name="range">Range.</param>
		void MoveSelectedIndiciesUp(int index, int range)
		{
			var start = index + 1;
			var end = start + range;

			//deselect indicies in removed range
			SelectedIndicies.Where(x => start <= x && x <= end).ForEach(Deselect);

			var remove_indicies = SelectedIndicies.Where(x => x > end).ToList();
			var add_indicies = remove_indicies.Select(x => x - range).ToList();

			SilentDeselect(remove_indicies);
			SilentSelect(add_indicies);
		}

		/// <summary>
		/// Moves the selected indicies down.
		/// </summary>
		/// <param name="index">Index.</param>
		/// <param name="range">Range.</param>
		void MoveSelectedIndiciesDown(int index, int range)
		{
			var start = index + 1;
			var end = start + range;
			var remove_indicies = SelectedIndicies.Where(x => start <= x && x <= end).ToList();
			var add_indicies = remove_indicies.ConvertAll<int>(x => x + range);

			SilentDeselect(remove_indicies);
			SilentSelect(add_indicies);
		}

		/// <summary>
		/// Get indicies of specified nodes.
		/// </summary>
		/// <returns>The indicies.</returns>
		/// <param name="targetNodes">Target nodes.</param>
		protected List<int> Nodes2Indicies(IEnumerable<TreeNode<TItem>> targetNodes)
		{
			return targetNodes.Select(x => NodesList.FindIndex(y => x==y.Node)).Where(i => i!=-1).ToList();
		}

		/// <summary>
		/// Process changes in node data.
		/// </summary>
		protected virtual void NodesChanged()
		{
			Refresh();
		}

		/// <summary>
		/// Refresh this instance.
		/// </summary>
		public virtual void Refresh()
		{
			if (nodes==null)
			{
				NodesList.Clear();

				return ;
			}

			NodesList.BeginUpdate();

			var selected_nodes = SelectedNodes;
			NodesList.Clear();

			Nodes2List(nodes, 0, NodesList);

			if (selected_nodes!=null)
			{
				SilentDeselect(SelectedIndicies);
				
				var indicies = Nodes2Indicies(selected_nodes);
				SilentSelect(indicies);
			}

			NodesList.EndUpdate();
		}

		/// <summary>
		/// Sets component data with specified item.
		/// </summary>
		/// <param name="component">Component.</param>
		/// <param name="item">Item.</param>
		protected override void SetData(TComponent component, ListNode<TItem> item)
		{

		}

		/// <summary>
		/// Removes the callback.
		/// </summary>
		/// <param name="item">Item.</param>
		/// <param name="index">Index.</param>
		protected override void RemoveCallback(ListViewItem item, int index)
		{
			if (item!=null)
			{
				(item as TreeViewComponentBase<TItem>).ToggleEvent.RemoveListener(OnToggleNode);
			}

			base.RemoveCallback(item, index);
		}

		/// <summary>
		/// Adds the callback.
		/// </summary>
		/// <param name="item">Item.</param>
		/// <param name="index">Index.</param>
		protected override void AddCallback(ListViewItem item, int index)
		{
			base.AddCallback(item, index);

			(item as TreeViewComponentBase<TItem>).ToggleEvent.AddListener(OnToggleNode);
		}

		/// <summary>
		/// This function is called when the MonoBehaviour will be destroyed.
		/// </summary>
		protected override void OnDestroy()
		{
			OnSelect.RemoveListener(OnSelectNode);
			OnDeselect.RemoveListener(OnDeselectNode);

			if (Nodes!=null)
			{
				Nodes.Dispose();
				Nodes = null;
			}

			base.OnDestroy();
		}
	}
}