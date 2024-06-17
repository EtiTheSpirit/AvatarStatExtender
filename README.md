# Avatar Extensions

## HEY: This is the Patch 4 prototype branch! This won't work so easily (and no release exists yet) until my dependencies (BoneLib, FieldInjector) both have their public releases. THIS BRANCH IS NOT EASY TO INSTALL, BUT IT WILL BE SOON.

***

### ⚠ **If you were looking for a way to customize an avatar while you play, go get [Avatar Stats Loader](https://bonelab.thunderstore.io/package/FirEmerald/AvatarStatsLoader/), not this.** This is for including stats with the avatar pallet for download, not customizing them while you play.

***

Avatar Extensions is a **hybrid SDK Plugin x MelonLoader mod** that allows you to **customize your avatar's stats and masses, and preview the vanilla values, all from within the SDK!** It also allows you to create new types of sound effects for reactions to things in the world. Now that's a fine sewer melon. 

**While similar, this is different than the Avatar Stats Loader mod.** This mod allows avatars to **include their custom stats with their released pallet**, as a means of creative or mechanical expression on behalf of the avatar's creator. The creator chooses its stats so it plays and acts like they want it to, you download it and choose it, and it already has its custom values right out of the box.

The SDK and mod are both available on [the GitHub](https://github.com/EtiTheSpirit/AvatarStatExtender).

## 🐞 I found a bug / ⚙ I have an idea!
Let me know [in the Issues tab at the top of the page](https://github.com/EtiTheSpirit/AvatarStatExtender/issues/new/choose) (or click that link).

## 😒 Someone's avatar said it had custom stats, but it's not working!
You need both their avatar *and this mod* (the mod is what tells the game to get rid of the vanilla stats and use these instead). See below for how to install.

# 🍉 How To Install
There is a mod (that's this repo), and there's a Unity Package (for the editor, check the releases). This goes over installing the mod portion.

## ⛽ Dependencies
* [Download BoneLib](https://bonelab.thunderstore.io/package/gnonme/BoneLib/) first. || ⚠ Not released to Patch 4 as of writing (but please check for yourself!)
* [Download FieldInjector](https://bonelab.thunderstore.io/package/WNP78/FieldInjector/) next. || ⚠ Not released to Patch 4 as of writing (but please check for yourself!)

**The mod will fail to load and/or crash if you do not have these installed.** Install them.
***
## I want to use custom stats on an avatar I installed!...

### 🖥 ...And I am on PCVR
* Install MelonLoader. [Download it here](https://melonwiki.xyz/#/?id=automated-installation)
	* YOU MUST USE MELONLOADER 0.6.1
	* Launch the game without any mods installed. It will take longer than usual, but this sets it up for modding.
* Install BoneLib if you haven't already. [Download BoneLib here](https://bonelab.thunderstore.io/package/gnonme/BoneLib/).
* Install FieldInjector if you haven't already. [Download FieldInjector here](https://bonelab.thunderstore.io/package/WNP78/FieldInjector/).
* Install this mod [by downloading it from the top most file from the Releases page](https://github.com/EtiTheSpirit/AvatarStatExtender/releases).
* Now, if you download (or make) any avatars that use the Stat Driver, they will load their custom stats!
***
### ♾ ...And I am on Quest
> ⚠ **NOTE:** This hasn't been tested on Quest and was not made for it. This mod might be very broken, and I have no way to fix it for Quest!

* Install LemonLoader. [Click here for a tutorial on YouTube](https://www.youtube.com/watch?v=Ax6vAd_lGsg)
* Install BoneLib if you haven't already. [Download BoneLib here](https://bonelab.thunderstore.io/package/gnonme/BoneLib/).
* Install FieldInjector if you haven't already. [Download FieldInjector here](https://bonelab.thunderstore.io/package/WNP78/FieldInjector/).
* Install this mod [by downloading it from the top most file from the Releases page](https://github.com/EtiTheSpirit/AvatarStatExtender/releases).
* Now, if you download (or make) any avatars that use the Stat Driver, they will load their custom stats!

***

## I want to make / upgrade *my* avatar project in Unity to use custom stats!
* BACK UP YOUR UNITY PROJECT. It's not necessary, but it's usually a good idea, especially since this is still in testing phase.
* Install the Unity Editor Package [by downloading it from the top most file from the Releases page](https://github.com/EtiTheSpirit/AvatarStatExtender/releases).
* Follow the instructions [on the Documentation website](https://etithespir.it/bonelab/avatarextender).

***
## I'm a modder, how did you get the vanilla stats function?
About a day straight in IDA, reading raw assembly, and translating it by hand to C#. It was not easy.

[Anyway, here's the code](https://github.com/EtiTheSpirit/AvatarStatExtender/blob/master/AvatarStatExtender/Tools/AvatarStatCalculationExtension.cs).
