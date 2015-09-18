
using UnityEngine;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

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

	#region Horizontal Rule
	
	public override void Create_HR(PUGameObject container) {
		currentY -= paragraphSpacing;

		PUColor color = new PUColor ();
		color.color = new Color(0.8f, 0.8f, 0.8f, 1.0f);
		color.SetFrame (padding.left + 2, currentY, 0, 1, 0, 0, "top,left");
		color.LoadIntoPUGameObject (container);
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
		
		
		RectTransform lineText = container.rectTransform.GetChild (container.rectTransform.childCount - 1) as RectTransform;
		
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


		RectTransform lineText = container.rectTransform.GetChild (container.rectTransform.childCount - 1) as RectTransform;

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

	public void AddTextWithOptions(PUGameObject container, string content, Color color, float fontScale, string style, TMPro.TextAlignmentOptions alignment) {

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
		
		text.rectTransform.sizeDelta = text.CalculateTextSize (content, maxWidth);
		
		currentY -= text.rectTransform.sizeDelta.y + padding.bottom;
	}

	public Color textColor() {
		Color color = Color.black;
		
		if (blockquotesTop.Count > 0) {
			color = new Color(0.8f, 0.8f, 0.8f, 1.0f);
		}

		return color;
	}

	#endregion
}
