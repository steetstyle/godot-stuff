using Godot;
using System;

public partial class ExtendedNode3D : Node
{
	[Signal]
	public delegate void JumpedEventHandler();
	[Signal]
	public delegate void HitEventHandler();
	[Signal]
	public delegate void WasHitEventHandler();
	[Signal]
	public delegate void RolledEventHandler();
	[Signal]
	public delegate void DiedEventHandler();
	[Signal]
	public delegate void BlockedEventHandler();
	[Signal]
	public delegate void RipostedEventHandler();
	[Signal]
	public delegate void SpawnedEventHandler();
}
