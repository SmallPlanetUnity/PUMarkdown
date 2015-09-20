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

	public MarkdownStyle style;
	public string value;

	public override void gaxb_final(XmlReader reader, object _parent, Hashtable args) {
		base.gaxb_final(reader, _parent, args);

		if (reader != null) {
			string s = reader.GetAttribute ("style");
			if (s != null) {
				string classString = "MarkdownStyle"+s;

				Type entityClass = Type.GetType (classString, true);
				style = (Activator.CreateInstance (entityClass)) as MarkdownStyle;
			}

			s = reader.GetAttribute ("value");
			if (s != null) {
				value = PlanetUnityStyle.ReplaceStyleTags(PlanetUnityLanguage.Translate(s));
			}
		}

		if (style == null) {
			style = new MarkdownStyleGeneric();
		}

	}

	// This is required for application-level subclasses
	public override void gaxb_init () {
		base.gaxb_init ();
		gaxb_addToParent();
	}

	public override void gaxb_complete () {
		base.gaxb_complete ();

		ScheduleForUpdate ();
	}

	private float lastWidth = 0;
	public override void Update () {
		if (lastWidth != rectTransform.rect.size.x) {
			lastWidth = rectTransform.rect.size.x;
			LoadMarkdown(value);
		}
	}

	public void LoadMarkdown(string content) {
		// first, clear ourselves
		unloadAllChildren ();

		// run the parser on the content
		Markdown md = new Markdown ();
		Dictionary<string, LinkDefinition> definitions;

		md.ExtraMode = true;

		Debug.Log ("value: " + content);


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
				style.Create_P(container, currentString.ToString());
			}

			if (currentBlock.blockType == BlockType.h1) {
				style.Create_H1(container, currentString.ToString());
			}

			if (currentBlock.blockType == BlockType.h2) {
				style.Create_H2(container, currentString.ToString());
			}

			if (currentBlock.blockType == BlockType.h3) {
				style.Create_H3(container, currentString.ToString());
			}

			if (currentBlock.blockType == BlockType.h4) {
				style.Create_H4(container, currentString.ToString());
			}

			if (currentBlock.blockType == BlockType.h5) {
				style.Create_H5(container, currentString.ToString());
			}

			if (currentBlock.blockType == BlockType.h6) {
				style.Create_H6(container, currentString.ToString());
			}

			if (currentBlock.blockType == BlockType.hr) {
				style.Create_HR(container);
			}

			if (currentBlock.blockType == BlockType.quote) {
				style.Begin_Blockquote(container);
			}

			if (currentBlock.blockType == BlockType.quote_end) {
				style.End_Blockquote(container);
			}

			if (currentBlock.blockType == BlockType.ul) {
				listStack.Push(currentBlock.blockType);
				style.Begin_UnorderedList(container);
			}
			
			if (currentBlock.blockType == BlockType.ul_end) {
				listStack.Pop();
				style.End_UnorderedList(container);
			}

			if (currentBlock.blockType == BlockType.ul_li) {
				style.Create_UL_LI(container, currentString.ToString());
			}

			if (currentBlock.blockType == BlockType.ol) {
				listStack.Push(currentBlock.blockType);
				style.Begin_OrderedList(container);
			}
			
			if (currentBlock.blockType == BlockType.ol_end) {
				listStack.Pop();
				style.End_OrderedList(container);
			}
			
			if (currentBlock.blockType == BlockType.ol_li) {
				style.Create_OL_LI(container, currentString.ToString());
			}

			if (currentBlock.blockType == BlockType.codeblock) {
				style.Create_CodeBlock(container, currentBlock.Content);
			}

			if(currentBlock.blockType == BlockType.table_spec){
				style.Create_Table(container, currentBlock.data as TableSpec);
			}

			if(currentBlock.blockType == BlockType.dt){
				style.Create_DefinitionTerm(container, currentString.ToString());
			}

			if(currentBlock.blockType == BlockType.dd){
				style.Create_DefinitionData(container, currentString.ToString());
			}

		};

		style.Begin (container);

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
					style.Create_IMG(container, link.def.url, link.link_text);
				}

				if(token.type == TokenType.Text){
					currentString.Append(tokenString, token.startOffset, token.length);
				}


				if(token.type == TokenType.code_span){
					style.Tag_Code(container, currentString, true);
					currentString.Append(tokenString, token.startOffset, token.length);
					style.Tag_Code(container, currentString, false);
				}

				if(token.type == TokenType.open_strong){
					style.Tag_Strong(container, currentString, true);
				}
				
				if(token.type == TokenType.close_strong){
					style.Tag_Strong(container, currentString, false);
				}
				
				if(token.type == TokenType.br){
					style.Tag_BreakingReturn(container, currentString);
				}

				if(token.type == TokenType.link){
					LinkInfo link = token.data as LinkInfo;
					style.Tag_Link(container, currentString, link.def.url, link.link_text);
				}
				
				if(token.type == TokenType.open_em){
					style.Tag_Emphasis(container, currentString, true);
				}
				
				if(token.type == TokenType.close_em){
					style.Tag_Emphasis(container, currentString, false);
				}


			}
		});

		CommitMarkdownBlock();

		style.End (container);

		CalculateContentSize ();

		Debug.Log ("html: " + htmlTranslation);
	}

}
