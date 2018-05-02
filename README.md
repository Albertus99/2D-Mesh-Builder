2D Mesh Builder
==========
Plugin for unity to easily create 2d shapes by generating meshes.
![view](https://i.imgur.com/KdOoBMp.png)


Usage
-----

Just add **MeshBuilder.cs** script to empty gameobject and press *Quick Start*.


Properties and Inspector
----------

![editor](https://i.imgur.com/ZFWbSMj.png)

- **Render Mode** - Chooses to render as Post Effect or just apply blurred texture to UI material.

- **Kernel Size** - Bigger kernels produces bigger blur, but are more expensive.

- **Interpolation** - Use if you want to create smooth blurring transition.

- **Downsample** - Controls buffer resolution (0 = no downsampling, 1 = half resolution... etc.).

- **Iterations** - More iterations = bigger blur, but comes at perfomance cost.

- **Gamma Correction** - Enables gamma correction to produce correct blur in Gamma Colorspace. Disable this option if you use Linear Colorspace. 

Controls
----------

![editor](https://i.imgur.com/qAVJ77J.png)

- **Alt** - Hold to toggle vertical and horizontal symmetry.

- **Shift** - Hold to turn off outline, handles, axes and grind for clean view.

- **Ctrl** - Hold and click on hande to switch it to "bezier curve" mode or back.

- **Ctrl+Shift** - Hold both to change center of your shape.

