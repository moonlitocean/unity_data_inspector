# unity_data_inspector
Inspect/Edit any data from Editor script through reflection.

![feature.jpg](feature.jpg)

# Examples

First run mklink.cmd scripts from each Example* folder, which creates symbolic links to the lib source code. After that, Examples are valid Unity3d projects.

Click Window/Watch menu to open the example window.

# Features

## Inpect data

```c#
private Inspector ins = new Inspector();
ins.Inspect(string name, string path, object data)
```

## Custom visualizers

```c#
class MyVisualizer : VisualizerBase { /**/ }

ins.RemoveAllVisualizers()

ins.GetVisualizer(Type type)
ins.SetVisualizer(Type type, VisualizerBase income)
ins.RemoveVisualizer(Type type)

ins.GetSpecialVisualizer(SpecialVisualizer type)
ins.SetSpecialVisualizer(SpecialVisualizer type, VisualizerBase income)
```
Supports generic type and inheritance.  

## Define IMarkAttribute to provide different views 

To mark some int as timestamp, or mark some string as Locale (that can translates from "skill.101.name" to  "Rocket Punch").

```c#
[UnixTimestamp]int time;
[Locale]string skill_name;
```
