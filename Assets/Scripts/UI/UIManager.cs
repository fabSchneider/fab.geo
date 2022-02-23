using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.Geo.UI
{
	[RequireComponent(typeof(UIDocument))]
	public class UIManager : MonoBehaviour
	{
		private UIDocument document;
		private Popup popup;
		public Popup Popup => popup;

		private ControlPanel controlPanel;
		public ControlPanel ControlPanel => controlPanel;

		void Awake()
		{
			document = GetComponent<UIDocument>();

			popup = new Popup(document.rootVisualElement);
			controlPanel = new ControlPanel(document.rootVisualElement);
		}
	}
}
