extends SubViewport


# Called when the node enters the scene tree for the first time.
func _ready():
	world_2d = get_tree().root.get_viewport().world_2d


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass
