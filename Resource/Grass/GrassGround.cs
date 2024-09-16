using System.Collections.Generic;
using Godot;

namespace Common.Resource.Grass;

public partial class GrassGround : Node3D
{
    private List<Node3D> players = new();
    private ShaderMaterial grassShaderMaterial;
    private ImageTexture playerPositionsTexture;
    private Image playerPositionsImage;
    
    private const int MaxPlayers = 10;

    public override void _Ready()
    {
        var targetSurface = GetNode<MultiMeshInstance3D>("Ground_MultiMeshInstance3D");
        grassShaderMaterial = (ShaderMaterial)targetSurface.GetMaterialOverride();
        
        if (grassShaderMaterial == null)
        {
            GD.Print("Shader material not found!");
            return;
        }

        // Initialize the texture for player positions
        playerPositionsImage = Image.CreateEmpty(MaxPlayers, 1, false, Image.Format.Rgbf);
        playerPositionsTexture = ImageTexture.CreateFromImage(playerPositionsImage);
        grassShaderMaterial.SetShaderParameter("player_positions_texture", playerPositionsTexture);

        // Add player nodes to list (assuming all players are in a group called "players")
        foreach (var node in GetTree().GetNodesInGroup("players"))
        {
            //if (node is Common.Playable.BasicPlayable playable)
            //    players.Add(playable);
        }
    }

    public override void _Process(double delta)
    {
        UpdatePlayerPositionsTexture();
    }

    private void UpdatePlayerPositionsTexture()
    {
        if (players.Count == 0 || grassShaderMaterial == null)
            return;

        var playerPositions = new Vector3[10];
        for (var i = 0; i < Mathf.Min(players.Count, MaxPlayers); i++)
        {
            var playerPosition = players[i].GlobalTransform.Origin;
            playerPositions[i] = playerPosition - new Vector3(0, 0.4f, 0 );
        }

        for (var i = players.Count; i < MaxPlayers; i++)
            playerPositions[i] = Vector3.Zero;

        grassShaderMaterial.SetShaderParameter($"player_positions", playerPositions);
        grassShaderMaterial.SetShaderParameter($"player_count", players.Count);

    }
}