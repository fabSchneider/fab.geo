using UnityEngine;

namespace Fab.Geo.Gen
{
	public class GenerateDistanceField
	{
		private static readonly string ComputeShaderPath = "Compute/JumpFloodDistance";

		private static readonly string InitMaskKernelName = "InitMask";
		private static readonly string InitMaskInvertKernelName = "InitMask_Invert";
		private static readonly string JFAKernelName = "JFA";
		private static readonly string FillDistanceTransformKernelName = "FillDistanceTransform";

		private int InitMaskKernel;
		private int InitMaskInvertKernel;
		private int JFAKernel;
		private int FillDistanceTransformKernel;

		private ComputeShader compute;
		private RenderTexture tmp1, tmp2;

		public bool invertMask;

		public GenerateDistanceField()
		{
			compute = Resources.Load<ComputeShader>(ComputeShaderPath);

			if (compute == null)
				Debug.LogError("Distance field compute shader could not be found");

			InitMaskKernel = compute.FindKernel(InitMaskKernelName);
			InitMaskInvertKernel = compute.FindKernel(InitMaskInvertKernelName);
			JFAKernel = compute.FindKernel(JFAKernelName);
			FillDistanceTransformKernel = compute.FindKernel(FillDistanceTransformKernelName);
		}

		public void Generate(Texture2D mask, RenderTexture dest)
		{
			InitRenderTexture(mask);

			int threadGroupsX = Mathf.CeilToInt(mask.width / 8.0f);
			int threadGroupsY = Mathf.CeilToInt(mask.height / 8.0f);

			// Init Mask
			Graphics.Blit(mask, tmp1);

			compute.SetInt("Width", mask.width);
			compute.SetInt("Height", mask.height);

			int kernel = invertMask ? InitMaskInvertKernel : InitMaskKernel;
			compute.SetTexture(kernel, "Source", tmp1);
			compute.Dispatch(kernel, threadGroupsX, threadGroupsY, 1);

			// JFA
			int stepAmount = (int)Mathf.Log(Mathf.Max(mask.width, mask.height), 2);

			for (int i = 0; i < stepAmount; i++)
			{
				int step = (int)Mathf.Pow(2, stepAmount - i - 1);
				compute.SetInt("Step", step);
				compute.SetTexture(JFAKernel, "Source", tmp1);
				compute.SetTexture(JFAKernel, "Result", tmp2);
				compute.Dispatch(JFAKernel, threadGroupsX, threadGroupsY, 1);
				Graphics.Blit(tmp2, tmp1);

				compute.SetTexture(FillDistanceTransformKernel, "Source", tmp1);
				compute.SetTexture(FillDistanceTransformKernel, "Result", tmp2);
				compute.Dispatch(FillDistanceTransformKernel, threadGroupsX, threadGroupsY, 1);
			}

			compute.SetTexture(FillDistanceTransformKernel, "Source", tmp1);
			compute.SetTexture(FillDistanceTransformKernel, "Result", tmp2);
			compute.Dispatch(FillDistanceTransformKernel, threadGroupsX, threadGroupsY, 1);

			Graphics.Blit(tmp2, dest);

			tmp1.Release();
			tmp1 = null;
			tmp2.Release();
			tmp2 = null;
		}

		private void InitRenderTexture(Texture source)
		{
			if (tmp1 == null || tmp1.width != source.width || tmp1.height != source.height)
			{
				tmp1 = new RenderTexture(source.width, source.height, 0,
					RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
				tmp1.enableRandomWrite = true;
				tmp1.Create();

				tmp2 = new RenderTexture(source.width, source.height, 0,
				   RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
				tmp2.enableRandomWrite = true;
				tmp2.Create();
			}
		}
	}
}
