# doTweenClip
 
DoTweenClip is a Unity AnimationClip Custom interpreter, which is designed for - dynamic property & fixblity. 

# Usage: 

## Install
1. Open Test Scene to see the demo
2. Copy & Paste the source folder into your project.

## Play a animation
var tweener =  transform.DoAnimationClipAbsolute(DoTweenClip clip);

This function will do the same thing as Animation.Play("animationClipName")


1. Create a DoTweenClip asset in asset menu.
2. Drag & Drop AnimationClip to DoTweenClip inspector, to import data to DoTweenClip
3. At runtime, load your newDoTweenClip & change some property in DoTweenClipCurve (optional) then play.


# Feature:
1. full control animation: set all object properties by your self.

2. direct animationCurve access (see DoTweenClipCurve.cs)

3. Editor Custom AnimationProperty Rebinder : Change Animation Property Reference at editor time. (see DoTweenClipBinderEditor.cs)

4. Editor DoTweenClip Preview

5. (in progress) CubicBezierCurve support.