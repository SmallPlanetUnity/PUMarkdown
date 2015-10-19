/*
This is free software distributed under the terms of the MIT license, reproduced below. It may be used for any purpose, including commercial purposes, at absolutely no cost. No paperwork, no royalties, no GNU-like "copyleft" restrictions. Just download and enjoy.

Copyright (c) 2014 Chimera Software, LLC

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/


using UnityEngine;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using MarkdownDeep;
using System.Text;

public class MarkdownStyleGeneric : MarkdownStyle {

	
	protected float currentY = 0;
	protected Padding padding = new Padding (0, 0, 0, 0);

	protected Stack<float> blockquotesTop = new Stack<float>();
	protected Stack<int> listCounts = new Stack<int>();
	protected List<string> urlLinks = new List<string>();

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

	#region Definitions
	
	public override void Create_DefinitionTerm(PUGameObject container, string content) {
		AddTextWithOptions (container, content, textColor(), 1.0f, "BoldItalic", TMPro.TextAlignmentOptions.Left);
	}
	
	public override void Create_DefinitionData(PUGameObject container, string content) {
		currentY += DefaultFontSize() * 0.8f;

		padding.left += DefaultFontSize() * 1.0f;
		AddTextWithOptions (container, content, textColor(), 0.8f, "Normal", TMPro.TextAlignmentOptions.Left);
		padding.left -= DefaultFontSize() * 1.0f;
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

		currentY -= paragraphSpacing();

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
		currentY -= paragraphSpacing();

		PUColor color = new PUColor ();
		color.color = new Color(0.8f, 0.8f, 0.8f, 1.0f);
		color.SetFrame (padding.left + 2, currentY, 0, 1, 0, 0, "top,left");
		color.LoadIntoPUGameObject (container);
	}
	
	#endregion

	#region CodeBlock
	
	public override void Create_CodeBlock(PUGameObject container, string content) {

		float margin = 10;

		padding.left += DefaultFontSize() * 2.0f;
		padding.right += DefaultFontSize() * 2.0f;

		PUTMPro text = AddTextWithOptions (container, content, textColor(), 0.8f, "Normal", TMPro.TextAlignmentOptions.Left);

		PutTextInBox (container, text, margin, new Color32 (204, 204, 204, 255), new Color32 (248, 248, 248, 255));

		padding.left -= DefaultFontSize() * 2.0f;
		padding.right -= DefaultFontSize() * 2.0f;

		currentY -= margin;
	}
	
	#endregion

	#region Table
	
	public override void Create_Table(PUGameObject container, TableSpec spec) {

		float margin = DefaultFontSize();

		currentY -= paragraphSpacing();

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

		tableGroup.layout.cellSize = new Vector2 (maxCellWidth, maxCellHeight);
		tableGroup.rectTransform.sizeDelta = new Vector2 (maxCellWidth * numberOfCols, maxCellHeight * numberOfRows);
		currentY = savedY - tableGroup.rectTransform.sizeDelta.y;
	}
	
	#endregion

	#region BLOCKQUOTE

	public override void Begin_Blockquote(PUGameObject container) {
		padding.left += 16;
		blockquotesTop.Push (currentY - DefaultFontSize());
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
			currentY += DefaultFontSize() * 0.5f;
		}
		
		float oldY = currentY;
		
		padding.left += DefaultFontSize() * 2.0f;
		Create_P(container, content);
		padding.left -= DefaultFontSize() * 2.0f;

		PUTMPro text = new PUTMPro ();
		text.SetFrame (padding.left, currentY - padding.top, DefaultFontSize() * 1.5f, (oldY - currentY) - DefaultFontSize(), 0, 0, "top,left");
		text.font = DefaultFont();
		text.value = "•";
		text.fontColor = textColor();
		text.fontStyle = "Bold";
		text.fontSize = (int)(DefaultFontSize());
		text.sizeToFit = true;
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
			currentY += DefaultFontSize() * 0.5f;
		}

		float oldY = currentY;
		
		padding.left += DefaultFontSize() * 2.0f;
		Create_P(container, content);
		padding.left -= DefaultFontSize() * 2.0f;

		PUTMPro text = new PUTMPro ();
		text.SetFrame (padding.left, currentY - padding.top, DefaultFontSize() * 1.5f, (oldY - currentY) - DefaultFontSize(), 0, 0, "top,left");
		text.font = DefaultFont();
		text.value = string.Format ("{0}.", listCounts.Peek () + 1);
		text.fontColor = textColor();
		text.fontStyle = "Bold";
		text.fontSize = (int)(DefaultFontSize());
		text.sizeToFit = true;
		text.alignment = TMPro.TextAlignmentOptions.TopRight;
		text.enableWordWrapping = false;
		text.LoadIntoPUGameObject (container);

		text.textGUI.OverflowMode = TMPro.TextOverflowModes.Overflow;
		
		listCounts.Push(listCounts.Pop() + 1);
	}
	
	#endregion


	#region Links

	public override void Tag_Link(PUGameObject container, StringBuilder content, string url, string text) {
		urlLinks.Add (url);
		content.AppendFormat ("<#5a92c9ff><u>\x0b{0}\x0c</u></color>", text);
	}

	#endregion


	#region Utility

	public PUTMPro AddTextWithOptions(PUGameObject container, string content, Color color, float fontScale, string style, TMPro.TextAlignmentOptions alignment) {

		currentY -= paragraphSpacing();

		float maxWidth = container.size.Value.x - (padding.left + padding.right);
		
		PUTMPro text = new PUTMPro ();
		text.SetFrame (padding.left, currentY - padding.top, maxWidth, 0, 0, 1, "top,left");
		text.font = DefaultFont();
		text.fontColor = color;
		text.fontStyle = style;
		text.fontSize = (int)(DefaultFontSize()*fontScale);
		text.sizeToFit = true;
		text.alignment = alignment;
		text.value = content;

		if (urlLinks.Count > 0) {
			string[] linkURLs = urlLinks.ToArray();
			text.OnLinkClickAction = (linkText,linkIdx) => {
				OpenLink(linkURLs[linkIdx]);
			};
			urlLinks.Clear();
		}

		text.LoadIntoPUGameObject (container);



		Vector2 size = text.CalculateTextSize (content, maxWidth);
		text.rectTransform.sizeDelta = size;
		
		currentY -= text.rectTransform.sizeDelta.y + padding.bottom;

		return text;
	}

	public virtual Color textColor() {
		Color color = Color.black;
		
		if (blockquotesTop.Count > 0) {
			color = Color.grey;
		}

		return color;
	}

	public virtual float paragraphSpacing() {
		return DefaultFontSize();
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

	public virtual void OpenLink(string link) {
		Application.OpenURL(link);
	}

	#endregion
}
