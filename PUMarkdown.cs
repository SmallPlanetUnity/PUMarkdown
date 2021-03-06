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
using System;
using MarkdownDeep;
using System.Collections.Generic;
using System.Text;

public class PUMarkdown : PUScrollRect {

	public MarkdownStyle mdStyle;

	public Action<string> onLinkClicked;

	public string style;
	public string value;

	public bool autoreload = true;
	public bool delayedLoad = false;

	public override void gaxb_final(TB.TBXMLElement element, object _parent, Hashtable args) {
		base.gaxb_final(element, _parent, args);

		if (element != null) {
			style = element.GetAttribute ("style");

			string s = element.GetAttribute ("value");
			if (s != null) {
				value = PlanetUnityStyle.ReplaceStyleTags(PlanetUnityOverride.processString(this, s));
			}

			s = element.GetAttribute ("autoreload");
			if (s != null) {
				autoreload = bool.Parse(s);
			}
		}

		if (style != null) {
			string classString = "MarkdownStyle" + style;
			
			Type entityClass = Type.GetType (classString, true);
			mdStyle = (Activator.CreateInstance (entityClass)) as MarkdownStyle;
		} else {
			mdStyle = new MarkdownStyleGeneric();
		}

		mdStyle.markdownEntity = this;

	}

	// This is required for application-level subclasses
	public override void gaxb_init () {
		base.gaxb_init ();
		gaxb_addToParent();
	}

	public override void gaxb_complete () {
		base.gaxb_complete ();

		ScheduleForUpdate ();

		if (PlanetUnityGameObject.MainCanvas () != null) {
			if(delayedLoad == false){
				Update ();
			}
		}
	}

	private float lastWidth = 0;
	private float lastHeight = 0;
	public override void Update () {

        if (Input.GetMouseButton(0) == false)
        {
            // disable scrolling if we fit...
            RectTransform myRectTransform = (RectTransform)contentObject.transform;
            if (myRectTransform.rect.height < this.rectTransform.rect.height)
            {
                if (scroll.enabled != false)
                {
                    scroll.enabled = false;
                    myRectTransform.anchoredPosition = Vector2.zero;
                }
            }
            else
            {
                if (scroll.enabled != true)
                {
                    scroll.enabled = true;
                }
            }
        }

		if (autoreload == false && lastWidth != 0 && lastHeight != 0) {
			return;
		}

		if (lastWidth != rectTransform.rect.size.x ||
			lastHeight != rectTransform.rect.size.y) {
			lastWidth = rectTransform.rect.size.x;
			lastHeight = rectTransform.rect.size.y;
			Reload ();
		}
	}

	public void Reload() {
		LoadMarkdown(value);
	}

	public void LoadMarkdown(string content) {
        if (Application.isEditor)
        {
            Debug.Log(content);
        }

		// first, clear ourselves
		unloadAllChildren ();

		value = content;

		// run the parser on the content
		Markdown md = new Markdown ();
		Dictionary<string, LinkDefinition> definitions;

		md.ExtraMode = true;

		// Our job is to interface with MarkdownDeep, grab the necessary bits, and call the simplied API in MarkdownStyle
		StringBuilder currentString = new StringBuilder ();
		Block currentBlock = null;
		PUGameObject container = this as PUGameObject;
		RectTransform containerRT = contentObject.transform as RectTransform;
		Stack<BlockType> listStack = new Stack<BlockType>();

		container.size = containerRT.rect.size;

		Action CommitMarkdownBlock = () => {

			if (currentBlock == null) {
				return;
			}

			if (currentBlock.blockType == BlockType.p) {
				mdStyle.Create_P(container, currentString.ToString());
			}

			if (currentBlock.blockType == BlockType.h1) {
				mdStyle.Create_H1(container, currentString.ToString());
			}

			if (currentBlock.blockType == BlockType.h2) {
				mdStyle.Create_H2(container, currentString.ToString());
			}

			if (currentBlock.blockType == BlockType.h3) {
				mdStyle.Create_H3(container, currentString.ToString());
			}

			if (currentBlock.blockType == BlockType.h4) {
				mdStyle.Create_H4(container, currentString.ToString());
			}

			if (currentBlock.blockType == BlockType.h5) {
				mdStyle.Create_H5(container, currentString.ToString());
			}

			if (currentBlock.blockType == BlockType.h6) {
				mdStyle.Create_H6(container, currentString.ToString());
			}

			if (currentBlock.blockType == BlockType.hr) {
				mdStyle.Create_HR(container);
			}

			if (currentBlock.blockType == BlockType.quote) {
				mdStyle.Begin_Blockquote(container);
			}

			if (currentBlock.blockType == BlockType.quote_end) {
				mdStyle.End_Blockquote(container);
			}

			if (currentBlock.blockType == BlockType.ul) {
				listStack.Push(currentBlock.blockType);
				mdStyle.Begin_UnorderedList(container);
			}
			
			if (currentBlock.blockType == BlockType.ul_end) {
				listStack.Pop();
				mdStyle.End_UnorderedList(container);
			}

			if (currentBlock.blockType == BlockType.ul_li) {
				mdStyle.Create_UL_LI(container, currentString.ToString());
			}

			if (currentBlock.blockType == BlockType.ol) {
				listStack.Push(currentBlock.blockType);
				mdStyle.Begin_OrderedList(container);
			}
			
			if (currentBlock.blockType == BlockType.ol_end) {
				listStack.Pop();
				mdStyle.End_OrderedList(container);
			}
			
			if (currentBlock.blockType == BlockType.ol_li) {
				mdStyle.Create_OL_LI(container, currentString.ToString());
			}

			if (currentBlock.blockType == BlockType.codeblock) {
				mdStyle.Create_CodeBlock(container, currentBlock.Content);
			}

			if(currentBlock.blockType == BlockType.table_spec){
				mdStyle.Create_Table(container, currentBlock.data as TableSpec);
			}

			if(currentBlock.blockType == BlockType.dt){
				mdStyle.Create_DefinitionTerm(container, currentString.ToString());
			}

			if(currentBlock.blockType == BlockType.dd){
				mdStyle.Create_DefinitionData(container, currentString.ToString());
			}

		};

		mdStyle.Begin (container);

		string htmlTranslation = md.Transform (content, out definitions, (block, token, tokenString) => {

			if(block != null){
				//Debug.Log ("block: " + block.blockType + " :: " + block.Content);

				CommitMarkdownBlock();

				currentBlock = block;

				currentString.Length = 0;
			}

			if(token != null) {
				//Debug.Log ("token: " + token.type);

				if(token.type == TokenType.img){

					LinkInfo link = token.data as LinkInfo;
					mdStyle.Create_IMG(container, link.def.url, link.link_text);
				}

				if(token.type == TokenType.Text){
					currentString.Append(tokenString, token.startOffset, token.length);
				}


                if (token.type == TokenType.code_span) {
                    string codeContent = tokenString.Substring(token.startOffset, token.length);
                    if (mdStyle.Tag_Code(container, currentString, codeContent, true)) {
                        currentString.Append(codeContent);
                    }
                    mdStyle.Tag_Code(container, currentString, codeContent, false);
                }

				if(token.type == TokenType.open_strong){
					mdStyle.Tag_Strong(container, currentString, true);
				}
				
				if(token.type == TokenType.close_strong){
					mdStyle.Tag_Strong(container, currentString, false);
				}
				
				if(token.type == TokenType.br){
					mdStyle.Tag_BreakingReturn(container, currentString);
				}

				if(token.type == TokenType.link){
					LinkInfo link = token.data as LinkInfo;
					mdStyle.Tag_Link(container, currentString, link.def.url, link.link_text);
				}
				
				if(token.type == TokenType.open_em){
					mdStyle.Tag_Emphasis(container, currentString, true);
				}
				
				if(token.type == TokenType.close_em){
					mdStyle.Tag_Emphasis(container, currentString, false);
				}


			}
		});

		CommitMarkdownBlock();

		mdStyle.End (container);

		CalculateContentSize ();
	}

}
