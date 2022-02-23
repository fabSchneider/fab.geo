using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.Geo.UI
{
	public class Popup
	{
		private static readonly string blockLayerClassName = "blocking-layer";
		private static readonly string className = "popup";
		private static readonly string headerClassName = className + "__header";
		private static readonly string contentClassName = className + "__content";
		private static readonly string footerClassName = className + "__footer";
		private static readonly string closeBtnClassName = className + "__close-btn";
		private static readonly string titleClassName = className + "__title";
		private static readonly string textClassName = className + "__text";
		private static readonly string imgClassName = className + "__img";

		private VisualElement root;
		private VisualElement popupRoot;
		private VisualElement popupElement;
		private Label titleLabel;

		private VisualElement header;
		private VisualElement content;
		private VisualElement footer;

		public Popup(VisualElement root)
		{
			this.root = root;

			popupRoot = new VisualElement();
			popupRoot.AddToClassList(blockLayerClassName);

			popupElement = new VisualElement();
			popupElement.AddToClassList(className);

			header = new VisualElement();
			header.AddToClassList(headerClassName);
			titleLabel = new Label();
			titleLabel.AddToClassList(titleClassName);
			Button closeButton = new Button(Close);
			closeButton.text = "";
			closeButton.AddToClassList(closeBtnClassName);

			header.Add(titleLabel);
			header.Add(closeButton);

			content = new VisualElement();
			content.AddToClassList(contentClassName);

			footer = new VisualElement();
			footer.AddToClassList(footerClassName);


			popupElement.Add(header);
			popupElement.Add(content);
			popupElement.Add(footer);

			popupRoot.Add(popupElement);
		}

		public bool IsOpen => popupRoot.parent != null;

		public void Close()
		{
			if (!IsOpen)
				return;

			popupRoot.Blur();
			popupRoot.RemoveFromHierarchy();
			titleLabel.text = null;
			content.Clear();
			footer.Clear();
		}


		public void Show()
		{
			if (IsOpen)
				Close();

			root.Add(popupRoot);
			popupElement.Focus();
		}

		public Popup WithTitle(string title)
		{
			titleLabel.text = title;
			return this;
		}

		public Popup WithText(string text)
		{
			Label textLabel = new Label(text);
			textLabel.AddToClassList(textClassName);
			content.Add(textLabel);
			return this;
		}

		public Popup WithImage(Texture2D image)
		{
			Image img = new Image();
			img.image = image;
			img.AddToClassList(imgClassName);
			img.style.maxWidth = image.width;
			img.style.maxHeight = image.height;
			content.Add(img);
			return this;
		}

		public Popup WithButton(string text, Action onClick)
		{
			Button btn = new Button(onClick);
			btn.text = text;
			footer.Add(btn);
			return this;
		}
	}
}
