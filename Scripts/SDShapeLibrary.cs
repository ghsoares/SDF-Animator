using System;
using System.Collections.Generic;
using Godot;

public static class SDShapeLibrary
{
	public static Dictionary<string, SDShape> Library { get; private set; }

	static SDShapeLibrary()
	{
		Library = new Dictionary<string, SDShape>();

		Library.Add("SDCircle", new SDShape
		{
			FunctionName = "SDCircle",
			SourceCode = "return length(p) - radius;",
			Variables = new List<Variable> {
				new Variable("float", "radius", 32f),
			}
		});

		Library.Add("SDBox", new SDShape
		{
			FunctionName = "SDBox",
			SourceCode = "vec2 d = abs(p) - size; return min(max(d.x, d.y), 0.0) + length(max(d, 0.0));",
			Variables = new List<Variable> {
				new Variable("vec2", "size", Vector2.One * 32f)
			}
		});
	}

	public static SDShape GetShape(string name)
	{
		return Library[name];
	}
}
