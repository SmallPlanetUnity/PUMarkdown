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

    void Start()
    {
        // Check to see if this is an app resource, if so load it directly
        RawImage rawImage = GetComponent<RawImage>();
        if (rawImage != null)
        {
            Texture2D localImage = PlanetUnityResourceCache.GetTexture(path);
            if(localImage != null){
                rawImage.texture = localImage;
                if (onComplete != null)
                {
                    onComplete();
                }
                GameObject.Destroy(this);
                return;
            }
        }

        // Check to see if this is an app resource, if so load it directly
        Image image = GetComponent<Image>();
        if (image != null)
        {
            Sprite localSprite = PlanetUnityResourceCache.GetSprite(path, true);
            if (localSprite != null)
            {
                image.sprite = localSprite;
                if (onComplete != null)
                {
                    onComplete();
                }
                GameObject.Destroy(this);
                return;
            }
        }


        if (path.StartsWith("http://") || path.StartsWith("https://"))
        {
            www = new WWW(path);
            return;
        }
        www = new WWW("file://" + path);
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

	public PUMarkdown markdownEntity;

	public virtual string DefaultFont() {
		return "Fonts/ARIAL_SDF";
	}
	
	public virtual float DefaultFontSize() {
		return PlanetUnityStyle.GlobalFontSize;
	}


	public virtual void Begin(PUGameObject container) {
		
	}

	public virtual void End(PUGameObject container) {
		
	}

	#region Body

	public virtual PUTMPro Create_P(PUGameObject container, string content) {
		return null;
	}

	#endregion

	#region Definitions
	
	public virtual void Create_DefinitionTerm(PUGameObject container, string content) {
		
	}

	public virtual void Create_DefinitionData(PUGameObject container, string content) {
		
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
	
	public virtual void Tag_Link(PUGameObject container, StringBuilder content, string url, string text) {
		content.AppendFormat ("[link]{0}[/link]", text);
	}

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
