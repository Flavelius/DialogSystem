## Simple Dialogs
### About
A Unity extension, that allows to create branching dialogs through a visual editor that support requirements and actions. Localization is done per dialog.
A bit of scripting is required to implement custom requirements and actions.
A single Dialog is constructed in this format:

* [Title]  
* [Text]  
 * *Choice 1*  
 * *Choice x*

#### Getting Started

1. Create a dialog collection via Right-clicking in the Project panel or choosing `Assets` from the menu, then `Create` -> `Dialog Collection`.
2. Click `Edit` from the inspector of the newly created asset.
3. Add Conversations (edit dialogs by clicking them) in the editor window.
4. Add the `ConversationEngine` Component to an *Npc* for example and drag the asset to its *Dialogs* field.
5. Implement the interfaces IDialogRelevant[Player/Npc/World] for each part. (world can also be left out if not needed)

Starting a conversation is done via:

```csharp
var dialog = referenceToConversationEngine.GetAvailableTopics(npc, player, worldContext/null, language);
```
If multiple Topics are available, this will return a *Conversation* with its type set to *TopicList* (topics as *Answers*) , else it's returned like a dialog (*Title*/*Text*/*Answers*) and the type is set to *SingleDialog*.

Responding (selecting a topic or an answer) is done via:

```csharp
var newDialog = referenceToConversationEngine.Answer(npc, player, worldContext/null, dialog, answer, language)
```
where *dialog* is the reference to the previous and answer is a selection from its list.

#### Extending

To support more languages add the corresponding entry in the `DialogLanguage` enum (make sure to not shuffle the order, or existing dialogs will have incorrect labels for their strings).

To implement more requirements, inherit from `DialogRequirement` in the `DialogSystem.Requirements` namespace.
For actions, inherit from `DialogOptionAction` inside `DialogSystem.Actions`.  
By creating new classes this way, use the ReadableName attribute on them to specify a distinct (short) name which will show up in the editor interface.
Editable variables in them should be public or marked with [SerializeField] **OR** have DrawComplexGui overridden to display the fields in the editor (in the second case return true from the overridden function).

#### Notes
A test scene can be found in the **DialogSystem/Test** folder.  
The IDIalogRelevantPlayer interface already has some sample methods declared for the test scene.
For a clean project, remove the **Test** folder with its content and these methods from the player interface.
