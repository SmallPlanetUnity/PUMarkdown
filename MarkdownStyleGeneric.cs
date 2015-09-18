
using UnityEngine;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

public class MarkdownStyleGeneric : MarkdownStyle {

	public float fontSize = 18;
	public string font = null;

	private float currentY = 0;
	private Padding padding = new Padding (0, 0, 0, 0);

	private List<float> blockquotesTop = new List<float>();

	public override void Begin(PUGameObject container) {
		currentY = 0;
		blockquotesTop.Clear ();
	}
	
	public override void End(PUGameObject container) {
		
	}
	
	public override void Create_P(PUGameObject container, string content) {
		AddTextWithOptions (container, content, Color.black, 1.0f, "Normal", TMPro.TextAlignmentOptions.Left);
	}

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

	public override void Begin_Blockquote(PUGameObject container) {
		padding.left += 20;
		blockquotesTop.Add (currentY - fontSize);
	}
	
	public override void End_Blockquote(PUGameObject container) {
		padding.left -= 20;

		int idx = blockquotesTop.Count - 1;
		float topY = blockquotesTop [idx];
		blockquotesTop.RemoveAt (idx);

		PUColor color = new PUColor ();
		color.color = Color.gray;
		color.SetFrame (padding.left, currentY, 6, Mathf.Abs (topY - currentY), 0, 0, "top,left");
		color.LoadIntoPUGameObject (container);
	}


	public void AddTextWithOptions(PUGameObject container, string content, Color color, float fontScale, string style, TMPro.TextAlignmentOptions alignment) {
		currentY -= fontSize;

		float maxWidth = container.size.Value.x - (padding.left + padding.right);
		
		PUTMPro text = new PUTMPro ();
		text.SetFrame (padding.left, currentY - padding.top, maxWidth, 0, 0, 1, "top,left");
		text.fontColor = color;
		text.fontStyle = style;
		text.fontSize = (int)(fontSize*fontScale);
		text.alignment = alignment;
		text.value = content;
		text.LoadIntoPUGameObject (container);
		
		text.rectTransform.sizeDelta = text.CalculateTextSize (content, maxWidth);
		
		currentY -= text.rectTransform.sizeDelta.y + padding.bottom;
	}
}
