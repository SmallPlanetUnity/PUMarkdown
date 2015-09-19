
using UnityEngine;
using System.Xml;
using System.Collections;
using System.Text;
using UnityEngine.UI;
using System;
using MarkdownDeep;

public struct Padding {
	public float left, right, bottom, top;
	public Padding(float top, float left, float bottom, float right){
		this.left = left;
		this.top = top;
		this.right = right;
		this.bottom = bottom;
	}
}

public class MarkdownRemoteImageLoader : MonoBehaviour {
	public string path;
	public Action onComplete;
	private WWW www;
	
	void Start() {
		if (path.StartsWith ("http://") || path.StartsWith ("https://")) {
			www = new WWW (path);
			return;
		}
		www = new WWW ("file://" + path);
	}
	
	void Update() {
		if (www.isDone) {
			RawImage rawImage = GetComponent<RawImage>();
			if(rawImage != null){
				rawImage.texture = www.texture;
				rawImage.texture.wrapMode = TextureWrapMode.Clamp;
				if(onComplete != null){
					onComplete();
				}
			}
			
			www.Dispose();
			GameObject.Destroy (this);
		}
	}
}

public class MarkdownStyle {

	public virtual void Begin(PUGameObject container) {
		
	}

	public virtual void End(PUGameObject container) {
		
	}

	#region Body

	public virtual void Create_P(PUGameObject container, string content) {

	}

	#endregion

	#region Headers

	public virtual void Create_H1(PUGameObject container, string content) {
		
	}

	public virtual void Create_H2(PUGameObject container, string content) {
		
	}

	public virtual void Create_H3(PUGameObject container, string content) {
		
	}

	public virtual void Create_H4(PUGameObject container, string content) {
		
	}

	public virtual void Create_H5(PUGameObject container, string content) {
		
	}

	public virtual void Create_H6(PUGameObject container, string content) {
		
	}

	#endregion

	#region Image
	
	public virtual void Create_IMG(PUGameObject container, string url, string title) {
		
	}
	
	#endregion

	#region Horizontal Rule

	public virtual void Create_HR(PUGameObject container) {
		
	}

	#endregion

	#region CodeBlock
	
	public virtual void Create_CodeBlock(PUGameObject container, string content) {
		
	}
	
	#endregion

	#region Table
	
	public virtual void Create_Table(PUGameObject container, TableSpec table) {
		
	}
	
	#endregion

	#region Blockquotes

	public virtual void Begin_Blockquote(PUGameObject container) {

	}

	public virtual void End_Blockquote(PUGameObject container) {
		
	}

	#endregion

	#region Unordered List

	public virtual void Begin_UnorderedList(PUGameObject container) {
		
	}
	
	public virtual void End_UnorderedList(PUGameObject container) {
		
	}

	public virtual void Create_UL_LI(PUGameObject container, string content) {
		
	}

	#endregion

	#region Ordered List

	public virtual void Begin_OrderedList(PUGameObject container) {
		
	}
	
	public virtual void End_OrderedList(PUGameObject container) {
		
	}
	
	public virtual void Create_OL_LI(PUGameObject container, string content) {
		
	}

	#endregion



	#region Inline tags

	public virtual void Tag_BreakingReturn(PUGameObject container, StringBuilder content) {
		content.Append ("\n");
	}

	public virtual void Tag_Emphasis(PUGameObject container, StringBuilder content, bool isOpen) {
		if (isOpen) {
			content.Append ("<i>");
		} else {
			content.Append ("</i>");
		}
	}

	public virtual void Tag_Strong(PUGameObject container, StringBuilder content, bool isOpen) {
		if (isOpen) {
			content.Append ("<b>");
		} else {
			content.Append ("</b>");
		}
	}

	public virtual void Tag_Code(PUGameObject container, StringBuilder content, bool isOpen) {
		if (isOpen) {
			content.Append ("<u>");
		} else {
			content.Append ("</u>");
		}
	}

	#endregion

}
