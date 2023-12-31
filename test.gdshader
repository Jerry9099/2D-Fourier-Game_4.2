shader_type canvas_item;

uniform float phlash_speed : hint_range(0.0, 10.0) = 1;
uniform float spin_speed : hint_range(0.0, 50.0) = 20.0;
uniform float spin_radius : hint_range(0.0, 30.0) = 5.0;
void fragment(){
	//COLOR = vec4(0.2, 0.6, 0.8, 1.0);
	vec4 pixelColor = texture(TEXTURE, UV);
	COLOR = COLOR + vec4(cos(phlash_speed*TIME), cos(phlash_speed*TIME), sin(phlash_speed*TIME), 1.0);
	COLOR = COLOR * pixelColor.a;
	//COLOR = vec4(UV, abs(sin(phlash_speed*TIME)), 1.0);	
}


//void vertex(){
	//VERTEX += spin_radius * vec2(cos(spin_speed*TIME), sin(spin_speed*TIME));
	//VERTEX += vec2(VERTEX.x*cos(5.0*TIME), VERTEX.y*sin(5.0*TIME));
//}
