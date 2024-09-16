using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Common.Playable.Animation;

[Tool]
public partial class MixamoSkeleton :  Skeleton3D, IPlayableSkeletonAccessor
{
	private const string BonePrefix = "mixamorig5_";
	public int[] GetHierarchyIndexes(Skeleton3D skeleton, int rootIdx)
	{
		var indexes = new List<int>();
		var childBones = skeleton.GetBoneChildren(rootIdx);
		foreach (var child in childBones)
		{
			indexes.AddRange(GetHierarchyIndexes(skeleton, child));
		}
		indexes.Add(rootIdx);
		indexes.Sort();
		return indexes.ToArray();
	}

	public int[] GetTorsoBonesIndexes(Skeleton3D skeleton)
	{
		return GetHierarchyIndexes(skeleton, FindBone($"{BonePrefix}Spine"));
	}

	public int GetHipsIndex(Skeleton3D skeleton)
	{
		return skeleton.FindBone($"{BonePrefix}Hips");
	}

	public int[] GetLegBonesIndexes(Skeleton3D skeleton)
	{
		var rightLegIndexes = GetHierarchyIndexes(skeleton, FindBone($"{BonePrefix}RightUpLeg"));
		var leftLegIndexes = GetHierarchyIndexes(skeleton, FindBone($"{BonePrefix}LeftUpLeg"));
		var combined = new int[1 + rightLegIndexes.Length + leftLegIndexes.Length];
		combined[0] = GetHipsIndex(skeleton);
		var counter = 1;
		foreach (var rightLegIndex in rightLegIndexes)
		{
			combined[counter] = rightLegIndex;
			counter++;
		}
		foreach (var leftLegIndex in leftLegIndexes)
		{
			combined[counter] = leftLegIndex;
			counter++;
		}

		return combined;
	}

	public override void _Ready()
	{
		RunScriptCall();
	}

	[Export] public AnimationLibrary AnimationLibrary;
	private void RunScriptCall()
	{
		var splitBodyAnimator = GetNode<SplitBodyAnimator>("../../../../Model/SplitBodyAnimator");
		var player = FindAnimationPlayer(this);
		var skeleton = FindSkeleton(this);
		GD.Print($"Animations found: {player.GetAnimationList().Length}");

		player = FixRootMotion(player);

		var torsoAnimationPlayer = SaveAnimations(GetTorsoBonesIndexes(skeleton), skeleton, player, SplitBodyAnimator.TorsoAnimationSuffix);
		splitBodyAnimator.TorsoAnimationPlayer.RemoveAnimationLibrary("");
		splitBodyAnimator.TorsoAnimationPlayer.AddAnimationLibrary("",torsoAnimationPlayer.GetAnimationLibrary(""));

		var legAnimationPlayer = SaveAnimations(GetLegBonesIndexes(skeleton), skeleton, player, SplitBodyAnimator.LegAnimationSuffix);
		splitBodyAnimator.LegsAnimationPlayer.RemoveAnimationLibrary("");
		splitBodyAnimator.LegsAnimationPlayer.AddAnimationLibrary("",legAnimationPlayer.GetAnimationLibrary(""));
	}

	private static AnimationPlayer FixRootMotion(AnimationPlayer player)
	{
		foreach (var animationName in player.GetAnimationList())
		{
			if (!RootMotionAnimation(animationName)) continue;
			var animation = player.GetAnimation(animationName);
			var hipsPositionTrack = animation.FindTrack($"Armature/Skeleton3D:{BonePrefix}Hips", Godot.Animation.TrackType.Position3D);
			var firstTrackPosition = animation.TrackGetKeyValue(hipsPositionTrack, 0).AsVector3();
			var animationTrackCount = animation.TrackGetKeyCount(hipsPositionTrack);
			animation.TrackSetKeyValue(hipsPositionTrack, animationTrackCount - 1, firstTrackPosition);
			for (var track = 0; track < animationTrackCount; track++)
			{
				var trackPosition = animation.TrackGetKeyValue(hipsPositionTrack, track);
				var positionWithoutZ = trackPosition.AsVector3();
				positionWithoutZ.Z = firstTrackPosition.Z;
				animation.TrackSetKeyValue(hipsPositionTrack, track, positionWithoutZ);
			}
		}

		return player;
	}

	private static bool RootMotionAnimation(string animation)
	{
		return animation.Contains("Idle") || animation.Contains("Walk");
	}

	private static AnimationPlayer SaveAnimations(int[] boneIndexes, Skeleton3D skeleton, AnimationPlayer player, string suffix)
	{
		var animationLibrary = new AnimationLibrary();
		animationLibrary.ResourcePath = "";
		
		foreach (var animationName in player.GetAnimationList())
		{
			if(SkipAnimation(animationName)) continue;

			var animation = player.GetAnimation(animationName);
			var newAnimation = new Godot.Animation();
			newAnimation.Length = animation.Length;

			for (var track = 0; track < animation.GetTrackCount(); track++)
			{
				var trackPath = animation.TrackGetPath(track).ToString();
				if(SkipTrackPath(trackPath)) continue;

				var boneName = trackPath.Replace("Armature/Skeleton3D:", "");
				var boneIndex = skeleton.FindBone(boneName);
				if (boneIndexes.Contains(boneIndex))
				{
					animation.CopyTrack(track, newAnimation);
				}
			}

			if (LoopModeAnimation(animationName))
			{
				// Check if the start and end position of the animation do not match
				var startEndMismatch = false;
				for (var track = 0; track < newAnimation.GetTrackCount(); track++)
				{
					var startPos = newAnimation.TrackGetKeyValue(track, 0);  // Get the first key position
					var endPos = newAnimation.TrackGetKeyValue(track, newAnimation.TrackGetKeyCount(track) - 1);  // Get the last key position

					if (startPos.Equals(endPos)) continue;
					startEndMismatch = true;
					break; // No need to check further if one track doesn't match
				}

				// If start and end positions don't match, rewind (set loop to non-linear)
				//newAnimation.SetLoopMode(startEndMismatch
				//	? Godot.Animation.LoopModeEnum.Pingpong
				//	// If start and end positions match, set to linear loop mode
				//	: Godot.Animation.LoopModeEnum.Linear);
				newAnimation.SetLoopMode(Godot.Animation.LoopModeEnum.Linear);
			}

			ResourceSaver.Save(newAnimation, $"res://Playable/FootballPlayer/Assets/Processed/Animations/{suffix}/{animationName}_{suffix}.res");
			animationLibrary.AddAnimation($"{animationName}_{suffix}", newAnimation);
		}
		ResourceSaver.Save(animationLibrary, $"res://Playable/FootballPlayer/Assets/Processed/Animations/{suffix}/animation_library_{suffix}.res");

		var animationPlayer = new AnimationPlayer();
		animationPlayer.AddAnimationLibrary("", animationLibrary);
		return animationPlayer;

	}

	private static bool LoopModeAnimation(string animation)
	{
		return animation.Contains("Idle") || animation.Contains("Walk") || animation.Contains("Run");
	}
	
	private static bool SkipAnimation(string animation)
	{
		return animation.Contains("Armature") || animation.Contains("Action") || animation.Contains("Layer");
	}
	
	private static bool SkipTrackPath(string animation)
	{
		return animation.Contains("Ctrl_") || animation.Contains("_IK_") || animation.Contains("_FK_");
	}

	private static AnimationPlayer FindAnimationPlayer(MixamoSkeleton skeleton)
	{
		return skeleton.GetNode<AnimationPlayer>("../../AnimationPlayer");
	}
	
	private static Skeleton3D FindSkeleton(MixamoSkeleton skeleton)
	{
		return skeleton;
	}
}
