﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections.Generic;
using System;

namespace UIWidgets
{
	[Serializable]
	/// <summary>
	/// Resizable event.
	/// </summary>
	public class ResizableEvent : UnityEvent<Resizable>
	{

	}

	/// <summary>
	/// Resizable.
	/// N - north (top).
	/// S - south (bottom).
	/// E - east (right).
	/// W - west (left).
	/// </summary>
	public class Resizable : MonoBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
	{
		[Serializable]
		/// <summary>
		/// Resize directions.
		/// </summary>
		public struct Directions {
			/// <summary>
			/// Allow resize from top.
			/// </summary>
			public bool Top;

			/// <summary>
			/// Allow resize from bottom.
			/// </summary>
			public bool Bottom;

			/// <summary>
			/// Allow resize from left.
			/// </summary>
			public bool Left;

			/// <summary>
			/// Allow resize from right.
			/// </summary>
			public bool Right;

			/// <summary>
			/// Initializes a new instance of the <see cref="UIWidgets.Resizable+Directions"/> struct.
			/// </summary>
			/// <param name="top">If set to <c>true</c> allow resize from top.</param>
			/// <param name="bottom">If set to <c>true</c> allow resize from bottom.</param>
			/// <param name="left">If set to <c>true</c> allow resize from left.</param>
			/// <param name="right">If set to <c>true</c> allow resize from right.</param>
			public Directions(bool top, bool bottom, bool left, bool right)
			{
				Top = top;
				Bottom = bottom;
				Left = left;
				Right = right;
			}
		}

		/// <summary>
		/// Active resize region.
		/// </summary>
		public struct Regions {
			/// <summary>
			/// The top.
			/// </summary>
			public bool Top;

			/// <summary>
			/// The bottom.
			/// </summary>
			public bool Bottom;

			/// <summary>
			/// The left.
			/// </summary>
			public bool Left;

			/// <summary>
			/// The right.
			/// </summary>
			public bool Right;

			/// <summary>
			/// NWSE
			/// </summary>
			/// <value><c>true</c> if cursor mode is NWSE; otherwise, <c>false</c>.</value>
			public bool NWSE {
				get {
					return (Top && Left) || (Bottom && Right);
				}
			}

			/// <summary>
			/// NESW.
			/// </summary>
			/// <value><c>true</c> if cursor mode is NESW; otherwise, <c>false</c>.</value>
			public bool NESW {
				get {
					return (Top && Right) || (Bottom && Left);
				}
			}

			/// <summary>
			/// NS
			/// </summary>
			/// <value><c>true</c> if cursor mode is NS; otherwise, <c>false</c>.</value>
			public bool NS {
				get {
					return (Top && !Right) || (Bottom && !Left);
				}
			}

			/// <summary>
			/// EW.
			/// </summary>
			/// <value><c>true</c> if cursor mode is EW; otherwise, <c>false</c>.</value>
			public bool EW {
				get {
					return (!Top && Right) || (!Bottom && Left);
				}
			}

			/// <summary>
			/// Is any region active.
			/// </summary>
			/// <value><c>true</c> if any region active; otherwise, <c>false</c>.</value>
			public bool Active {
				get {
					return Top || Bottom || Left || Right;
				}
			}

			/// <summary>
			/// Reset this instance.
			/// </summary>
			public void Reset()
			{
				Top = false;
				Bottom = false;
				Left = false;
				Right = false;
			}

			/// <summary>
			/// Returns a string that represents the current object.
			/// </summary>
			/// <returns>A string that represents the current object.</returns>
			public override string ToString()
			{
				return String.Format("Top: {0}; Bottom: {1}; Left: {2}; Right: {3}", Top, Bottom, Left, Right);
			}
		}

		[SerializeField]
		/// <summary>
		/// Is need to update RectTransform on Resize.
		/// </summary>
		public bool UpdateRectTransform = true;

		[SerializeField]
		/// <summary>
		/// Is need to update LayoutElement on Resize.
		/// </summary>
		public bool UpdateLayoutElement = true;

		/// <summary>
		/// The active region in points from left or right border where resize allowed.
		/// </summary>
		[SerializeField]
		[Tooltip("Maximum padding from border where resize active.")]
		public float ActiveRegion = 5;

		[SerializeField]
		/// <summary>
		/// The minimum size.
		/// </summary>
		public Vector2 MinSize;
		
		[SerializeField]
		[Tooltip("Set 0 to unlimit.")]
		/// <summary>
		/// The maximum size.
		/// </summary>
		public Vector2 MaxSize;

		[SerializeField]
		/// <summary>
		/// Resize directions.
		/// </summary>
		public Directions ResizeDirections = new Directions(true, true, true, true);

		/// <summary>
		/// The current camera. For Screen Space - Overlay let it empty.
		/// </summary>
		[SerializeField]
		public Camera CurrentCamera;
		
		/// <summary>
		/// The cursor EW texture.
		/// </summary>
		[SerializeField]
		public Texture2D CursorEWTexture;
		
		/// <summary>
		/// The cursor EW hot spot.
		/// </summary>
		[SerializeField]
		public Vector2 CursorEWHotSpot = new Vector2(16, 16);

		/// <summary>
		/// The cursor NS texture.
		/// </summary>
		[SerializeField]
		public Texture2D CursorNSTexture;
		
		/// <summary>
		/// The cursor NS hot spot.
		/// </summary>
		[SerializeField]
		public Vector2 CursorNSHotSpot = new Vector2(16, 16);
		
		/// <summary>
		/// The cursor NESW texture.
		/// </summary>
		[SerializeField]
		public Texture2D CursorNESWTexture;
		
		/// <summary>
		/// The cursor NESW hot spot.
		/// </summary>
		[SerializeField]
		public Vector2 CursorNESWHotSpot = new Vector2(16, 16);

		/// <summary>
		/// The cursor NWSE texture.
		/// </summary>
		[SerializeField]
		public Texture2D CursorNWSETexture;
		
		/// <summary>
		/// The cursor NWSE hot spot.
		/// </summary>
		[SerializeField]
		public Vector2 CursorNWSEHotSpot = new Vector2(16, 16);

		/// <summary>
		/// The default cursor texture.
		/// </summary>
		[SerializeField]
		public Texture2D DefaultCursorTexture;
		
		/// <summary>
		/// The default cursor hot spot.
		/// </summary>
		[SerializeField]
		public Vector2 DefaultCursorHotSpot;

		/// <summary>
		/// OnStartResize event.
		/// </summary>
		public ResizableEvent OnStartResize = new ResizableEvent();

		/// <summary>
		/// OnEndResize event.
		/// </summary>
		public ResizableEvent OnEndResize = new ResizableEvent();

		RectTransform rectTransform;

		/// <summary>
		/// Gets the RectTransform.
		/// </summary>
		/// <value>RectTransform.</value>
		public RectTransform RectTransform {
			get {
				if (rectTransform==null)
				{
					rectTransform = transform as RectTransform;
				}
				return rectTransform;
			}
		}

		LayoutElement layoutElement;

		/// <summary>
		/// Gets the LayoutElement.
		/// </summary>
		/// <value>LayoutElement.</value>
		public LayoutElement LayoutElement {
			get {
				if (layoutElement==null)
				{
					layoutElement = GetComponent<LayoutElement>() ?? gameObject.AddComponent<LayoutElement>();
				}
				return layoutElement;
			}
		}
		
		Regions regions;
		Regions dragRegions;

		Canvas canvas;
		RectTransform canvasRect;
		
		bool processDrag;

		void Start()
		{
			var layout = GetComponent<HorizontalOrVerticalLayoutGroup>();
			if (layout)
			{
				Utilites.UpdateLayout(layout);
			}
			
			Init();
		}
		
		/// <summary>
		/// Raises the initialize potential drag event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public void OnInitializePotentialDrag(PointerEventData eventData)
		{
			Init();
		}
		
		/// <summary>
		/// Init this instance.
		/// </summary>
		public void Init()
		{
			canvasRect = Utilites.FindCanvas(transform) as RectTransform;
			canvas = canvasRect.GetComponent<Canvas>();
		}
		
		static bool globalCursorSetted;
		bool cursorSetted;
		
		void LateUpdate()
		{
			globalCursorSetted = false;
			if (globalCursorSetted && !cursorSetted)
			{
				return ;
			}

			if (processDrag)
			{
				return ;
			}
			if (!Input.mousePresent)
			{
				return ;
			}
			
			Vector2 point;
			if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, Input.mousePosition, CurrentCamera, out point))
			{
				return ;
			}
			var r = RectTransform.rect;
			if (!r.Contains(point))
			{
				regions.Reset();
				UpdateCursor();
				return ;
			}

			UpdateRegions(point);

			UpdateCursor();
		}

		void UpdateRegions(Vector2 point)
		{
			regions.Top = ResizeDirections.Top && CheckTop(point);
			regions.Bottom = ResizeDirections.Bottom && CheckBottom(point);
			regions.Left = ResizeDirections.Left && CheckLeft(point);
			regions.Right = ResizeDirections.Right && CheckRight(point);
		}

		void UpdateCursor()
		{
			if (regions.NWSE)
			{
				globalCursorSetted = true;
				cursorSetted = true;
				Cursor.SetCursor(CursorNWSETexture, CursorNWSEHotSpot, Utilites.GetCursorMode());
			}
			else if (regions.NESW)
			{
				globalCursorSetted = true;
				cursorSetted = true;
				Cursor.SetCursor(CursorNESWTexture, CursorNESWHotSpot, Utilites.GetCursorMode());
			}
			else if (regions.NS)
			{
				globalCursorSetted = true;
				cursorSetted = true;
				Cursor.SetCursor(CursorNSTexture, CursorNSHotSpot, Utilites.GetCursorMode());
			}
			else if (regions.EW)
			{
				globalCursorSetted = true;
				cursorSetted = true;
				Cursor.SetCursor(CursorEWTexture, CursorEWHotSpot, Utilites.GetCursorMode());
			}
			else if (cursorSetted)
			{
				globalCursorSetted = false;
				cursorSetted = false;
				Cursor.SetCursor(DefaultCursorTexture, DefaultCursorHotSpot, Utilites.GetCursorMode());
			}
		}

		/// <summary>
		/// Checks if point in the top region.
		/// </summary>
		/// <returns><c>true</c>, if point in the top region, <c>false</c> otherwise.</returns>
		/// <param name="point">Point.</param>
		bool CheckTop(Vector2 point)
		{
			var rect = RectTransform.rect;

			rect.position = new Vector2(rect.position.x, rect.position.y + rect.height - ActiveRegion);
			rect.height = ActiveRegion;

			return rect.Contains(point);
		}

		/// <summary>
		/// Checks if point in the right region.
		/// </summary>
		/// <returns><c>true</c>, if right was checked, <c>false</c> otherwise.</returns>
		/// <param name="point">Point.</param>
		bool CheckBottom(Vector2 point)
		{
			var rect = RectTransform.rect;
			rect.height = ActiveRegion;

			return rect.Contains(point);
		}

		/// <summary>
		/// Checks if point in the left region.
		/// </summary>
		/// <returns><c>true</c>, if point in the left region, <c>false</c> otherwise.</returns>
		/// <param name="point">Point.</param>
		bool CheckLeft(Vector2 point)
		{
			var rect = RectTransform.rect;
			rect.width = ActiveRegion;

			return rect.Contains(point);
		}
		
		/// <summary>
		/// Checks if point in the right region.
		/// </summary>
		/// <returns><c>true</c>, if right was checked, <c>false</c> otherwise.</returns>
		/// <param name="point">Point.</param>
		bool CheckRight(Vector2 point)
		{
			var rect = RectTransform.rect;
			
			rect.position = new Vector2(rect.position.x + rect.width - ActiveRegion, rect.position.y);
			rect.width = ActiveRegion;

			return rect.Contains(point);
		}

		/// <summary>
		/// Raises the begin drag event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public void OnBeginDrag(PointerEventData eventData)
		{
			Vector2 point;
			processDrag = false;
			
			if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, eventData.pressPosition, eventData.pressEventCamera, out point))
			{
				return ;
			}
			
			UpdateRegions(point);

			processDrag = regions.Active;

			dragRegions = regions;

			UpdateCursor();

			LayoutElement.preferredHeight = RectTransform.rect.height;
			LayoutElement.preferredWidth = RectTransform.rect.width;

			OnStartResize.Invoke(this);
		}

		void ResetCursor()
		{
			globalCursorSetted = false;
			cursorSetted = false;

			Cursor.SetCursor(DefaultCursorTexture, DefaultCursorHotSpot, Utilites.GetCursorMode());
		}

		/// <summary>
		/// Raises the end drag event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public void OnEndDrag(PointerEventData eventData)
		{
			ResetCursor();

			processDrag = false;

			OnEndResize.Invoke(this);
		}
		
		/// <summary>
		/// Raises the drag event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public void OnDrag(PointerEventData eventData)
		{
			if (!processDrag)
			{
				return ;
			}
			if (canvas==null)
			{
				throw new MissingComponentException(gameObject.name + " not in Canvas hierarchy.");
			}

			Vector2 p1;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, eventData.position, CurrentCamera, out p1);
			Vector2 p2;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, eventData.position - eventData.delta, CurrentCamera, out p2);
			var delta = p1 - p2;

			if (UpdateRectTransform)
			{
				PerformUpdateRectTransform(delta);
			}
			if (UpdateLayoutElement)
			{
				PerformUpdateLayoutElement(delta);
			}
		}

		void PerformUpdateRectTransform(Vector2 delta)
		{
			var pivot = RectTransform.pivot;
			var size = RectTransform.rect.size;
			var sign = new Vector2(1, 1);
			if (dragRegions.Left || dragRegions.Right)
			{
				sign.x = dragRegions.Right ? +1 : -1;
				size.x = Mathf.Max(MinSize.x, size.x + (sign.x * delta.x));
				if (MaxSize.x!=0f)
				{
					size.x = Mathf.Min(MaxSize.x, size.x);
				}
			}
			if (dragRegions.Top || dragRegions.Bottom)
			{
				sign.y = dragRegions.Top ? +1 : -1;
				size.y = Mathf.Max(MinSize.y, size.y + (sign.y * delta.y));
				if (MaxSize.y!=0f)
				{
					size.y = Mathf.Min(MaxSize.y, size.y);
				}
			}
			var anchorSign = new Vector2(dragRegions.Right ? pivot.x : pivot.x - 1, dragRegions.Top ? pivot.y : pivot.y - 1);
			var anchorDelta = size - RectTransform.rect.size;
			anchorDelta = new Vector2(anchorDelta.x * anchorSign.x, anchorDelta.y * anchorSign.y);

			RectTransform.anchoredPosition += anchorDelta;
			RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
			RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
		}

		void PerformUpdateLayoutElement(Vector2 delta)
		{
			if (dragRegions.Left || dragRegions.Right)
			{
				var sign = (dragRegions.Right) ? +1 : -1;
				var width = Mathf.Max(MinSize.x, LayoutElement.preferredWidth + (sign * delta.x));
				if (MaxSize.x!=0f)
				{
					width = Mathf.Min(MaxSize.x, width);
				}
				LayoutElement.preferredWidth = width;
			}
			if (dragRegions.Top || dragRegions.Bottom)
			{
				var sign = (dragRegions.Top) ? +1 : -1;
				var height = Mathf.Max(MinSize.y, LayoutElement.preferredHeight + (sign * delta.y));
				if (MaxSize.y!=0f)
				{
					height = Mathf.Min(MaxSize.y, height);
				}
				LayoutElement.preferredHeight = height;
			}
		}
	}
}