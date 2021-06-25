# Authorable Modifiers
Allows Audica custom maps to work with ingame modifiers.

## Api Controller
This allows you to sync Brightness and Colors to LEDs via API.
In the ML config file [PathToAudica]/UserData/MelonPreferences.cfg are 2 variables you need to set in order to use this.
```csharp
//example values
postToApi = true              //true to enable, false to disable
apiUrl = "192.168.0.10"       //URL to your API. 
```
It will POST information to your API in the following format:
```console
{
  "red": 0
  "green": 0
  "blue": 0
  "bright": 0
}
```
## Disclaimer
Maps using a speed modifier below 100% won't post scores. You can disable those modifiers in ModSettings.

## WARNING
If you have epilepsy, please turn the option "Flashing Lights" off in the in-game menu.
If you easily suffer from motion sickness, please turn the option "Arena Rotation" off in the in-game menu.


