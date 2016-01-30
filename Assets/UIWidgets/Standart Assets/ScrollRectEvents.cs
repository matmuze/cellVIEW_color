﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

namespace UIWidgets
{
	[RequireComponent(typeof(ScrollRect))]
	/// <summary>
	/// ScrollRect events.
	/// </summary>
	public class ScrollRectEvents : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
		[SerializeField]
		/// <summary>
		/// The required movement before raise events.
		/// </summary>
		public float RequiredMovement = 50f;

		[SerializeField]
		/// <summary>
		/// OnPullUp event.
		/// </summary>
		public UnityEvent OnPullUp = new UnityEvent();

		[SerializeField]
		/// <summary>
		/// OnPullDown event.
		/// </summary>
		public UnityEvent OnPullDown = new UnityEvent();

		[SerializeField]
		/// <summary>
		/// OnPullLeft event.
		/// </summary>
		public UnityEvent OnPullLeft = new UnityEvent();
		
		[SerializeField]
		/// <summary>
		/// OnPullRight event.
		/// </summary>
		public UnityEvent OnPullRight = new UnityEvent();

		ScrollRect scrollRect;

		/// <summary>
		/// Gets the ScrollRect.
		/// </summary>
		/// <value>ScrollRect.</value>
		public ScrollRect ScrollRect {
			get {
				if (scrollRect==null)
				{
					scrollRect = GetComponent<ScrollRect>();
				}
				return scrollRect;
			}
		}

		bool initedPullUp;
		bool initedPullDown;
		bool initedPullLeft;
		bool initedPullRight;

		float MovementUp;
		float MovementDown;
		float MovementLeft;
		float MovementRight;

		/// <summary>
		/// Called by a BaseInputModule before a drag is started.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public virtual void OnBeginDrag(PointerEventData eventData)
		{
			initedPullUp = false;
			initedPullDown = false;
			initedPullLeft = false;
			initedPullRight = false;

			MovementUp = 0f;
			MovementDown = 0f;
			MovementLeft = 0f;
			MovementRight = 0f;
		}

		/// <summary>
		/// Called by a BaseInputModule when a drag is ended.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public virtual void OnEndDrag(PointerEventData eventData)
		{
			initedPullUp = false;
			initedPullDown = false;
			initedPullLeft = false;
			initedPullRight = false;
			
			MovementUp = 0f;
			MovementDown = 0f;
			MovementLeft = 0f;
			MovementRight = 0f;
		}

		/// <summary>
		/// When draging is occuring this will be called every time the cursor is moved.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public virtual void OnDrag(PointerEventData eventData)
		{
			var scrollRectTransform = (ScrollRect.transform as RectTransform);
			var scroll_height = scrollRectTransform.rect.height;
			var scroll_width = scrollRectTransform.rect.width;

			var max_y = Mathf.Max(0f, ScrollRect.content.rect.height - scroll_height);
			var max_x = Mathf.Max(0f, ScrollRect.content.rect.width - scroll_width);

			if ((ScrollRect.content.anchoredPosition.y <= 0f) && (!initedPullUp))
			{
				MovementUp += -eventData.delta.y;
				if (MovementUp >= RequiredMovement)
				{
					initedPullUp = true;
					OnPullUp.Invoke();
				}
			}

			if ((ScrollRect.content.anchoredPosition.y >= max_y) && (!initedPullDown))
			{
				MovementDown += eventData.delta.y;
				if (MovementDown >= RequiredMovement)
				{
					initedPullDown = true;
					OnPullDown.Invoke();
				}
			}

			if ((ScrollRect.content.anchoredPosition.x <= 0f) && (!initedPullLeft))
			{
				MovementLeft += -eventData.delta.x;
				if (MovementLeft >= RequiredMovement)
				{
					initedPullLeft = true;
					OnPullLeft.Invoke();
				}
			}
			
			if ((ScrollRect.content.anchoredPosition.x >= max_x) && (!initedPullRight))
			{
				MovementRight += eventData.delta.x;
				if (MovementRight >= RequiredMovement)
				{
					initedPullRight = true;
					OnPullRight.Invoke();
				}
			}

		}
	}
}