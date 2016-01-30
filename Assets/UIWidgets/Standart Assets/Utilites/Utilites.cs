﻿using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;

namespace UIWidgets {

	/// <summary>
	/// Utilites.
	/// </summary>
	static public class Utilites {
#if UNITY_EDITOR
		/// <summary>
		/// Creates the object by path to asset prefab.
		/// </summary>
		/// <returns>The created object.</returns>
		/// <param name="path">Path.</param>
		/// <param name="parent">Parent.</param>
		static public GameObject CreateObject(string path, Transform parent=null)
		{
			var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
			if (prefab==null)
			{
				throw new ArgumentException(string.Format("Prefab not found at path {0}.", path));
			}

			var go = UnityEngine.Object.Instantiate(prefab) as GameObject;

			var go_parent = parent ?? Selection.activeTransform;
			if ((go_parent==null) || ((go_parent.gameObject.transform as RectTransform)==null))
			{
				go_parent = UnityEngine.Object.FindObjectOfType<Canvas>().transform;
			}

			if (go_parent!=null)
			{
				go.transform.SetParent(go_parent, false);
			}

			go.name = prefab.name;

			var rectTransform = go.transform as RectTransform;
			if (rectTransform!=null)
			{
				rectTransform.anchoredPosition = new Vector2(0, 0);
			}

			FixInstantiated(prefab, go);

			return go;
		}

		/// <summary>
		/// Creates the object from asset.
		/// </summary>
		/// <returns>The object from asset.</returns>
		/// <param name="path">Path.</param>
		static public GameObject CreateObjectFromAsset(string searchString, string undoName)
		{
			var prefab = AssetDatabase.FindAssets(searchString);
			if (prefab.Length==0)
			{
				return null;
			}
			var go = Utilites.CreateObject(AssetDatabase.GUIDToAssetPath(prefab[0]));
			Undo.RegisterCreatedObjectUndo(go, undoName);

			return go;
		}

		static public GameObject CreateWidgetFromAsset(string widget)
		{
			return CreateObjectFromAsset("l:Uiwidgets" + widget + "Prefab", "Create " + widget);
		}
		#endif

		/// <summary>
		/// Fixs the instantiated object (in some cases object have wrong position, rotation and scale).
		/// </summary>
		/// <param name="source">Source.</param>
		/// <param name="instance">Get.</param>
		static public void FixInstantiated(Component source, Component instance)
		{
			FixInstantiated(source.gameObject, instance.gameObject);
		}

		/// <summary>
		/// Fix the instantiated object (in some cases object have wrong position, rotation and scale).
		/// </summary>
		/// <param name="source">Source.</param>
		/// <param name="instance">Get.</param>
		static public void FixInstantiated(GameObject source, GameObject instance)
		{
			var defaultRectTransform = source.transform as RectTransform;

			var rectTransform = instance.transform as RectTransform;

			rectTransform.localPosition = defaultRectTransform.localPosition;
			rectTransform.position = defaultRectTransform.position;
			rectTransform.rotation = defaultRectTransform.rotation;
			rectTransform.localScale = defaultRectTransform.localScale;
			rectTransform.anchoredPosition = defaultRectTransform.anchoredPosition;
			rectTransform.sizeDelta = defaultRectTransform.sizeDelta;
		}

		/// <summary>
		/// Finds the canvas.
		/// </summary>
		/// <returns>The canvas.</returns>
		/// <param name="currentObject">Current object.</param>
		static public Transform FindCanvas(Transform currentObject)
		{
			var canvas = currentObject.GetComponentInParent<Canvas>();
			if (canvas==null)
			{
				return null;
			}
			return canvas.transform;
		}

		/// <summary>
		/// Calculates the drag position.
		/// </summary>
		/// <returns>The drag position.</returns>
		/// <param name="screenPosition">Screen position.</param>
		/// <param name="canvas">Canvas.</param>
		/// <param name="canvasRect">Canvas rect.</param>
		static public Vector3 CalculateDragPosition(Vector3 screenPosition, Canvas canvas, RectTransform canvasRect)
		{
			Vector3 result;
			var canvasSize = canvasRect.sizeDelta;
			Vector2 min = Vector2.zero;
			Vector2 max = canvasSize;
			
			var isOverlay = canvas.renderMode == RenderMode.ScreenSpaceOverlay;
			var noCamera = canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null;
			if (isOverlay || noCamera)
			{
				result = screenPosition;
			}
			else
			{
				var ray = canvas.worldCamera.ScreenPointToRay(screenPosition);
				var plane = new Plane(canvasRect.forward, canvasRect.position);
				
				float distance;
				plane.Raycast(ray, out distance);
				
				result = canvasRect.InverseTransformPoint(ray.origin + (ray.direction * distance));
				
				min = - Vector2.Scale(max, canvasRect.pivot);
				max = canvasSize - min;
			}
			
			result.x = Mathf.Clamp(result.x, min.x, max.y);
			result.y = Mathf.Clamp(result.y, min.x, max.y);
			
			return result;
		}

		static public CursorMode GetCursorMode()
		{
			#if UNITY_WEBGL
			return CursorMode.ForceSoftware;
			#else
			return CursorMode.Auto;
			#endif
		}

		static public void UpdateLayout(LayoutGroup layout)
		{
			layout.CalculateLayoutInputHorizontal();
			layout.SetLayoutHorizontal();
			layout.CalculateLayoutInputVertical();
			layout.SetLayoutVertical();
		}
	}
}