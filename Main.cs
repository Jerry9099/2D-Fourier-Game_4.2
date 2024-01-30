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
	static int N_DEFAULT = 256;
	//Complex[,] xfmed_data = new Complex[N_DEFAULT, N_DEFAULT];
	FileDialog file_dialog = new FileDialog();
	Sprite2D import_sprite = new Sprite2D();
	TextureRect fft = new TextureRect();
	TextureRect ifft = new TextureRect();
	TextureRect ifft_mask = new TextureRect();
	SubViewport ifft_viewport = new SubViewport();
	ImageTexture black_texture = new ImageTexture();
	Complex[,] data = new Complex[N_DEFAULT, N_DEFAULT];
	//static String movable_viewport_path = "MovableViewer/SubViewport";
	static String import_viewport_path = "VBoxContainer/HBoxContainer/DisplayedImage/SubViewportContainer/SubViewport";
	static String import_sprite_path = import_viewport_path + "/DrawParent/InputTexture"; 
	static String viewport_path = import_viewport_path;
	static String fft_viewport_path = "VBoxContainer/HBoxContainer/FFT/SubViewportContainer/SubViewport";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		file_dialog = GetNode<FileDialog>("FileDialog");
		import_sprite = GetNode<Sprite2D>(import_sprite_path);
		fft = GetNode<TextureRect>("VBoxContainer/HBoxContainer/FFT/SubViewportContainer/SubViewport/FFT");
		ifft = GetNode<TextureRect>("VBoxContainer/HBoxContainer/DisplayedImage/IFFT");
		ifft_mask = GetNode<TextureRect>("VBoxContainer/HBoxContainer/FFT/IFFTContainer/SubViewport/IFFTMask");
		ifft_viewport = GetNode<SubViewport>("VBoxContainer/HBoxContainer/FFT/IFFTContainer/SubViewport");
		black_texture = ImageTexture.CreateFromImage(Image.LoadFromFile("res://fill.png"));

		//connect Signals
		file_dialog.FileSelected += OnFileSelected; 
		GetNode<Button>("Buttons/Upload").Pressed += OnFileButtonLoad; 
		GetNode<Button>("Buttons/ViewFinder").Pressed += OnViewFinderButtonPress;
		GetNode<HSlider>("Controls/SizeSlider").ValueChanged += OnSizeSliderValueChanged;
		GetNode<HSlider>("Controls/XSlider").ValueChanged += OnXSliderValueChanged;
		GetNode<HSlider>("Controls/YSlider").ValueChanged += OnYSliderValueChanged;

		//DEBUG
		//ImageTexture PaintTexture = ImageTexture.CreateFromImage(Image.LoadFromFile("res://icon.svg"));
		//ifft.Texture = PaintTexture;
	}


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
		Image importImage = GetNode<SubViewport>(viewport_path).GetTexture().GetImage();
		//Image importImage = new Image();
		Generate_FFT(importImage);
		//get_node("FFT_Display").texture = ImageTexture.create_from_image(dynImage);
		//Image fftImage = GetNode<SubViewport>(fft_viewport_path).GetTexture().GetImage();
		Generate_IFFT(importImage, data);
	}

	public void Generate_FFT(Image importImage)
	{
		//convert Image data to array of Complex objects
		Image outImage = new Image();
		outImage.CopyFrom(importImage); //create new outIm
		int N = importImage.GetWidth();
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
		//xfmed_data = data; //copy to main-wide var
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

	public void Generate_IFFT(Image importImage, Complex[,] data)
	{
		//convert Image data to array of Complex objects
		Image outImage = new Image();
		outImage.CopyFrom(importImage); //create new outIm
		int N = importImage.GetWidth();
		//perform FFT2 in place**************************************************
		Image mask_image = ifft_viewport.GetTexture().GetImage();
		for (int i = 0; i < N; i++)
		{
			for (int j = 0; j < N; j++)
			{
				double v = (mask_image.GetPixel(i, j).R + mask_image.GetPixel(i, j).G + mask_image.GetPixel(i, j).B) / 3; 
				data[i, j] = Complex.Multiply(data[i, j], v);
				//GD.Print(v);
			}
		}

		//perform FFT2 in place - SHIFTED***************************************
		// Complex[,] xfmed_data = data;
		// for (int i = 0; i < N/2; i++)  //left half
		// {
		// 	for (int j = 0; j < N/2; j++) //top left half
		// 	{
		// 		double v = (mask_image.GetPixel(j, i).R + mask_image.GetPixel(j, i).G + mask_image.GetPixel(j, i).B) / 3; 
		// 		xfmed_data[j, i] = Complex.Multiply(data[N/2-i-1, N/2-j-1], v);
		// 	}

		// 	for (int j = N-1; j > N/2-1; j--) //bottom left half
		// 	{
		// 		double v = (mask_image.GetPixel(j, i).R + mask_image.GetPixel(j, i).G + mask_image.GetPixel(j, i).B) / 3; 
		// 		xfmed_data[j, i] = Complex.Multiply(data[N/2-i-1, N-j-1+ N/2], v);
		// 	}
		// }

		// for (int i = N-1; i > N/2-1; i--)  //left half
		// {
		// 	for (int j = 0; j < N/2; j++) //top left half
		// 	{
		// 		double v = (mask_image.GetPixel(j, i).R + mask_image.GetPixel(j, i).G + mask_image.GetPixel(j, i).B) / 3; 
		// 		xfmed_data[j, i] = Complex.Multiply(data[N-i-1 + N/2, N/2-j-1], v);
		// 	}

		// 	for (int j = N-1; j > N/2-1; j--) //bottom left half
		// 	{
		// 		double v = (mask_image.GetPixel(j, i).R + mask_image.GetPixel(j, i).G + mask_image.GetPixel(j, i).B) / 3; 
		// 		xfmed_data[j, i] = Complex.Multiply(data[N-i-1 + N/2, N-j-1+ N/2], v);
		// 	}
		// }

		FourierTransform.FFT2(data, FourierTransform.Direction.Backward); 
		//***********************************************************************
		// set results into Image - ORIGINAL, UNSHIFTED
		int mag = 2;
		for (int i = 0; i < N; i++)
		{
			for (int j = 0; j < N; j++)
			{
				float a = mag*(float)data[i, j].Magnitude;
				Color c = new Color(a, a, a);
				outImage.SetPixel(i, j, c);
			}
		}

		//set FFT Texture to final image
		ifft.Texture = ImageTexture.CreateFromImage(outImage);
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
		ifft_viewport.RenderTargetClearMode = SubViewport.ClearMode.Once;
		//ifft_mask.Texture = black_texture;
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
