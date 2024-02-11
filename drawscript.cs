using Godot;
using System;

public partial class drawscript : TextureRect
{
	// Called when the node enters the scene tree for the first time.
	bool can_draw = false;
	ImageTexture PaintTexture;
	ImageTexture EraseTexture;
	ImageTexture BlackTexture; //needed?
	ImageTexture ClearTexture;
	public override void _Ready()
	{
		MouseEntered += OnMouseEntered;
		MouseExited += OnMouseExited;
		PaintTexture = ImageTexture.CreateFromImage(Image.LoadFromFile("res://spot.png"));
		EraseTexture = ImageTexture.CreateFromImage(Image.LoadFromFile("res://spot_dark.png"));
		BlackTexture = ImageTexture.CreateFromImage(Image.LoadFromFile("res://fill.png"));
		ClearTexture = ImageTexture.CreateFromImage(Image.LoadFromFile("res://clear.png"));

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		QueueRedraw();
	}

	public override void _Draw()
	{
		// if (this.Texture != ClearTexture)
		// {
		// 	this.Texture = ClearTexture;
		// }
		Vector2 offset = new Vector2(-64, -64);
		Vector2 pos = GetViewport().GetMousePosition() + offset;
		Vector2 size = new Vector2(128, 128);
		Rect2 rect = new Rect2(pos, size);

		if(can_draw && Input.IsActionPressed("ui_select"))
		{
			DrawTextureRect(PaintTexture, rect , false);
		}

		else if (can_draw && Input.IsActionPressed("ui_cancel"))
		{
			DrawTextureRect(EraseTexture, rect , false);
		}
	}

	public void OnMouseEntered()
	{
		can_draw = true;
	}

	public void OnMouseExited()
	{
		can_draw = false;
	}
}
