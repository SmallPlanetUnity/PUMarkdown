
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
		BlockType currentBlockType = BlockType.Blank;
		PUGameObject container = this as PUGameObject;
		RectTransform containerRT = contentObject.transform as RectTransform;
		Stack<BlockType> listStack = new Stack<BlockType>();

		container.size = containerRT.rect.size;

		Action CommitMarkdownBlock = () => {

			if (currentBlockType == BlockType.p) {
				style.Create_P(container, currentString.ToString());
			}

			if (currentBlockType == BlockType.h1) {
				style.Create_H1(container, currentString.ToString());
			}

			if (currentBlockType == BlockType.h2) {
				style.Create_H2(container, currentString.ToString());
			}

			if (currentBlockType == BlockType.h3) {
				style.Create_H3(container, currentString.ToString());
			}

			if (currentBlockType == BlockType.h4) {
				style.Create_H4(container, currentString.ToString());
			}

			if (currentBlockType == BlockType.h5) {
				style.Create_H5(container, currentString.ToString());
			}

			if (currentBlockType == BlockType.h6) {
				style.Create_H6(container, currentString.ToString());
			}

			if (currentBlockType == BlockType.quote) {
				style.Begin_Blockquote(container);
			}

			if (currentBlockType == BlockType.quote_end) {
				style.End_Blockquote(container);
			}

			if (currentBlockType == BlockType.ul) {
				listStack.Push(currentBlockType);
				style.Begin_UnorderedList(container);
			}
			
			if (currentBlockType == BlockType.ul_end) {
				listStack.Pop();
				style.End_UnorderedList(container);
			}

			if (currentBlockType == BlockType.ul_li) {
				style.Create_UL_LI(container, currentString.ToString());
			}

			if (currentBlockType == BlockType.ol) {
				listStack.Push(currentBlockType);
				style.Begin_OrderedList(container);
			}
			
			if (currentBlockType == BlockType.ol_end) {
				listStack.Pop();
				style.End_OrderedList(container);
			}
			
			if (currentBlockType == BlockType.ol_li) {
				style.Create_OL_LI(container, currentString.ToString());
			}

			// This one is tricky; MarkdownDeep doesn't seem to handle well OL and UL following each
			// other and returns LI instead of OL_LI or UL_LI. So, we try and determine which one we're in.
			// and send that instead...
			/*
			if (currentBlockType == BlockType.li) {
				BlockType tempType = listStack.Peek();
				if(tempType == BlockType.ul)
					style.Create_UL_LI(container, currentString.ToString());
				if(tempType == BlockType.ol)
					style.Create_OL_LI(container, currentString.ToString());
			}*/

		};

		style.Begin (container);

		string htmlTranslation = md.Transform (content, out definitions, (block, token, tokenString) => {

			if(block != null){
				Debug.Log ("block: " + block.blockType);

				CommitMarkdownBlock();

				currentBlockType = block.blockType;

				currentString.Length = 0;
			}

			if(token != null) {
				Debug.Log ("token: " + token.type);

				if(token.type == TokenType.Text){
					currentString.Append(tokenString, token.startOffset, token.length);
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
