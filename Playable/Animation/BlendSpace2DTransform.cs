using Godot;
using System;
using System.Collections.Generic;

public class BlendSpace2DTransform
{
    private readonly Godot.Collections.Dictionary<Vector2, Godot.Collections.Dictionary<int, Transform3D>> _poses;
    private Vector2[] _lastNearestPoints;
    private float[] _lastCalculatedWeights;

    public BlendSpace2DTransform(Godot.Collections.Dictionary<Vector2, Godot.Collections.Dictionary<int, Transform3D>> poses)
    {
        _poses = poses;
    }

    public Dictionary<int, Transform3D> GetBlendedPose(Vector2 input)
    {
        var nearestPoints = FindNearestPoints(input);
        var weights = CalculateWeights(input, nearestPoints);

        return BlendTransforms(nearestPoints, weights);
    }

    public Vector2[] FindNearestPoints(Vector2 input)
    {
        var bottomLeft = new Vector2((float)Math.Floor(input.X), (float)Math.Floor(input.Y));
        var bottomRight = bottomLeft + Vector2.Right;
        var topLeft = bottomLeft + Vector2.Up;
        var topRight = topLeft + Vector2.Right;

        _lastNearestPoints = new[] { bottomLeft, bottomRight, topLeft, topRight };

        return _lastNearestPoints;
    }

    public float[] CalculateWeights(Vector2 input, Vector2[] points)
    {
        var weights = new float[points.Length];
        var totalWeight = 0f;

        for (var i = 0; i < points.Length; i++)
        {
            var distance = input.DistanceTo(points[i]);
            if (distance == 0)
                weights[i] = 1f;
            else
                weights[i] = 1f / distance;

            totalWeight += weights[i];
        }

        for (var i = 0; i < weights.Length; i++)
        {
            weights[i] /= totalWeight;
        }

        _lastCalculatedWeights = weights;

        return _lastCalculatedWeights;
    }


    private Dictionary<int, Transform3D> BlendTransforms(Vector2[] points, float[] weights)
    {
        var blendedTransforms = new Dictionary<int, Transform3D>();

        foreach (var boneName in _poses[points[0]].Keys)
        {
            var blendedPosition = Vector3.Zero;
            var blendedRotation = Quaternion.Identity;

            for (var i = 0; i < points.Length; i++)
            {
                if (!_poses.ContainsKey(points[i]) || !_poses[points[i]].ContainsKey(boneName)) continue;
                var transform = _poses[points[i]][boneName];
                var weight = weights[i];
                blendedPosition += transform.Origin * weight;
                blendedRotation = blendedRotation.Slerp(transform.Basis.GetRotationQuaternion(), weight);
            }
            
            var blendedBasis = new Basis(blendedRotation);
            if (blendedPosition == Vector3.Zero) blendedPosition = Vector3.Inf;

            blendedTransforms[boneName] = new Transform3D(blendedBasis, blendedPosition);
        }

        return blendedTransforms;
    }

    public Vector2[] GetLastNearestPoints()
    {
        return _lastNearestPoints;
    }

    public float[] GetLastCalculatedWeights()
    {
        return _lastCalculatedWeights;
    }
}
