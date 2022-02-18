using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.Geo.UI
{
	/// <summary>
	/// Utility class for building a VisualElement hierachy from a path
	/// </summary>
	public class VisualElementHierachyBuilder
	{
		public static readonly string GroupNameSuffix = "-group";

		private VisualElement root;
		private char separatorChar;

		private Func<string, VisualElement> createGroupFunc;

		/// <summary>
		/// </summary>
		/// <param name="root">The root element of the hierachy</param>
		/// <param name="createGroupFunc">The function creating a hierachy element</param>
		/// <param name="separatorChar">The separator charactor to split the path by</param>
		public VisualElementHierachyBuilder(VisualElement root, Func<string, VisualElement> createGroupFunc, char separatorChar = '.')
		{
			this.root = root;
			this.separatorChar = separatorChar;
			this.createGroupFunc = createGroupFunc;
		}

		/// <summary>
		/// Gets the elements name from a path
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public string GetName(string path)
		{
			if (path == null)
				return null;

			int lastIndex = path.LastIndexOf(separatorChar);
			if (lastIndex == -1)
				return path;
			else
				return path.Substring(lastIndex + 1);
		}

		/// <summary>
		///  Returns the element at the given path
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public VisualElement GetElementAtPath(string path)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));

			VisualElement parent = root;

			int separatorIndex = path.IndexOf(separatorChar);
			string workPath = path;

			while (separatorIndex != -1)
			{
				string parentName = workPath.Substring(0, separatorIndex);
				workPath = workPath.Substring(separatorIndex + 1);

				VisualElement group = parent.Q(name: parentName + GroupNameSuffix);

				if (group == null)
					return null;
				parent = group;

				separatorIndex = workPath.IndexOf(separatorChar);
			}

			return parent.Q(name: workPath);

		}

		/// <summary>
		/// Adds an element to the hierachy
		/// </summary>
		/// <param name="element">The element to add</param>
		/// <param name="path">The path of the element</param>
		/// <exception cref="ArgumentNullException"></exception>
		public void AddToHierachy(VisualElement element, string path)
		{
			if (element == null)
				throw new ArgumentNullException(nameof(element));

			if (path == null)
				throw new ArgumentNullException(nameof(path));

			VisualElement parent = root;

			int separatorIndex = path.IndexOf(separatorChar);
			string workPath = path;

			while (separatorIndex != -1)
			{
				string parentName = workPath.Substring(0, separatorIndex);
				workPath = workPath.Substring(separatorIndex + 1);

				VisualElement group = parent.Q(name: parentName + GroupNameSuffix);

				if (group == null)
				{
					group = CreateGroup(parentName);
					parent.Add(group);
				}
				parent = group;

				separatorIndex = workPath.IndexOf(separatorChar);
			}

			element.name = workPath;
			parent.Add(element);
		}

		/// <summary>
		/// Removes all elements leaving only the root
		/// </summary>
		public void RemoveAll()
		{
			root.Clear();
		}

		/// <summary>
		/// Removes an element at the given path
		/// </summary>
		/// <param name="path"></param>
		/// <returns>Returns true if the element was found and removed.False otherwise</returns>
		public bool RemoveElement(string path)
		{
			VisualElement element = GetElementAtPath(path);
			if (element == null)
				return false;

			VisualElement parent = element.parent;
			element.RemoveFromHierarchy();

			//recursively remove empty parents
			while (parent != root && parent.childCount == 0)
			{
				VisualElement newParent = parent.parent;
				parent.RemoveFromHierarchy();
				parent = newParent;
			}

			return true;
		}

		private VisualElement CreateGroup(string name)
		{
			VisualElement group = createGroupFunc(name);
			group.name = name + GroupNameSuffix;
			return group;
		}
	}
}
