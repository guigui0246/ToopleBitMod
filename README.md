# ToopleBitMod
Mod Setup for ToppleBit (**WINDOWS ONLY**)

Copy the contents of this folder into your ToppleBit installation folder.
Put your mods in the `Mods` folder.

`[Patch(typeof(SomeClass))]` to patch a class.
`[Patch(typeof(SomeClass), int)]` to patch a class AND give an order for the call of the Awake function.

`Awake` functions patch won't **replace** the original `Awake` functions.
They will be called in addition after **ALL** the original `Awake` functions.
