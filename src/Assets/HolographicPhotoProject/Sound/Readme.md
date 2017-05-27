# Sound
Plays appropriate sound on events.

## How to
While the audio scripts in this project are tailored to the needs of this app, they can be modified to play different sounds.

### Changing the sounds
The audio in this project is handled by the `AudioManager`, which is a child of the `Managers` object in the main scene. Within the `AudioHandler` component, the audio clips are assigned to be 
connected to events in this project. This project has audio effects for four different events: airtap, souvenir placement, toggle souvenir to active, and toggle souvenir to inactive.

To change the audio file for any of these events:
1. Find the `AudioManager` game object in the main scene under the `Managers`.
2. In the `AudioHandler` component, drag the new audio clip into the slot for the event.
3. If you want to change the default clip (to be played when no other clip has been assigned to an event), you can choose from the clips in the dropdown list.

![AudioHandler clips](../../../../../images/audio/audioclips.png)
## Content

### Audio
These audio files are public domain and are free to use.

#### airtap.wav
Sound file for every airtap. 

#### active.wav
Sound file for activating a souvenir.

#### inactive.wav
Sound file for deactivating a souvenir.

#### placement.wav
Sound file for placing an object.

### Scripts

#### AudioHandler.cs
Allows audio clips to be injected through Unity for easier access in the code. A default audio clip can be identified to play when no audio clip has been set.

#### AudioManager.cs
Allows audio to be played for user interactions in the HoloLens.
 