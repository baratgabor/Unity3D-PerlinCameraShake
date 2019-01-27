# Perlin Camera Shake for Unity3D
Just a camera shake script that you can attach to a Camera as a component in Unity3D. I guess it can be ideal if you just need camera shake, and would rather not use the full cinemachine package.

**It's not written in a crappy way:**
- It doesn't do any polling.
- Doesn't keep lerping indefinitely.
- Generally only executes what is strictly necessary for the given settings, e.g.:
  - If  you set only rotational shaking, it won't bother calculating translation.
  - If you only ask for X axis translation, it won't calculate things for the other axes.
  
Probably doesn't really matter much overall, but still. ;)

**Usage (but it's obvious):**
- Copy the `.cs` file to your `Assets` folder.
- Add the component to your Camera.
- Set the settings in the Inspector pane, and test it by manually adding some trauma.

Obviously, you'll need to add 'trauma' during runtime to initiate the camera shake. You can do this by directly accessing the public `Trauma` property of the component. I used it with my own event system instead, but I commented out the event-related lines. Feel free to remove these if you're not gonna use any event system.

**Settings exposed to the Editor:**

![Editor customization options of the Perlin Camera Shake component](https://github.com/baratgabor/PerlinCameraShake/blob/master/PerlinCameraShake_editor.png)
