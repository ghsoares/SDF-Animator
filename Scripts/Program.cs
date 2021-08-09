using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Program : Control
{
	private Shader _Shader;
	private ShaderMaterial _ShaderMaterial;
	private string _ShaderTemplate;
	private ColorRect _Canvas;
	private Node _SceneRoot;
	private List<SDShapeObject> _Objects;

	public Program()
	{
		_Objects = new List<SDShapeObject>();
	}

	public override void _EnterTree()
	{
		_Canvas = GetNode<ColorRect>("Canvas");
		_SceneRoot = GetNode<Node>("Scene");

		var circle = new SDShapeObject().Init("SDCircle");
		var circle2 = new SDShapeObject().Init("SDCircle");
		var circle3 = new SDShapeObject().Init("SDCircle");
		var circle4 = new SDShapeObject().Init("SDCircle");
        circle4.PositionTransformationFunction = "p + vec2(64.0, 0.0)";
		AddShape(_SceneRoot, circle);
		AddShape(_SceneRoot, circle2);
		AddShape(circle2, circle3);
		AddShape(circle2, circle4);

		_Shader = GD.Load<Shader>("res://Shaders/Template.gdshader");
		_ShaderTemplate = _Shader.Code;

		_ShaderMaterial = new ShaderMaterial();
		_ShaderMaterial.Shader = _Shader;

		_Canvas.Material = _ShaderMaterial;

		CompileShader();
		UpdateScene();
	}

	public void AddShape(Node parent, SDShapeObject shape)
	{
		parent.AddChild(shape);
		_Objects.Add(shape);
	}

	private void UpdateScene()
	{
		foreach (var shape in _Objects)
		{
			for (int i = 0; i < shape.Variables.Length; i++)
			{
				Variable v = shape.Shape.Variables[i];
				string varName = $"{shape.ShaderID}_VAR_{v.name}";
				_ShaderMaterial.SetShaderParam(varName, shape.Variables[i]);
			}
		}
	}

	private void CompileShader()
	{
		InitTree(_SceneRoot);
		string code = _ShaderTemplate;

		code = code.Replace("//{uniforms}", CompileUniforms());

		code = code.Replace("//{posTrFunctions}", CompilePosTrFunctions());

		code = code.Replace("//{sdfFunctions}", CompileFunctions());

		code = code.Replace("//{scene}", CompileScene());

		string sceneBlend = CompileSceneBlend(_SceneRoot);
		sceneBlend = $"dst = {sceneBlend};";
		code = code.Replace("//{sceneBlend}", sceneBlend);

        System.Console.WriteLine(code);

		_Shader.Code = code;
	}

	private void InitTree(Node root)
	{
		foreach (Node node in root.GetChildren())
		{
			if (node is SDShapeObject)
			{
				SDShapeObject shape = (SDShapeObject)node;
				shape.ShaderID = $"SHAPE_{shape.FunctionName}_ID{shape.GetInstanceId()}";
			}
			InitTree(node);
		}
	}

	private string CompileUniforms()
	{
		List<string> uniforms = new List<string>();

		foreach (var shape in _Objects)
		{
			foreach (var v in shape.Shape.Variables)
			{
				string h = v.hint != "" ? $": hint_{v.hint}" : "";
				string df = v.GetDefault();
				df = df != "" ? $" = {df}" : df;

				uniforms.Add($"uniform {v.type} {shape.ShaderID}_VAR_{v.name}{h}{df};");
			}
			uniforms.Add($"uniform int {shape.ShaderID}_VAR_blendMode = 0;");
			uniforms.Add($"uniform float {shape.ShaderID}_VAR_blendWeight = 0f;");
		}

		return string.Join("\n", uniforms);
	}

	private string CompilePosTrFunctions()
	{
		List<string> functions = new List<string>();

		foreach (var shape in _Objects)
		{
            if (shape.PositionTransformationFunction != "") {
                string funcName = $"{shape.ShaderID}_PosTr";
                string func = $"vec2 {funcName}(vec2 p)" + "{";
                func += $"return {shape.PositionTransformationFunction};";
                func += "}";
                functions.Add(func);
            }
		}

		return string.Join("\n", functions);
	}

	private string CompileFunctions()
	{
		List<string> functions = new List<string>();

		foreach (var shape in SDShapeLibrary.Library.Values)
		{
			List<string> vars = new List<string>();
			foreach (var v in shape.Variables)
			{
				vars.Add($"{v.type} {v.name}");
			}
			string func =
			$"float {shape.FunctionName}(vec2 p, ";
			func += string.Join(", ", vars) + ") {";
			func += shape.SourceCode;
			func += "}";

			functions.Add(func);
		}

		return string.Join("\n", functions);
	}

	private string CompileScene()
	{
		List<string> functionCalls = new List<string>();

		foreach (var shape in _Objects)
		{
			List<string> args = new List<string>();
            if (shape.PositionTransformationFunction == "") {
                args.Add("p");
            } else {
                args.Add($"{shape.ShaderID}_PosTr(p)");
            }
			//args.Add(shape.PositionTransformationFunction == "" ? "p" :);
			foreach (var v in shape.Shape.Variables)
			{
				string varName = $"{shape.ShaderID}_VAR_{v.name}";
				args.Add($"{varName}");
			}
			string arg = string.Join(", ", args);
			functionCalls.Add($"float {shape.ShaderID}_DIST = {shape.FunctionName}({arg});");
		}

		return string.Join("\n", functionCalls);
	}

	private string CompileSceneBlend(Node root)
	{
		string b = "";

		string blendMode = "0";
		string blendWeight = "0.0";

		if (root is SDShapeObject)
		{
			SDShapeObject thisShape = (SDShapeObject)root;
			b = $"{thisShape.ShaderID}_DIST";

			blendMode = $"{thisShape.ShaderID}_VAR_blendMode";
			blendWeight = $"{thisShape.ShaderID}_VAR_blendWeight";
		}

		int numChildren = root.GetChildCount();

		if (numChildren == 0)
		{
			return b;
		}
		else if (numChildren == 1)
		{
			Node child = root.GetChild(0);

			string dstId = CompileSceneBlend(child);
			b = $"{dstId}";
		}
		else if (numChildren == 2)
		{
			Node child1 = root.GetChild(0);
			Node child2 = root.GetChild(1);

			string dstId1 = CompileSceneBlend(child1);
			string dstId2 = CompileSceneBlend(child2);
			b = $"blend({dstId1}, {dstId2}, {blendWeight}, {blendMode})";
		}
		else
		{
			int id = 0;
			foreach (Node child in root.GetChildren())
			{
				if (child is SDShapeObject)
				{
					SDShapeObject otherShape = (SDShapeObject)child;
					string dstId = CompileSceneBlend(otherShape);
					if (id == 0)
					{
						b = $"blend({dstId}";
					}
					else if (id == 1)
					{
						b = $"{b}, {dstId}, {blendWeight}, {blendMode})";
					}
					else
					{
						b = $"blend({b}, {dstId}, {blendWeight}, {blendMode})";
					}
				}
				id++;
			}
		}

		return b;
	}
}
