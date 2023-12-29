extends SubViewportContainer


# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	var pos = get_tree().get_current_scene().get_global_mouse_position()
	$SubViewport/Camera2D.global_position = pos
	pos.x -= 128
	pos.y -= 128
	global_position = pos
