# 3DLife
3D Life in Unity

You just need the GameOfLife Script
Start an empty 3D project with directional light and camera.

Add an empty GameObject and attach the Game Of Life script.
Add a cube, scale it to 0.1 on all axes and make it a prefab
Drag that prefab to the first slot in the script (Cell).

Dimensions can be up to 255 on all three axes .. more is slower
Cell spacing should be about 0.1

The Limit curve allows the rules to change slightly to limit overpopulation.
Set it to fall from 1 to 0.1 on the y axis between 0 and 1 on the x axis.
Block count multiplier determines how this is applied, the number of blocks 
currently visible is divided by this to evaluate the curve.
The neighbour limit multiplier is the maximum numbers of neighbours allowed, 
set to 10 to start, decreasing this makes a sparser population.

If random spawn is swiched on then every x blocks (Spawn Chance) will be 
switched on inside the inner spawn block (dimensions are from the centre)

If it's not switched on then you just get an initial foursquare, which is enough :-)

The wait interval can be altered at runtime with the + and - keypad keys, set to 8 
to begin with.

The camera script allows panning and zooming but is pretty basic.  It needs
two empty gameobjects in the centre of the scene, one a child of the other. 
The child is the up/down gimbal, the parent holds the camera script.  The camera
is then a child of the gimbal, offset along the z axis.
