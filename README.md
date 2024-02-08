# Avatar Extensions

Avatar Extensions is the name of a **hybrid SDK Plugin x MelonLoader mod** that allows you to **customize your avatar's stats and masses, and preview the vanilla values, all from within the SDK!** Now that's a fine sewer melon. 

# How To Install
There is a mod (that's this repo), and there's a Unity Package (for the editor, check the releases).

### I want to use custom stats on an avatar I installed!...

#### ...And I am on PC
* Install MelonLoader. [Download it here](https://melonwiki.xyz/#/?id=automated-installation)
	* YOU MUST USE MELONLOADER 0.5.7 - IF YOU DO NOT, YOUR GAME WILL CRASH AND BREAK.
	* Launch the game without any mods installed. It will take longer than usual, but this sets it up for modding.
* Upgrade BONELAB to Patch 3 if you haven't already
	* Right click on the game in Steam, and click properties
	* Click on "Betas" on the side of the menu that opens up.
	* In the dropdown, select public_beta
	* Wait for the update to download.
* Install BoneLib if you haven't already. [Download BoneLib here](https://bonelab.thunderstore.io/package/gnonme/BoneLib/).
* Install this mod [by downloading it from the top most file from the Releases page](https://github.com/EtiTheSpirit/AvatarStatExtender/releases).
* Now, if you download (or make) any avatars that use the Stat Driver, they will load their custom stats!

#### ...And I am on Quest
> ⚠ **NOTE:** This hasn't been tested on Quest and was not made for it. This mod might be very broken, and I have no way to fix it for Quest!

* Install LemonLoader. [Click here for a tutorial on YouTube](https://www.youtube.com/watch?v=Ax6vAd_lGsg)
* Upgrade BONELAB to Patch 3 if you haven't already
	* Go to the Oculus Store (or your game library) and select BONELAB.
	* Scroll down to Additional Details
	* Find Version, and choose the version number that is next to it.
	* Press the Channel button, and choose public_beta
	* The game should update. If it doesn't, try restarting your Quest.
* Install BoneLib if you haven't already. [Download BoneLib here](https://bonelab.thunderstore.io/package/gnonme/BoneLib/).
* Install this mod [by downloading it from the top most file from the Releases page](https://github.com/EtiTheSpirit/AvatarStatExtender/releases).
* Now, if you download (or make) any avatars that use the Stat Driver, they will load their custom stats!

### I want to make / upgrade an avatar to use custom stats!
* BACK UP YOUR UNITY PROJECT. It's not necessary, but it's usually a good idea, especially since this is still in testing phase.
* Install the Unity Editor Package [by downloading it from the top most file from the Releases page](https://github.com/EtiTheSpirit/AvatarStatExtender/releases).
* Open your Avatar project in Unity if you haven't already.
* Drag this .unitypackage file into your Unity application (into your Assets window, at the bottom).
	a. You will be prompted to unpack it and install its contents. Do this.
	b. Wait on Unity to load everything.
* Select your Avatar prefab. In the Inspector, click the "Add Component" button at the bottom and choose "Avatar Stat Driver".
* Customize everything! Everything has an explanation, hover your cursor over options or click the dropdowns under each value for an explanation.

***
***
# I'm a modder, how did you get the vanilla stats?
About a day straight in IDA, reading raw assembly, and translating it by hand to C#.

[Anyway, here's the code](https://github.com/EtiTheSpirit/AvatarStatExtender/blob/0.1.0/AvatarStatExtender/Tools/AvatarStatCalculationExtension.cs).

***
***

## Component: Avatar Stat Driver
* Preview the vanilla stats that the game comes up with for you
* Edit all your avatar's stats and masses individually
  * Each stat can be toggled so that you can pick and choose which ones to override or leave as their vanilla value.
  * For mass, you can use two types of editing
    * Proportional editing: You change your total weight. Everything else scales for you to match it.
    * Individual editing: You choose the weight of each body part. This has a noticable effect on your physics.
* Other bonus stats
  * Jump velocity boost (a bit like the hyper jump mod)
  * Maybe more

## Component: Avatar Extended Audio Container
> ⚠ **NOTE:** This component is not yet complete. While you can set it up, it won't do anything in game!

* Provide an array of sound blocks. Sound blocks contain...
  * *When* that sound should play (both preset events, but also the capability for multiple custom event names via manual string input)
  * *How* that sound should play (choose one randomly out of the list? Play all of them at once? Also supports a string input which will call a delegate within a melon mod. Details on this later as it approaches release)
  * The name of this sound, so that you can query it by name.
  * The list of sounds
  * (TO DO) An AudioSource template, which allows you to declare the physical properties of the sound. There is already a really good default provided for this, and this mostly will have an effect in multiplayer and in rooms with reverb.

### Prototype Warning
This mod is currently a prototype and thus I have not yet released it on mod.io or thunderstore.

Additionally, features aren't final. I might remove stuff. I might add stuff.