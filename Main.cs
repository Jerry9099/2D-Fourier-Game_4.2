using Godot;
using System;
using System.Reflection;

using AForge.Math;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.IO;

public partial class Main : Control
{	
	FileDialog filedialog = new FileDialog();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		filedialog = GetNode<FileDialog>("FileDialog");
		filedialog.FileSelected += OnFileSelected; 
		GetNode<Button>("Buttons/Upload").Pressed += OnFileButtonLoad; 
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Image importImage = GetNode<SubViewport>("MovableViewer/SubViewport").GetTexture().GetImage();
		//Image importImage = new Image();
		Generate_FFT(importImage);
		//get_node("FFT_Display").texture = ImageTexture.create_from_image(dynImage);
	}

	public void Generate_FFT(Image importImage)
	{
		//convert Image data to array of Complex objects
		Image outImage = new Image();
		outImage.CopyFrom(importImage); //create new outIm
		int N = importImage.GetWidth();
		Complex[,] data = new Complex[N, N];
		for (int i = 0; i < N; i++)
		{
			for (int j = 0; j < N; j++)
			{
				float v = (importImage.GetPixel(i, j).R + importImage.GetPixel(i, j).G + importImage.GetPixel(i, j).B) / 3;  //avg, NOOB greyscale - IMPROVE?
				data[i, j] = new AForge.Math.Complex(v*255, 0);  //SCALE UP LATER? precision limited?
				//GD.Print(data[i, j]);
			}
		}
		//perform FFT2 in place**************************************************
		FourierTransform.FFT2(data, FourierTransform.Direction.Forward); 
		//***********************************************************************

		// set results into Image - ORIGINAL, UNSHIFTED
		// for (int i = 0; i < N; i++)
		// {
		// 	for (int j = 0; j < N; j++)
		// 	{
		// 		float a = 10*(float)data[i, j].Re;
		// 		Color c = new Color(a, a, a);
		// 		outImage.SetPixel(i, j, c);
		// 		//GD.Print(a);
		// 	}
		// }

		// //set results into Image - SHIFTED
		for (int i = 0; i < N/2; i++)  //left half
		{
			for (int j = 0; j < N/2; j++) //top left half
			{
				float a = 10*(float)data[j,i].Re;
				Color c = new Color(a, a, a);
				outImage.SetPixel(N/2-i-1, N/2-j-1, c);
			}

			for (int j = N-1; j > N/2-1; j--) //bottom left half
			{
				float a = 10*(float)data[j, i].Re;
				Color c = new Color(a, a, a);
				outImage.SetPixel(N/2-i-1, N-j-1+ N/2, c);
			}
		}

		for (int i = N-1; i > N/2-1; i--)  //left half
		{
			for (int j = 0; j < N/2; j++) //top left half
			{
				float a = 10*(float)data[j,i].Re;
				Color c = new Color(a, a, a);
				outImage.SetPixel(N-i-1 + N/2, N/2-j-1, c);
			}

			for (int j = N-1; j > N/2-1; j--) //bottom left half
			{
				float a = 10*(float)data[j, i].Re;
				Color c = new Color(a, a, a);
				outImage.SetPixel(N-i-1 + N/2, N-j-1+ N/2, c);
			}
		}

		//set FFT Texture to final image
		GetNode<TextureRect>("VBoxContainer/HBoxContainer/FFT/FFT").Texture = ImageTexture.CreateFromImage(outImage);
	}


	private void OnFileSelected(String path)
	{
		return; //load that shit
	}

	private void OnFileButtonLoad()
	{
		filedialog.Show();
	}
}
