# Tooltips
Can be attached to an object to prompt contextual information on gaze.

## How to

### Default tooltip
To use the default tooltip, you can follow these simple steps :

1. Add a `GazeObserver` component to the object that will trigger the tooltip (when gazing on this object, the tooltip will appear).
2. Drag the DefaultTooltip prefab onto the trigger object.
3. Edit the script settings :
    * Show Delay : the user has to gaze at the parent for at least `ShowDelay` seconds for the tooltip to show
    * Hide Delay : the tooltip will disappear when the user gazes away for at least `HideDelay` seconds

### Dynamically resized tooltip
To use the dynamically resized tooltip, you can follow these simple steps :

1. Add a `GazeObserver` component to the object that will trigger the tooltip (when gazing on this object, the tooltip will appear).
2. Drag the tooltip prefab you want onto the trigger object.
3. Edit the script settings :
    * Text : the text displayed by the tooltip (shown only at runtime)
    * Label : the game object containing the text (should already be set)
    * Text canvas : canvas containing the text (should already be set)
    * Background : object that acts as background for the tooltip
    * Show delay : the user has to gaze at the parent for at least `ShowDelay` seconds for the tooltip to show
    * Hide Delay : the tooltip will disappear when the user gazes away for at least `HideDelay` seconds

Note : the background will be adjusted to the length of the text relative to its original state. This means that beforehand, in the editor, the tooltip should be adjusted exactly how you want it to show at runtime.

![Tooltip script settings](../../../../../images/tooltips/TooltipScriptSettings.png)

## Content

### Scripts

#### Tooltip.cs
This component will show its game object when its parent is being gazed at for a certain amount of time.

#### DynamicTooltip.cs
Child of Tooltip. It resizes the width of the Background object to fit the text width dynamically. The size is relative to the initial state of the object.

### Materials
Materials used in the prefabs and the demo scene.

### Prefabs
Reusable tooltips prefabs.

#### DefaultTooltip.prefab
Can be used to display an object when gazing on a parent object.

#### DynamicTooltip.prefab
Can be used to display an object when gazing on a parent object. The width is dynamically adjusted to the label width, relative to the initial size.

#### DynamicVoiceCommandTooltip.prefab
Can be used to suggest a voice command when gazing on a parent object. The width is dynamically adjusted to the label width, relative to the initial size.

![Tooltips demo scene](../../../../../images/tooltips/TooltipsDemo.png)
