# Perlin Camera Shake
Just a camera shake script that you can attach to a Camera as a component in Unity3D. I guess it can be ideal if you just need camera shake, and would rather not use the full cinemachine package.

It's not written in a crappy way. It doesn't do any polling, doesn't keep lerping indefinitely, and generally only executes what is strictly necessary for the given settings. The latter means for example that if you set only rotational shaking, it won't bother calculating translation; and if you only ask for X axis translation, it won't calculate things for the other axes. Probably doesn't really matter much overall, but still. ;)

I used it with my own event system, instead of setting the `Trauma` property, but I commented out the event-related lines. Feel free to remove them, if you're not gonna use any event system.

**Settings exposed to the Editor:**

![Editor customization options of the Perlin Camera Shake component](https://github.com/baratgabor/PerlinCameraShake/blob/master/PerlinCameraShake_editor.png)
