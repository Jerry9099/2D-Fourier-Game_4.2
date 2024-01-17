using Godot;
using System;

public partial class drawscript : TextureRect
{
	// Called when the node enters the scene tree for the first time.
	bool can_draw = false;
	ImageTexture PaintTexture;
	public override void _Ready()
	{
		MouseEntered += OnMouseEntered;
		MouseExited += OnMouseExited;
		PaintTexture = ImageTexture.CreateFromImage(Image.LoadFromFile("res://icon.svg"));

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		QueueRedraw();
	}

	public override void _Draw()
	{
		if(can_draw && Input.IsActionPressed("ui_select"))
		{
			Vector2 pos = GetViewport().GetMousePosition();
			Vector2 size = new Vector2(64, 64);
			Rect2 rect = new Rect2(pos, size);
			DrawTextureRect(PaintTexture, rect , false);
			GD.Print("Paint: " + GetViewport().GetMousePosition());
		}
	}

	public void OnMouseEntered()
	{
		can_draw = true;
		GD.Print("can_draw: " + can_draw);
	}

	public void OnMouseExited()
	{
		can_draw = false;
		GD.Print("can_draw: " + can_draw);
	}
}
