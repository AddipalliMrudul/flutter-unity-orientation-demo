
using XcelerateGames.PsdImporter.Reconstructor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XcelerateGames.EditorTools.PsdToUI.Layer;
using XcelerateGames.EditorTools.PsdToUI.Text;
using XcelerateGames.Locale;
using XcelerateGames.UI;

namespace XcelerateGames.EditorTools.PsdToUI
{
    public class XgUIObjectsWorkshop
    {
		private enum GroupLayerType
		{
			None,
			Button,
			InputField,
			Toggle
		}

		public RectTransform CreateObject(XgImportLayerData layerData, ReconstructData data)
        {
			if (layerData.layerType == LayerType.Text)
				return CreateTextObject(layerData, data);
			else if(layerData.layerType == LayerType.Image)
				return CreateImageObject(layerData, data);

			return CreateObject(layerData.name);
		}

		private RectTransform CreateObject(string name)
		{
			GameObject rootObject = new GameObject(name, typeof(RectTransform));
			RectTransform rectT = rootObject.GetComponent<RectTransform>();

			return rectT;
		}

		private RectTransform CreateImageObject(XgImportLayerData layerData, ReconstructData data)
		{
			RectTransform rectT = CreateObject(layerData.name);

			// Find the sprite for this layer in the data sprite index
			Sprite layerSprite;
			if (data.spriteIndex.TryGetValue(layerData.indexId, out layerSprite))
			{
				var layerImg = rectT.gameObject.AddComponent<Image>();
				layerImg.sprite = layerSprite;
			}
			return rectT;
		}

		private RectTransform CreateTextObject(XgImportLayerData layerData, ReconstructData data)
		{
			RectTransform rectT = CreateObject(layerData.name);
			XgTextLayerData textLayerData = layerData as XgTextLayerData;
			XgPsdTextInfo textInfo = textLayerData.textInfo;
			//Update text info data into text component
			TextMeshProUGUI layerTxt = rectT.gameObject.AddComponent<TextMeshProUGUI>();
			TMP_FontAsset fontAsset = data.fonts[textInfo.fontName];
			layerTxt.font = fontAsset;
			layerTxt.text = textLayerData.textInfo.text;
			layerTxt.fontSize = textLayerData.textInfo.fontSize;
			layerTxt.color = new Color(textLayerData.textInfo.color[0], textLayerData.textInfo.color[1], textLayerData.textInfo.color[2], textLayerData.textInfo.color[3]);
			if (textLayerData.textInfo.boldEnabled)
				layerTxt.fontStyle = FontStyles.Bold;

			if (textLayerData.textInfo.italicsEnabled)
				layerTxt.fontStyle = FontStyles.Italic;

			//Setting other values of TMPro object
			layerTxt.alignment = TextAlignmentOptions.Center;
			layerTxt.enableWordWrapping = false;

			//Adding Localise component
			rectT.gameObject.AddComponent<UILocalizeTMP>();
			return rectT;
		}

		private void OnButtonLayer(RectTransform rectT)
		{
			Button buttonComp = rectT.gameObject.AddComponent<Button>();
			UiItem uiItem = rectT.gameObject.AddComponent<UiItem>();
			buttonComp.targetGraphic = uiItem._Image;
		}

		private void OnInputFieldLayer(RectTransform rectT)
		{
			TMP_InputField inputField = rectT.gameObject.AddComponent<TMP_InputField>();
			RectTransform textArea = new GameObject("TextArea", typeof(RectMask2D)).GetComponent<RectTransform>();
			textArea.SetParent(rectT);
			textArea.anchorMin = Vector2.zero;
			textArea.anchorMax = Vector2.one;
			textArea.offsetMin = Vector2.zero;
			textArea.offsetMax = Vector2.zero;
			inputField.textViewport = textArea;

			TextMeshProUGUI placeHolder = rectT.GetComponentInChildren<TextMeshProUGUI>();
			placeHolder.rectTransform.SetParent(textArea);
			placeHolder.rectTransform.anchorMin = Vector2.zero;
			placeHolder.rectTransform.anchorMax = Vector2.one;
			placeHolder.rectTransform.offsetMin = Vector2.zero;
			placeHolder.rectTransform.offsetMax = Vector2.zero;
			placeHolder.rectTransform.SetPivot(PivotPresets.MiddleCenter);
			inputField.placeholder = placeHolder;
			placeHolder.name = "PlaceHolder";

			GameObject textCompGO = GameObject.Instantiate(placeHolder.gameObject, textArea);
			TextMeshProUGUI textComp = textCompGO.GetComponent<TextMeshProUGUI>();
			textComp.text = "";
			textComp.name = "Text";
			inputField.textComponent = textComp;

			rectT.gameObject.AddComponent<UiInputItem>();
		}

		private void OnToggleLayer(RectTransform rectT)
        {
			Toggle toggleComp = rectT.gameObject.AddComponent<Toggle>();
			Image[] images = rectT.GetComponentsInChildren<Image>();
			foreach (Image img in images)
			{
				if (img.name.Contains("Selected"))
				{
					toggleComp.graphic = img;
					break;
				}
			}
			rectT.gameObject.AddComponent<UiToggleItem>();
        }

		public void SanitizeWidget(RectTransform widgetRectT)
        {
			CorrectWidgetBounds(widgetRectT);
			GroupLayerType layerType = GetGroupLayerType(widgetRectT.name);
			if (layerType == GroupLayerType.Button)
				OnButtonLayer(widgetRectT);
			else if (layerType == GroupLayerType.InputField)
				OnInputFieldLayer(widgetRectT);
			else if (layerType == GroupLayerType.Toggle)
				OnToggleLayer(widgetRectT);
		}

		private void CorrectWidgetBounds(RectTransform rootT)
		{
			Transform tempParent = new GameObject("Temp").transform;
			var min = Vector2.positiveInfinity;
			var max = Vector2.negativeInfinity;

			while (rootT.childCount > 0)
			{
				RectTransform rectT = rootT.GetChild(0) as RectTransform;
				//// update min and max
				min = Vector2.Min(min, rectT.offsetMin);
				max = Vector2.Max(max, rectT.offsetMax);
				rectT.SetParent(tempParent);
			}

			// create the bounds
			var bounds = new Bounds();
			bounds.SetMinMax(min, max);

			//// caching anchor values
			//Vector2 aMin = rootT.anchorMin;
			//Vector2 aMax = rootT.anchorMax;

			rootT.anchorMin = new Vector2(0.5f, 0.5f);
			rootT.anchorMax = new Vector2(0.5f, 0.5f);
			rootT.anchoredPosition = bounds.center;
			rootT.sizeDelta = bounds.size;

			//// setting back anchor values
			//rootT.anchorMin = aMin;
			//rootT.anchorMax = aMax;

			while (tempParent.childCount > 0)
				tempParent.GetChild(0).SetParent(rootT);

			GameObject.DestroyImmediate(tempParent.gameObject);
		}

		private GroupLayerType GetGroupLayerType(string layerName)
		{
			if (layerName.StartsWith("Btn"))
				return GroupLayerType.Button;
			else if (layerName.StartsWith("Ipf"))
				return GroupLayerType.InputField;
			else if (layerName.StartsWith("Tgl"))
				return GroupLayerType.Toggle;

			return GroupLayerType.None;
		}
	}
}
