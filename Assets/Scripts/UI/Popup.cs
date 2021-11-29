using UnityEngine.UIElements;

namespace Fab.Geo
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

        private VisualElement root;
        private VisualElement popupRoot;
        private VisualElement popupElement;
        private Label titleLabel;
        private Label textLabel;

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
            closeButton.text = "x";
            closeButton.AddToClassList(closeBtnClassName);
            header.Add(titleLabel);
            header.Add(closeButton);

            VisualElement content = new VisualElement();
            content.AddToClassList(contentClassName);
            textLabel = new Label();
            textLabel.AddToClassList(textClassName);
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
        }

        public void Show(string title, string text)
        {
            titleLabel.text = title;
            textLabel.text = text;

            root.Add(popupRoot);
            popupElement.Focus();
        }
    }
}
