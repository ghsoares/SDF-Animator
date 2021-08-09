using System;
using System.Collections.Generic;
using Godot;

public struct Variable
{
	public string type;
	public string name;
	public string hint;
	public object defaultValue;

	public Variable(string type, string name, object defaultValue, string hint = "")
	{
		this.type = type;
		this.name = name;
		this.hint = hint;
		this.defaultValue = defaultValue;
	}

	public string GetDefault()
	{
		string df = "";
		switch (this.type)
		{
			case "bool":
				{
					df = ((bool)defaultValue).ToString();
					break;
				}
			case "int":
				{
					df = $" = {defaultValue}";
					break;
				}
			case "float":
				{
					df = ((float)defaultValue).ToString().Replace(",", ".");
					break;
				}
			case "vec2":
				{
					Vector2 vc = (Vector2)defaultValue;
					string x = vc.x.ToString().Replace(",", ".");
					string y = vc.y.ToString().Replace(",", ".");
					df = $"vec2({x}, {y})";
					break;
				}
			case "vec3":
				{
					Vector3 vc = (Vector3)defaultValue;
					string x = vc.x.ToString().Replace(",", ".");
					string y = vc.y.ToString().Replace(",", ".");
					string z = vc.z.ToString().Replace(",", ".");
					df = $"vec3({x}, {y}, {z})";
					break;
				}
			case "vec4":
				{
					Color vc = (Color)defaultValue;
					string x = vc.r.ToString().Replace(",", ".");
					string y = vc.g.ToString().Replace(",", ".");
					string z = vc.b.ToString().Replace(",", ".");
					string w = vc.a.ToString().Replace(",", ".");
					df = $"vec4({x}, {y}, {z}, {w})";
					break;
				}
		}

		return df;
	}
}

public class SDShape : Node
{
	private string _FunctionName;
	private string _SourceCode;
	private List<Variable> _Variables;
	
	public string FunctionName { get => _FunctionName; set => _FunctionName = value; }
	public string SourceCode { get => _SourceCode; set => _SourceCode = value; }
	public List<Variable> Variables { get => _Variables; set => _Variables = value; }
}
