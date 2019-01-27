# Perlin Camera Shake component for Unity3D
Just a camera shake `MonoBehaviour` script that you can attach to a `Camera` as a component in Unity3D. I guess it can be ideal if you just need camera shake, and would rather not use the full Cinemachine package.

## Main feature: It's not crappy code
- It doesn't do any polling.
- Doesn't keep lerping indefinitely.
- Generally only executes what is strictly necessary for the given settings, e.g.:
  - If  you set only rotational shaking, it won't bother calculating translation.
  - If you only ask for X axis translation, it won't calculate things for the other axes.
  
Overall it should be performant. Not that it matters a lot probably, but still. ;)

## Prerequisites

- Your `Camera` should be parented to another `GameObject`, and that `GameObject` should be moved, if your game requires a moving camera. But this is how we normally use cameras in Unity.

## Usage
- Copy the `.cs` file to your `Assets` folder.
- Add the component to your Camera.
- Set the settings in the Inspector pane, and test it by manually adding some trauma.

Obviously, you'll need to add 'trauma' during runtime to initiate the camera shake. You can do this by directly accessing the public `Trauma` property of the component. I used it with my own event system instead, but I commented out the event-related lines. Feel free to remove these if you're not gonna use any event system.

## Settings

You can customize the following parameters of the Perlin noise based camera shake in the Inspector:

![Editor customization options of the Perlin Camera Shake component](https://github.com/baratgabor/PerlinCameraShake/blob/master/PerlinCameraShake_editor.png)

## Bugs

Hopefully none. If you happen to find any, let me know, and I'll fix them.

## Copyrights

Do whatever you want. ¯\\\_(ツ)\_/¯ It's just 300 lines of code.
