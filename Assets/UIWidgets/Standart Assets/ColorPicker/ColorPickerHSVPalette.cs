using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

namespace UIWidgets {
	/// <summary>
	/// Color picker HSV palette.
	/// </summary>
	public class ColorPickerHSVPalette : MonoBehaviour {
		[SerializeField]
		Image palette;
		
		RectTransform paletteRect;
		OnDragListener dragListener;

		/// <summary>
		/// Gets or sets the palette.
		/// </summary>
		/// <value>The palette.</value>
		public Image Palette {
			get {
				return palette;
			}
			set {
				if (dragListener!=null)
				{
					dragListener.OnDragEvent.RemoveListener(OnDrag);
				}
				palette = value;
				if (palette!=null)
				{
					paletteRect = palette.transform as RectTransform;
					dragListener = palette.GetComponent<OnDragListener>() ?? palette.gameObject.AddComponent<OnDragListener>();
					dragListener.OnDragEvent.AddListener(OnDrag);
					UpdateMaterial();
				}
				else
				{
					paletteRect = null;
				}
			}
		}
		
		[SerializeField]
		Shader paletteShader;

		/// <summary>
		/// Gets or sets the shader to display gradient in palette.
		/// </summary>
		/// <value>The palette shader.</value>
		public Shader PaletteShader {
			get {
				return paletteShader;
			}
			set {
				paletteShader = value;
				UpdateMaterial();
			}
		}
		
		[SerializeField]
		RectTransform paletteCursor;

		/// <summary>
		/// Gets or sets the palette cursor.
		/// </summary>
		/// <value>The palette cursor.</value>
		public RectTransform PaletteCursor {
			get {
				return paletteCursor;
			}
			set {
				paletteCursor = value;
				if (paletteCursor!=null)
				{
					UpdateView();
				}
			}
		}
		
		[SerializeField]
		Slider slider;

		/// <summary>
		/// Gets or sets the slider.
		/// </summary>
		/// <value>The slider.</value>
		public Slider Slider {
			get {
				return slider;
			}
			set {
				if (slider!=null)
				{
					slider.onValueChanged.RemoveListener(SliderValueChanged);
				}
				slider = value;
				if (slider!=null)
				{
					slider.onValueChanged.AddListener(SliderValueChanged);
				}
				
			}
		}
		
		[SerializeField]
		Image sliderBackground;

		/// <summary>
		/// Gets or sets the slider background.
		/// </summary>
		/// <value>The slider background.</value>
		public Image SliderBackground {
			get {
				return sliderBackground;
			}
			set {
				sliderBackground = value;
				UpdateMaterial();
			}
		}
		
		[SerializeField]
		Shader sliderShader;

		/// <summary>
		/// Gets or sets the shader to display gradient for slider background.
		/// </summary>
		/// <value>The slider shader.</value>
		public Shader SliderShader {
			get {
				return sliderShader;
			}
			set {
				sliderShader = value;
				UpdateMaterial();
			}
		}

		ColorPickerInputMode inputMode;

		/// <summary>
		/// Gets or sets the input mode.
		/// </summary>
		/// <value>The input mode.</value>
		public ColorPickerInputMode InputMode {
			get {
				return inputMode;
			}
			set {
				inputMode = value;
			}
		}
		
		ColorPickerPaletteMode paletteMode;

		/// <summary>
		/// Gets or sets the palette mode.
		/// </summary>
		/// <value>The palette mode.</value>
		public ColorPickerPaletteMode PaletteMode {
			get {
				return paletteMode;
			}
			set {
				paletteMode = value;

				bool is_active = paletteMode==ColorPickerPaletteMode.Hue
					|| paletteMode==ColorPickerPaletteMode.Saturation
					|| paletteMode==ColorPickerPaletteMode.Value;
				gameObject.SetActive(is_active);
				slider.maxValue = (paletteMode==ColorPickerPaletteMode.Hue) ? 359 : 255;

				if (is_active)
				{
					UpdateView();
				}
			}
		}

		/// <summary>
		/// OnChangeRGB event.
		/// </summary>
		public ColorRGBChangedEvent OnChangeRGB = new ColorRGBChangedEvent();

		/// <summary>
		/// OnChangeHSV event.
		/// </summary>
		public ColorHSVChangedEvent OnChangeHSV = new ColorHSVChangedEvent();

		/// <summary>
		/// OnChangeAlpha event.
		/// </summary>
		public ColorAlphaChangedEvent OnChangeAlpha = new ColorAlphaChangedEvent();

		bool isStarted;

		/// <summary>
		/// Start this instance.
		/// </summary>
		public void Start()
		{
			if (isStarted)
			{
				return ;
			}
			isStarted = true;
			
			Palette = palette;
			Slider = slider;
			SliderBackground = sliderBackground;
		}

		void OnEnable()
		{
			UpdateMaterial();
		}

		void SliderValueChanged(float value)
		{
			ValueChanged();
		}
		
		bool inUpdateMode;
		void ValueChanged()
		{
			if (inUpdateMode)
			{
				return ;
			}
			currentColorHSV = GetColor();

			OnChangeHSV.Invoke(currentColorHSV);
		}
		
		ColorHSV currentColorHSV;

		/// <summary>
		/// Sets the color.
		/// </summary>
		/// <param name="color">Color.</param>
		public void SetColor(Color32 color)
		{
			currentColorHSV = new ColorHSV(color);

			UpdateView();
		}

		/// <summary>
		/// Sets the color.
		/// </summary>
		/// <param name="color">Color.</param>
		public void SetColor(ColorHSV color)
		{
			currentColorHSV = color;

			UpdateView();
		}
		
		void OnDrag(PointerEventData eventData)
		{
			Vector2 size = paletteRect.rect.size;
			Vector2 cur_pos;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(paletteRect, eventData.position, eventData.pressEventCamera, out cur_pos);

			cur_pos.x = Mathf.Clamp(cur_pos.x, 0, size.x);
			cur_pos.y = Mathf.Clamp(cur_pos.y, -size.y, 0);
			paletteCursor.localPosition = cur_pos;
			
			ValueChanged();
		}
		
		ColorHSV GetColor()
		{
			var coords = GetCursorCoords();
			
			var s = Mathf.RoundToInt(slider.value);
			var x = coords.x;
			var y = coords.y;

			switch (paletteMode)
			{
				case ColorPickerPaletteMode.Hue:
					return new ColorHSV(s, Mathf.RoundToInt(y * 255f), Mathf.RoundToInt(x * 255f), currentColorHSV.A);
				case ColorPickerPaletteMode.Saturation:
					return new ColorHSV(Mathf.RoundToInt(x * 359f), s, Mathf.RoundToInt(y * 255f), currentColorHSV.A);
				case ColorPickerPaletteMode.Value:
					return new ColorHSV(Mathf.RoundToInt(x * 359f), Mathf.RoundToInt(y * 255f), s, currentColorHSV.A);
				default:
					return currentColorHSV;
			}
		}

		Vector2 GetCursorCoords()
		{
			var coords = paletteCursor.localPosition;
			var size = paletteRect.rect.size;
			
			var x = (coords.x / size.x);
			var y = coords.y / size.y + 1;

			return new Vector2(x, y);
		}

		void UpdateView()
		{
			#if UNITY_5_2
			UpdateMaterial();
			#else
			UpdateViewReal();
			#endif
		}

		void UpdateViewReal()
		{
			inUpdateMode = true;
			
			//set slider value
			if (slider!=null)
			{
				slider.value = GetSliderValue();
			}
			
			//set slider colors
			if (sliderBackground!=null)
			{
				var colors = GetSliderColors();
				sliderBackground.material.SetColor("_ColorBottom", colors[0]);
				sliderBackground.material.SetColor("_ColorTop", colors[1]);
			}
			
			//set palette drag position
			if ((paletteCursor!=null) && (palette!=null) && (paletteRect!=null))
			{
				var coords = GetPaletteCoords();
				var size = paletteRect.rect.size;

				paletteCursor.localPosition = new Vector3(coords.x * size.x, - (1 - coords.y) * size.y, 0);
			}
			
			//set palette colors
			if (palette!=null)
			{
				var colors = GetPaletteColors();

				palette.material.SetColor("_ColorLeft", colors[0]);
				palette.material.SetColor("_ColorRight", colors[1]);
				palette.material.SetColor("_ColorBottom", colors[2]);
				palette.material.SetColor("_ColorTop", colors[3]);
			}
			
			inUpdateMode = false;
		}
		
		int GetSliderValue()
		{
			switch (paletteMode)
			{
				case ColorPickerPaletteMode.Hue:
					return currentColorHSV.H;
				case ColorPickerPaletteMode.Saturation:
					return currentColorHSV.S;
				case ColorPickerPaletteMode.Value:
					return currentColorHSV.V;
				default:
					return 0;
			}
		}
		
		Color[] GetSliderColors()
		{
			switch (paletteMode)
			{
				case ColorPickerPaletteMode.Hue:
					return new Color[] {
						new Color(0f, 1f, 1f, 1f),
						new Color(1f, 1f, 1f, 1f),
					};
				case ColorPickerPaletteMode.Saturation:
					return new Color[] {
					new Color(currentColorHSV.H / 359f, 0f, Mathf.Max(ColorPicker.ValueLimit, currentColorHSV.V) / 255f, 1f),
					new Color(currentColorHSV.H / 359f, 1f, Mathf.Max(ColorPicker.ValueLimit, currentColorHSV.V) / 255f, 1f),
					};
				case ColorPickerPaletteMode.Value:
					return new Color[] {
						new Color(currentColorHSV.H / 359f, currentColorHSV.S / 255f, 0f, 1f),
						new Color(currentColorHSV.H / 359f, currentColorHSV.S / 255f, 1f, 1f),
					};
				default:
					return new Color[] {
						new Color(0f, 0f, 0f, 1f),
						new Color(1f, 1f, 1f, 1f)
					};
			}
		}
		
		Vector2 GetPaletteCoords()
		{
			switch (paletteMode)
			{
				case ColorPickerPaletteMode.Hue:
					return new Vector2(currentColorHSV.V / 255f, currentColorHSV.S / 255f);
				case ColorPickerPaletteMode.Saturation:
					return new Vector2(currentColorHSV.H / 359f, currentColorHSV.V / 255f);
				case ColorPickerPaletteMode.Value:
					return new Vector2(currentColorHSV.H / 359f, currentColorHSV.S / 255f);
				default:
					return new Vector2(0, 0);
			}
		}
		
		Color[] GetPaletteColors()
		{
			switch (paletteMode)
			{
				case ColorPickerPaletteMode.Hue:
					return new Color[] {
						new Color(currentColorHSV.H / 359f / 2f, 0f, 0f, 1f),
						new Color(currentColorHSV.H / 359f / 2f, 1f, 0f, 1f),
						new Color(currentColorHSV.H / 359f / 2f, 0f, 0f, 1f),
						new Color(currentColorHSV.H / 359f / 2f, 0f, 1f, 1f),
					};
				case ColorPickerPaletteMode.Saturation:
					return new Color[] {
						new Color(0f, currentColorHSV.S / 255f / 2f, 0f, 1f),
						new Color(1f, currentColorHSV.S / 255f / 2f, 0f, 1f),
						new Color(0f, currentColorHSV.S / 255f / 2f, 0f, 1f),
						new Color(0f, currentColorHSV.S / 255f / 2f, 1f, 1f),
					};
				case ColorPickerPaletteMode.Value:
					return new Color[] {
						new Color(0f, 0f, currentColorHSV.V / 255f / 2f, 1f),
						new Color(1f, 0f, currentColorHSV.V / 255f / 2f, 1f),
						new Color(0f, 0f, currentColorHSV.V / 255f / 2f, 1f),
						new Color(0f, 1f, currentColorHSV.V / 255f / 2f, 1f),
					};
				default:
					return new Color[] {
						new Color(0f, 0f, 1f, 1f),
						new Color(1f, 1f, 1f, 1f),
						new Color(0f, 0, 1f, 1f),
						new Color(1f, 1f, 1f, 1f),
					};
			}
		}
		
		void UpdateMaterial()
		{
			if ((paletteShader!=null) && (palette!=null))
			{
				palette.material = new Material(paletteShader);
			}
			
			if ((sliderShader!=null) && (sliderBackground!=null))
			{
				sliderBackground.material = new Material(sliderShader);
			}
			
			UpdateViewReal();
		}

		void OnDestroy()
		{
			dragListener = null;
			slider = null;
		}
	}
}