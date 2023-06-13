/*

The link.xml file in this folder allows you to build Dialogue System projects
with code stripping. (The Dialogue System's sequencer uses reflection. This 
file tells Unity to include sequencer commands in the build.)

If you're using the Assembly Definition Files provided in the Scripts folder,
use link_asmdef.xml instead of link.xml.

If your game doesn't use networking, you can remove the requirement for 
full network permissions in your builds by deleting these files:

- Plugins/Pixel Crushers/Dialogue System/Scripts/Model-View-Controller/View/Sequencer/Sequencer Commands/SequencerCommandWWW.cs
- Plugins/Pixel Crushers/Dialogue System/Scripts/Utility/LuaNetworkCommands.cs
- Plugins/Pixel Crushers/Dialogue System/Wrappers/Utility Wrappers/LuaNetworkCommands.cs
*/
