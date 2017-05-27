# VoiceCommands

## GeneralVoiceCommands

Voice commands added in this file are triggered anywhere in the app; they do not require the focus.

## VoiceCommandsManager

Triggers an event when a voice commands is recognized. Also contains a list of all the available voice commands.

### How to

To use this component, you will need to add one instance of it in your scene.

To add a voice command to a game object, you will need to get the reference to the VoiceCommandsManager in a script and a dictionary containing the available voice commands and the associated actions available. Here's an example of what it could look like.

```C#
Dictionary<string, Action> voiceCommands = new Dictionary<string, Action>()
    {
        { "Test voice command", CallbackMethod }
    };

var voiceCommandsManager = FindObjectOfType<VoiceCommandsManager>();
```

Then, you can use a function like the following to bind your dictionary to the voice commands manager.

```C#
private void OnSpeechKeywordRecognized(SpeechKeywordRecognizedEventData eventData)
{
    Action action;
    if (voiceCommands.TryGetValue(eventData.RecognizedText.ToLower(), out action))
    {
        action.Invoke();
    }
}
```

Finally, you need to register your method to the voice command manager OnSpeechKeywordRecognized event.

```C#
voiceCommandsManager.SpeechKeywordRecognized += OnSpeechKeywordRecognized;
```

You could also use the voice commands manager with the GazeObserver to execute the voice command only if the object is being gazed at.

```C#
private void OnSpeechKeywordRecognized(SpeechKeywordRecognizedEventData eventData)
{
    if (gazeObserver != null && gazeObserver.IsGazed)
    {
        Action action;
        if (voiceCommands.TryGetValue(eventData.RecognizedText.ToLower(), out action))
        {
            action.Invoke();
        }
    }
}
```
