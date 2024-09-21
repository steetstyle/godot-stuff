using Godot;
using System;
using System.Collections.Generic;

namespace Common.Terrains;

public partial class TerrainManager : Node3D
{
	// Lists to hold the dynamically loaded meshes
	private readonly List<PackedScene> _blockMeshes = new();
	private readonly List<PackedScene> _harmBlockMeshes = new();
	private readonly List<PackedScene> _buffBlockMeshes = new();

	// Define the width and distance for block generation
	[Export] public float GenerationDistance = 50f;
	[Export] public int MaxBlocks = 10;

	// Distance behind the player at which blocks will be removed
	[Export] public float RemoveDistance = 100f;

	// Player's reference to determine position
	[Export] public NodePath PlayerPath;
	private Node3D _player;

	private Vector3 _nextBlockPosition = Vector3.Zero;

	// List to keep track of all generated blocks
	private readonly List<Node3D> _activeBlocks = new();

	public override void _Ready()
	{
		// Reference to the player
		_player = GetNode<Node3D>(PlayerPath);
		_nextBlockPosition = Vector3.Zero;

		// Load block scenes from the directories
		LoadBlockScenes("res://Terrains/NormalBlock", _blockMeshes);
		LoadBlockScenes("res://Terrains/HarmBlock", _harmBlockMeshes);
		LoadBlockScenes("res://Terrains/BuffBlock", _buffBlockMeshes);
	}

	public override void _Process(double delta)
	{
		if (_player == null) return;

		// Dynamically generate blocks ahead of the player
		GenerateBlocks();

		// Remove blocks that are behind the player
		RemoveBlocksBehindPlayer();
	}

	private void GenerateBlocks()
	{
		// Check how far the player has moved and generate more blocks if needed
		while (_nextBlockPosition.Z < _player.GlobalTransform.Origin.Z + GenerationDistance)
		{
			// Randomly choose a mesh for the block (standard block or special block)
			var blockScene = GetRandomBlock();
			if (blockScene == null) continue;

			// Create the block
			var blockInstance = (Node3D)blockScene.Instantiate();

			// Assuming MeshInstance3D is a child of this node
			var meshInstance = blockInstance.GetNode<MeshInstance3D>("StaticBody3D/MeshInstance3D");
			if (meshInstance == null) continue;
			
			// Set the position of the block
			blockInstance.Position = _nextBlockPosition;

			// Add to the scene
			AddChild(blockInstance);

			// Keep track of the block
			_activeBlocks.Add(blockInstance);

			var aabb = meshInstance.GetAabb();  // Get the bounding box of the mesh
			var blockWidth = aabb.Size.Z;  // The Y size is the height
					
			// Move to the next position
			_nextBlockPosition.Z += blockWidth;
		}
	}

	private void RemoveBlocksBehindPlayer()
	{
		// Create a new list to store blocks to be removed
		List<Node3D> blocksToRemove = new List<Node3D>();

		foreach (var block in _activeBlocks)
		{
			// If the block is too far behind the player, mark it for removal
			if (block.Position.Z < _player.GlobalTransform.Origin.Z - RemoveDistance)
			{
				blocksToRemove.Add(block);
			}
		}

		// Remove blocks and clean up the list
		foreach (var block in blocksToRemove)
		{
			// Remove block from the scene
			block.QueueFree();

			// Remove block from the active list
			_activeBlocks.Remove(block);
		}
	}

	private PackedScene GetRandomBlock()
	{
		var random = new Random();
		var chance = random.Next(0, 100);

		switch (chance)
		{
			// 10% chance of a harmful block, 10% chance of a buff block, 80% chance of a normal block
			case < 25 when _harmBlockMeshes.Count > 0:
			{
				// Return a random harm block from the harm block list
				var randomHarmIndex = random.Next(0, _harmBlockMeshes.Count);
				return _harmBlockMeshes[randomHarmIndex];
			}
			case >= 25 and < 75 when _buffBlockMeshes.Count > 0:
			{
				// Return a random buff block from the buff block list
				int randomBuffIndex = random.Next(0, _buffBlockMeshes.Count);
				return _buffBlockMeshes[randomBuffIndex];
			}
		}

		if (_blockMeshes.Count <= 0) return null;
		// Return a random normal block from the list
		var randomIndex = random.Next(0, _blockMeshes.Count);
		return _blockMeshes[randomIndex];

	}

	private static void LoadBlockScenes(string directoryPath, List<PackedScene> sceneList)
	{
		var dir = DirAccess.Open(directoryPath);
		if (dir != null)
		{
			dir.ListDirBegin();

			var fileName = dir.GetNext();
			while (fileName != "")
			{
				if (fileName.EndsWith(".tscn"))
				{
					var scenePath = $"{directoryPath}/{fileName}";
					var scene = (PackedScene)ResourceLoader.Load(scenePath);
					if (scene != null)
					{
						sceneList.Add(scene);
					}
				}
				fileName = dir.GetNext();
			}

			dir.ListDirEnd();
		}
		else
		{
			GD.PrintErr($"Failed to open directory: {directoryPath}");
		}
	}
}
