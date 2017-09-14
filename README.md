# unity_data_inspector
Inspect/Edit any data from Editor script through reflection. Highly extensible.

![feature.jpg](feature.jpg)

# Examples

First run mklink.cmd scripts from each Example* folder, which creates symbolic links to the lib source code. After that, Examples are valid Unity3d projects.

Click Window/Watch menu to open the example window.

# Features

## Inpect data

```c#
private Inspector ins = new Inspector();

// In OnGUI
ins.Inspect("My Data", "My Data", mydata); // shows content of mydata
ins.Inspect("My Class", "My Class", typeof(MyClass)); // shows static members of MyClass
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
Supports generic type and inheritance. All predefined visualizer can be replaced.

To further customization, you can define attribute over fields, to, e.g. mark some int as timestamp, or mark some string as Locale (that can translates from "skill.101.name" to  "Rocket Punch").

```c#
[UnixTimestamp]int time;
[Locale]string skill_name;
```

> Note: customization must take place before any call to Inspect(), because there is a internal type cache that is costly to rebuild.

### Match Order

* IMark attribute on field
* SpecialVisualizer.PrimitiveAndNull
* SpecialVisualizer.Enum
* Registered type that equals exactly
* Registered type that is the first exact ancestor type (e.g: IList<int>) or generice base type (eg: IList<>)
	* For the same ancestor, exact type first.
* Registered interface of type
  * If multiply interfaces match, the result is unspecified, currently depends on the sequence of GetInterfaces()
* SpecialVisualizer.Composite
