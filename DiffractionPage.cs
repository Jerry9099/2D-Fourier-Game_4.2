using Godot;
using System;
using System.Reflection;

using AForge.Math;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.IO;

public partial class DiffractionPage : Control
{	
	FileDialog file_dialog = new FileDialog();
	Sprite2D import_sprite = new Sprite2D();
	Camera2D import_camera = new Camera2D();
	TextureRect fft = new TextureRect();
	//static String movable_viewport_path = "MovableViewer/SubViewport";
	static String import_viewport_path = "VBoxContainer/HBoxContainer/DisplayedImage/SubViewportContainer/SubViewport";
	static String import_sprite_path = import_viewport_path + "/DrawParent/InputTexture"; 
	static String viewport_path = import_viewport_path;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		file_dialog = GetNode<FileDialog>("FileDialog");
		import_sprite = GetNode<Sprite2D>(import_sprite_path);
		import_camera = GetNode<Camera2D>(import_viewport_path + "/Camera2D");
		fft = GetNode<TextureRect>("VBoxContainer/HBoxContainer/FFT/FFT");

		file_dialog.FileSelected += OnFileSelected; 
		GetNode<Button>("Buttons/Upload").Pressed += OnFileButtonLoad; 
		GetNode<Button>("Buttons/ViewFinder").Pressed += OnViewFinderButtonPress;
		GetNode<HSlider>("VBoxContainer/HBoxContainer/DisplayedImage/SizeSlider").ValueChanged += OnSizeSliderValueChanged;
		GetNode<HSlider>("VBoxContainer/HBoxContainer/DisplayedImage/XSlider").ValueChanged += OnXSliderValueChanged;
		GetNode<HSlider>("VBoxContainer/HBoxContainer/DisplayedImage/YSlider").ValueChanged += OnYSliderValueChanged;
	}


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
		Image importImage = GetNode<SubViewport>(viewport_path).GetTexture().GetImage();
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

		int mag = 2;
		for (int i = 0; i < N/2; i++)  //left half
		{
			for (int j = 0; j < N/2; j++) //top left half
			{
				float a = mag*(float)data[j,i].Magnitude;
				Color c = new Color(a, a, a);
				outImage.SetPixel(N/2-i-1, N/2-j-1, c);
			}

			for (int j = N-1; j > N/2-1; j--) //bottom left half
			{
				float a = mag*(float)data[j, i].Magnitude;
				Color c = new Color(a, a, a);
				outImage.SetPixel(N/2-i-1, N-j-1+ N/2, c);
			}
		}

		for (int i = N-1; i > N/2-1; i--)  //left half
		{
			for (int j = 0; j < N/2; j++) //top left half
			{
				float a = mag*(float)data[j,i].Magnitude;
				Color c = new Color(a, a, a);
				outImage.SetPixel(N-i-1 + N/2, N/2-j-1, c);
			}

			for (int j = N-1; j > N/2-1; j--) //bottom left half
			{
				float a = mag*(float)data[j, i].Magnitude;
				Color c = new Color(a, a, a);
				outImage.SetPixel(N-i-1 + N/2, N-j-1+ N/2, c);
			}
		}

		//set FFT Texture to final image
		fft.Texture = ImageTexture.CreateFromImage(outImage);
	}


	private void OnFileSelected(String path)
	{
		Image image = Image.LoadFromFile(path);
		import_sprite.Texture = ImageTexture.CreateFromImage(image);
	}

	private void OnFileButtonLoad()
	{
		file_dialog.Show();
	}

	private void OnViewFinderButtonPress()
	{
		//if ( viewport_path.Equals(movable_viewport_path) )
		//{
		//	viewport_path = import_viewport_path;
		//}

		//else
		//{
		//d	viewport_path = movable_viewport_path;
		//}
		return;
	}

	private void OnSizeSliderValueChanged(double value)
	{
		Vector2 val = new Vector2((float)value, (float)value);
		//GetNode<Camera2D>(import_viewport_path + "/Camera2D").Zoom = val;
		GetNode<Node2D>(import_viewport_path + "/DrawParent").Scale = val;
	}

	private void OnXSliderValueChanged(double value)
	{
		Vector2 val = new Vector2((float)value, import_sprite.Offset.Y);
		//import_sprite.Offset = val;
		//Vector2 x = new Vector2(128, 0);
		import_sprite.RotationDegrees = (float)value;
		//import_camera.Offset = val + x;
	}

	private void OnYSliderValueChanged(double value)
	{
		Vector2 val = new Vector2(import_sprite.Offset.X, (float)value);
		import_sprite.Offset = val;
		Vector2 y = new Vector2(0, 128);
		//import_camera.Offset = val + y;
	}
	
	
}
