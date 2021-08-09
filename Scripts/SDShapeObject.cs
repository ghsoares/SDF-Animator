using System;
using Godot;

public class SDShapeObject : Node
{
	private string _FunctionName;
	private SDShape _Shape;
	private object[] _Variables;
	private string _ShaderID;
	private int _BlendMode = 0;
	private float _BlendWeight = 0f;
	private string _PositionTransformationFunction = "";

	public string FunctionName { get => _FunctionName; }
	public SDShape Shape { get => _Shape; }
	public object[] Variables { get => _Variables; }
	public string ShaderID { get => _ShaderID; set => _ShaderID = value; }
	public int BlendMode { get => _BlendMode; set => _BlendMode = value; }
	public float BlendWeight { get => _BlendWeight; set => _BlendWeight = value; }
	public string PositionTransformationFunction { get => _PositionTransformationFunction; set => _PositionTransformationFunction = value; }

	public SDShapeObject Init(string funcName)
	{
		this._FunctionName = funcName;
		this._Shape = SDShapeLibrary.GetShape(funcName);
		this._Variables = new object[this._Shape.Variables.Count];

		return this;
	}

	public void SetVar(int idx, object value)
	{
		this._Variables[idx] = value;
	}
}
