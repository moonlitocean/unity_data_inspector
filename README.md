# unity_data_inspector
A data inspector lib for Unity, to inspect/edit any data structure from Editor script.

# Example

* Example0_BasicUse: view most type, also as a testing.
* Example1_CustomType: show custom inspector

Before opening any example project, you should firstly run script mklink.cmd in the Example  folder, which creates a symbolic link to the lib source code. After that, example is a valid Unity3d project.

Click Window/Watch menu will give you the example window.

# Features

## Inpect any data

```Inspector.Inspect(string name, string path, object data)```

## Custom visualizers

```
Inspector.RemoveAllVisualizers()

Inspector.GetVisualizer(Type type)
Inspector.SetVisualizer(Type type, VisualizerBase income)
Inspector.RemoveVisualizer(Type type)

Inspector.GetSpecialVisualizer(SpecialVisualizer type)
Inspector.SetSpecialVisualizer(SpecialVisualizer type, VisualizerBase income)
```

## Use IMarkAttribute to provide different views 

For example, normally an int uses PrimitiveVisualizer. But if some int represents a timestamp, you can define and mark it as UnityTimeStamp for UnixTimestampVisualizer, which displays the time.

```
[UnixTimestamp]int time;
```