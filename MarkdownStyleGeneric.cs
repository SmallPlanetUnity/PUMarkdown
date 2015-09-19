
using UnityEngine;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using MarkdownDeep;

public class MarkdownStyleGeneric : MarkdownStyle {


	public float fontSize = 18;
	public string font = "Fonts/ArialRegular";

	public float paragraphSpacing {
		get {
			return fontSize;
		}
	}

	private float currentY = 0;
	private Padding padding = new Padding (0, 0, 0, 0);

	private Stack<float> blockquotesTop = new Stack<float>();
	private Stack<int> listCounts = new Stack<int>();

	public override void Begin(PUGameObject container) {
		currentY = 0;
		blockquotesTop.Clear ();
	}
	
	public override void End(PUGameObject container) {
		
	}

	#region BODY
	
	public override void Create_P(PUGameObject container, string content) {
		AddTextWithOptions (container, content, textColor(), 1.0f, "Normal", TMPro.TextAlignmentOptions.Left);
	}

	#endregion

	#region HEADERS

	public override void Create_H1(PUGameObject container, string content) {
		AddTextWithOptions (container, content, Color.black, 2.0f, "Bold", TMPro.TextAlignmentOptions.Left);
	}

	public override void Create_H2(PUGameObject container, string content) {
		AddTextWithOptions (container, content, Color.black, 1.71f, "Bold", TMPro.TextAlignmentOptions.Left);
	}

	public override void Create_H3(PUGameObject container, string content) {
		AddTextWithOptions (container, content, Color.black, 1.28f, "Bold", TMPro.TextAlignmentOptions.Left);
	}

	public override void Create_H4(PUGameObject container, string content) {
		AddTextWithOptions (container, content, Color.black, 1.14f, "Bold", TMPro.TextAlignmentOptions.Left);
	}

	public override void Create_H5(PUGameObject container, string content) {
		AddTextWithOptions (container, content, Color.black, 1.0f, "Bold", TMPro.TextAlignmentOptions.Left);
	}

	public override void Create_H6(PUGameObject container, string content) {
		AddTextWithOptions (container, content, Color.grey, 1.0f, "Bold", TMPro.TextAlignmentOptions.Left);
	}

	#endregion

	#region Image
	
	public override void Create_IMG(PUGameObject container, string url, string title) {

		Vector2 size = new Vector2 (100, 100);
		if (title.Contains (",")) {
			size = Vector2.zero.PUParse(title);
		}

		currentY -= paragraphSpacing;

		PURawImage img = new PURawImage ();
		img.SetFrame (padding.left, currentY - padding.top, size.x, size.y, 0, 1, "top,left");
		img.LoadIntoPUGameObject (container);

		MarkdownRemoteImageLoader loader = img.gameObject.AddComponent<MarkdownRemoteImageLoader> ();
		loader.path = url;
		loader.onComplete = () => {
			// now that we have this image, adjust the size?
			int w = img.image.texture.width;
			int h = img.image.texture.height;
			img.rectTransform.sizeDelta = new Vector2(w, h);
		};

		currentY -= size.y;
	}
	
	#endregion

	#region Horizontal Rule
	
	public override void Create_HR(PUGameObject container) {
		currentY -= paragraphSpacing;

		PUColor color = new PUColor ();
		color.color = new Color(0.8f, 0.8f, 0.8f, 1.0f);
		color.SetFrame (padding.left + 2, currentY, 0, 1, 0, 0, "top,left");
		color.LoadIntoPUGameObject (container);
	}
	
	#endregion

	#region CodeBlock
	
	public override void Create_CodeBlock(PUGameObject container, string content) {

		float margin = 10;

		padding.left += fontSize * 2.0f;
		padding.right += fontSize * 2.0f;

		PUTMPro text = AddTextWithOptions (container, content, textColor(), 0.8f, "Normal", TMPro.TextAlignmentOptions.Left);

		PutTextInBox (container, text, margin, new Color32 (204, 204, 204, 255), new Color32 (248, 248, 248, 255));

		padding.left -= fontSize * 2.0f;
		padding.right -= fontSize * 2.0f;

		currentY -= margin;
	}
	
	#endregion

	#region Table
	
	public override void Create_Table(PUGameObject container, TableSpec spec) {

		float margin = fontSize;

		currentY -= paragraphSpacing;

		float savedY = currentY;

		PUGridLayoutGroup tableGroup = new PUGridLayoutGroup();
		tableGroup.SetFrame (padding.left+2, currentY, container.size.Value.x, 100, 0, 1, "top,left");
		tableGroup.LoadIntoPUGameObject (container);

		// Fill out the group, then figure out the height / widths needed based upon the content
		float maxCellWidth = 0;
		float maxCellHeight = 0;
		int numberOfCols = 0;
		int numberOfRows = 0;

		for(int i = 0; i < spec.Headers.Count; i++) {
			string header = spec.Headers[i];
			ColumnAlignment alignment = spec.Columns[i];

			TMPro.TextAlignmentOptions tmAlignment = TMPro.TextAlignmentOptions.Left;
			if(alignment == ColumnAlignment.Right){
				tmAlignment = TMPro.TextAlignmentOptions.Right;
			}
			if(alignment == ColumnAlignment.Center){
				tmAlignment = TMPro.TextAlignmentOptions.Center;
			}

			PUTMPro text = AddTextWithOptions (tableGroup, header, textColor(), 1.0f, "Bold", tmAlignment);
			Vector2 size = text.rectTransform.sizeDelta + new Vector2(margin*2.0f,margin);

			text.rectTransform.pivot = Vector2.zero;
			text.rectTransform.anchorMax = Vector2.one;
			text.rectTransform.anchorMin = Vector2.zero;

			PutTextInBox (tableGroup, text, 2, new Color32 (204, 204, 204, 255), new Color32 (255, 255, 255, 255));
			text.SetStretchStretch (margin*0.5f, margin, margin*0.5f, margin);

			if(size.x > maxCellWidth){
				maxCellWidth = size.x;
			}
			if(size.y > maxCellHeight){
				maxCellHeight = size.y;
			}
		}

		numberOfCols = spec.Rows[0].Count;
		numberOfRows = spec.Rows.Count;

		if (spec.Headers.Count > 0) {
			numberOfRows++;
		}
		
		for(int i = 0; i < spec.Rows.Count; i++) {
			List<string> rows = spec.Rows[i];

			for(int j = 0; j < rows.Count; j++) {
				string row = rows[j];

				ColumnAlignment alignment = spec.Columns[j];
				
				TMPro.TextAlignmentOptions tmAlignment = TMPro.TextAlignmentOptions.Left;
				if(alignment == ColumnAlignment.Right){
					tmAlignment = TMPro.TextAlignmentOptions.Right;
				}
				if(alignment == ColumnAlignment.Center){
					tmAlignment = TMPro.TextAlignmentOptions.Center;
				}


				PUTMPro text = AddTextWithOptions (tableGroup, row, textColor(), 1.0f, "Normal", tmAlignment);
				Vector2 size = text.rectTransform.sizeDelta + new Vector2(margin*2.0f,margin);

				text.rectTransform.pivot = Vector2.zero;
				text.rectTransform.anchorMax = Vector2.one;
				text.rectTransform.anchorMin = Vector2.zero;

				if(i % 2 != 0){
					PutTextInBox (tableGroup, text, 2, new Color32 (204, 204, 204, 255), new Color32 (248, 248, 248, 255));
				}else{
					PutTextInBox (tableGroup, text, 2, new Color32 (204, 204, 204, 255), new Color32 (255, 255, 255, 255));
				}
				text.SetStretchStretch (margin*0.5f, margin, margin*0.5f, margin);

				if(size.x > maxCellWidth){
					maxCellWidth = size.x;
				}
				if(size.y > maxCellHeight){
					maxCellHeight = size.y;
				}
			}
		}

		Debug.Log ("numberOfRows: " + numberOfRows);
		Debug.Log ("numberOfCols: " + numberOfCols);

		tableGroup.layout.cellSize = new Vector2 (maxCellWidth, maxCellHeight);
		tableGroup.rectTransform.sizeDelta = new Vector2 (maxCellWidth * numberOfCols, maxCellHeight * numberOfRows);
		currentY = savedY - tableGroup.rectTransform.sizeDelta.y;

	}
	
	#endregion

	#region BLOCKQUOTE

	public override void Begin_Blockquote(PUGameObject container) {
		padding.left += 16;
		blockquotesTop.Push (currentY - fontSize);
	}
	
	public override void End_Blockquote(PUGameObject container) {
		padding.left -= 16;

		float topY = blockquotesTop.Peek ();
		blockquotesTop.Pop ();

		PUColor color = new PUColor ();
		color.color = new Color(0.8f, 0.8f, 0.8f, 1.0f);
		color.SetFrame (padding.left+2, currentY, 4, Mathf.Abs (topY - currentY), 0, 0, "top,left");
		color.LoadIntoPUGameObject (container);
	}

	#endregion

	#region Unordered List

	public override void Begin_UnorderedList(PUGameObject container) {
		listCounts.Push (0);
	}
	
	public override void End_UnorderedList(PUGameObject container) {
		listCounts.Pop ();
	}
	
	public override void Create_UL_LI(PUGameObject container, string content) {

		if (listCounts.Peek() != 0) {
			currentY += fontSize * 0.5f;
		}
		
		float oldY = currentY;
		
		padding.left += fontSize * 2.0f;
		Create_P(container, content);
		padding.left -= fontSize * 2.0f;

		PUTMPro text = new PUTMPro ();
		text.SetFrame (padding.left, currentY - padding.top, fontSize * 1.5f, (oldY - currentY) - fontSize, 0, 0, "top,left");
		text.font = font;
		text.value = "•";
		text.fontColor = textColor();
		text.fontStyle = "Bold";
		text.fontSize = (int)(fontSize);
		text.alignment = TMPro.TextAlignmentOptions.TopRight;
		text.enableWordWrapping = false;
		text.LoadIntoPUGameObject (container);
		
		text.textGUI.OverflowMode = TMPro.TextOverflowModes.Overflow;
		
		listCounts.Push(listCounts.Pop() + 1);
	}

	#endregion


	#region Ordered List
	
	public override void Begin_OrderedList(PUGameObject container) {
		listCounts.Push (0);
	}
	
	public override void End_OrderedList(PUGameObject container) {
		listCounts.Pop ();
	}
	
	public override void Create_OL_LI(PUGameObject container, string content) {
		
		if (listCounts.Peek() != 0) {
			currentY += fontSize * 0.5f;
		}

		float oldY = currentY;
		
		padding.left += fontSize * 2.0f;
		Create_P(container, content);
		padding.left -= fontSize * 2.0f;

		PUTMPro text = new PUTMPro ();
		text.SetFrame (padding.left, currentY - padding.top, fontSize * 1.5f, (oldY - currentY) - fontSize, 0, 0, "top,left");
		text.font = font;
		text.value = string.Format ("{0}.", listCounts.Peek () + 1);
		text.fontColor = textColor();
		text.fontStyle = "Bold";
		text.fontSize = (int)(fontSize);
		text.alignment = TMPro.TextAlignmentOptions.TopRight;
		text.enableWordWrapping = false;
		text.LoadIntoPUGameObject (container);

		text.textGUI.OverflowMode = TMPro.TextOverflowModes.Overflow;
		
		listCounts.Push(listCounts.Pop() + 1);
	}
	
	#endregion


	#region Utility

	public PUTMPro AddTextWithOptions(PUGameObject container, string content, Color color, float fontScale, string style, TMPro.TextAlignmentOptions alignment) {

		currentY -= paragraphSpacing;

		float maxWidth = container.size.Value.x - (padding.left + padding.right);
		
		PUTMPro text = new PUTMPro ();
		text.SetFrame (padding.left, currentY - padding.top, maxWidth, 0, 0, 1, "top,left");
		text.font = font;
		text.fontColor = color;
		text.fontStyle = style;
		text.fontSize = (int)(fontSize*fontScale);
		text.alignment = alignment;
		text.value = content;
		text.LoadIntoPUGameObject (container);

		Vector2 size = text.CalculateTextSize (content, maxWidth);
		text.rectTransform.sizeDelta = size;
		
		currentY -= text.rectTransform.sizeDelta.y + padding.bottom;

		return text;
	}

	public Color textColor() {
		Color color = Color.black;
		
		if (blockquotesTop.Count > 0) {
			color = Color.grey;
		}

		return color;
	}

	public void PutTextInBox(PUGameObject container, PUTMPro text, float margin, Color outlineColor, Color backgroundColor) {

		PUColor outlineColorGO = new PUColor ();
		outlineColorGO.color = outlineColor;
		outlineColorGO.SetFrame (text.rectTransform.anchoredPosition.x,text.rectTransform.anchoredPosition.y,text.rectTransform.sizeDelta.x + margin * 2.0f,text.rectTransform.sizeDelta.y + margin * 2.0f,0,1,"top,left");
		outlineColorGO.LoadIntoPUGameObject (container);

		PUColor backgroundColorGO = new PUColor ();
		backgroundColorGO.color = backgroundColor;
		backgroundColorGO.SetFrame (0, 0, 0, 0, 0, 0, "stretch,stretch");
		backgroundColorGO.LoadIntoPUGameObject (outlineColorGO);
		backgroundColorGO.SetStretchStretch (1, 1, 1, 1);

		text.rectTransform.SetParent (outlineColorGO.rectTransform, false);
		text.rectTransform.pivot = Vector2.zero;
		text.rectTransform.anchorMax = Vector2.one;
		text.rectTransform.anchorMin = Vector2.zero;
		text.SetStretchStretch (margin, margin, margin, margin);

	}

	#endregion
}
