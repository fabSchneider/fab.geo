using Fab.Geo.Lua.Core;
using Fab.Geo.UI;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Fab.Geo.Lua.Interop
{
	[LuaHelpInfo("Module to create a popup and show it on screen")]
	public class Popup : LuaObject, ILuaObjectInitialize
	{
		private UI.Popup popup;
		public void Initialize()
		{
			UIManager manager = GameObject.FindObjectOfType<UIManager>();

			if (!manager)
				throw new LuaObjectInitializationException("UI Manager could not be found");

			popup = manager.Popup;
		}

		[LuaHelpInfo("Shows the popup")]
		public void show() => popup.Show();

		[LuaHelpInfo("Sets the title of the popup")]
		public Popup title(string title)
		{
			popup.WithTitle(title);
			return this;
		}

		[LuaHelpInfo("Adds some text to the popup")]
		public Popup text(string text)
		{
			popup.WithText(text);
			return this;
		}

		[LuaHelpInfo("Adds an image to the popup")]
		public Popup image(ImageProxy image)
		{
			popup.WithImage(image.Target);
			return this;
		}

		[LuaHelpInfo("Adds a button to the bottom of the popup")]
		public Popup button(string text, Closure action)
		{
			popup.WithButton(text, () => action.Call());
			return this;
		}

		[LuaHelpInfo("Closes any open popup")]
		public void close()
		{
			popup.Close();
		}
	}
}
