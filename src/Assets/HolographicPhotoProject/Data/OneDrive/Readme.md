# OneDrive
Interactions with OneDrive using the OneDrive C# SDK.

Note that Unity 5.5 only supports .NET 3.5, so it cannot support the OneDrive C# SDK. Therefore, all interactions with the OneDrive SDK should be contained within `#if UNITY_UWP` / `#endif` statements. 

## How to
1. To use the OneDrive SDK within this application, you will need to [register your application](https://dev.onedrive.com/app-registration.htm). 
2. Once your app has been registered, copy the application ID. 
3. Open the configuration manager in HolographicSouvenirProject/src/Assets/HolographicSouvenirProject/Utilities/ConfigurationManager.cs. 
4. Replace the text `INSERT YOUR OWN CLIENT ID HERE` with this application ID.

## Content

### Controllers

#### IItemController.cs
Interface to control items that will be displayed. This includes authentication and retrieval of the items.

#### OneDriveController.cs
Implementation of IItemController for OneDrive items. 

**There are instances of undocumented OneDrive API calls in this file. These are subject to change without any warning.**

#### OneDriveProvider.cs
Fetches content from OneDrive and formats it into the requested content type.

### Models

#### IItemModel.cs
Interface for a item that will be displayed. Contains Id for identification, Name for display name, MemoryStream for full resolution representation, and Thumbnail URL.

#### OneDriveModel.cs
Implementation of IItemModel for OneDrive items. 
