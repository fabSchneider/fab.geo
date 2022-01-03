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
        private Label textLabel;
        private Image img;

        public Popup(VisualElement root)
        {
            this.root = root;

            popupRoot = new VisualElement();
            popupRoot.AddToClassList(blockLayerClassName);

            popupElement = new VisualElement();
            popupElement.AddToClassList(className);

            VisualElement header = new VisualElement();
            header.AddToClassList(headerClassName);
            titleLabel = new Label();
            titleLabel.AddToClassList(titleClassName);
            Button closeButton = new Button(Close);
            closeButton.text = "";
            closeButton.AddToClassList(closeBtnClassName);
            header.Add(titleLabel);
            header.Add(closeButton);

            VisualElement content = new VisualElement();
            content.AddToClassList(contentClassName);
            textLabel = new Label();
            textLabel.AddToClassList(textClassName);
            img = new Image();
            img.AddToClassList(imgClassName);
            content.Add(img);
            content.Add(textLabel);

            VisualElement footer = new VisualElement();
            footer.AddToClassList(footerClassName);

            popupElement.Add(header);
            popupElement.Add(content);
            popupElement.Add(footer);

            popupRoot.Add(popupElement);
        }

        public void Close()
        {
            popupElement.Blur();
            popupRoot.RemoveFromHierarchy();
            titleLabel.text = null;
            textLabel.text = null;
            img.image = null;
            img.style.maxWidth = StyleKeyword.Null;
            img.style.maxWidth = StyleKeyword.Null;
        }

        public void Show(string title, string text)
        {
            titleLabel.text = title;
            img.style.display = DisplayStyle.None;
            textLabel.text = text;
            textLabel.style.display = DisplayStyle.Flex;

            root.Add(popupRoot);
            popupElement.Focus();
        }

        public void Show(string title, Texture2D image)
        {
            titleLabel.text = title;
            textLabel.style.display = DisplayStyle.None;
            img.image = image;
            img.style.maxWidth = image.width;
            img.style.maxHeight = image.height;
            img.style.display = DisplayStyle.Flex;
            root.Add(popupRoot);
            popupElement.Focus();
        }
    }
}
