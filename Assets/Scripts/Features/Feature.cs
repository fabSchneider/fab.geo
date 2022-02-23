using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Fab.Geo
{
	public abstract class Feature : MonoBehaviour, IPointerClickHandler
	{
		public event Action clicked;

		public abstract Coordinate[] Geometry { get; }

		protected int baseColorId;

		protected new Renderer renderer;
		protected MaterialPropertyBlock styleProps;

		protected virtual void Awake()
		{
			baseColorId = Shader.PropertyToID("_BaseColor");

			renderer = GetComponent<Renderer>();
			styleProps = new MaterialPropertyBlock();
			styleProps.SetColor(baseColorId, renderer.sharedMaterial.GetColor(baseColorId));
		}

		/// <summary>
		/// Sets the features name
		/// </summary>
		/// <param name="name"></param>
		public virtual void SetName(string name)
		{
			this.name = name;
		}

		/// <summary>
		/// Removes the feature
		/// </summary>
		public void Remove()
		{
			FeatureManager manager = GetComponentInParent<FeatureManager>();
			if (manager)
				manager.RemoveFeature(this);
			else
				Destroy(gameObject);
		}

		/// <summary>
		/// Sets the main color of this feature
		/// </summary>
		/// <param name="color"></param>
		public virtual void SetColor(Color color)
		{
			styleProps.SetColor(baseColorId, color);
			renderer.SetPropertyBlock(styleProps, 0);
		}

		/// <summary>
		/// Returns the main color of this feature
		/// </summary>
		/// <returns></returns>
		public virtual Color GetColor()
		{
			return styleProps.GetColor(baseColorId);
		}

		/// <summary>
		/// Resets the features style to its default state
		/// </summary>
		public virtual void ResetStyle()
		{
			styleProps.Clear();
			renderer.SetPropertyBlock(styleProps, 0);

			//set the default values in the styleProps 
			styleProps.SetColor(baseColorId, renderer.sharedMaterial.GetColor(baseColorId));
		}

		void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Left)
				clicked?.Invoke();
		}
	}
}
