using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Common.Playable.Animation;

public partial class MixamoSkeleton :  Skeleton3D, IPlayableSkeletonAccessor
{
	private const string BonePrefix = "mixamorig_";
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
	[Export] public Node SplitBodyAnimatorNode;
	private void RunScriptCall()
	{
		if (SplitBodyAnimatorNode == null) return;
		var splitBodyAnimator = (SplitBodyAnimator)SplitBodyAnimatorNode;

		var player = FindAnimationPlayer(this);
		var skeleton = FindSkeleton(this);
		GD.Print($"Animations found: {player.GetAnimationList().Length}");

		//player = FixRootMotion(player);

		var torsoAnimationPlayer = SaveAnimations(GetTorsoBonesIndexes(skeleton), skeleton, player, SplitBodyAnimator.TorsoAnimationSuffix);
		splitBodyAnimator.TorsoAnimationPlayer.RemoveAnimationLibrary("");
		splitBodyAnimator.TorsoAnimationPlayer.AddAnimationLibrary("",torsoAnimationPlayer.GetAnimationLibrary(""));

		var legAnimationPlayer = SaveAnimations(GetLegBonesIndexes(skeleton), skeleton, player, SplitBodyAnimator.LegAnimationSuffix);
		splitBodyAnimator.LegsAnimationPlayer.RemoveAnimationLibrary("");
		splitBodyAnimator.LegsAnimationPlayer.AddAnimationLibrary("",legAnimationPlayer.GetAnimationLibrary(""));

		//SetAutomaticBlendingTimes();
	}

	private static AnimationPlayer FixRootMotion(AnimationPlayer player)
	{
		foreach (var animationName in player.GetAnimationList())
		{
			if (!RootMotionAnimation(animationName)) continue;
			var animation = player.GetAnimation(animationName);
			
			var hipsPositionTrack = animation.FindTrack($"Character/Skeleton3D:{BonePrefix}Hips", Godot.Animation.TrackType.Position3D);
			var animationPositionTrackCount = animation.TrackGetKeyCount(hipsPositionTrack);
			for (var track = 1; track < animationPositionTrackCount; track++)
			{
				animation.TrackRemoveKey(hipsPositionTrack, track);
			}
			
			var hipsRotationTrack = animation.FindTrack($"Character/Skeleton3D:{BonePrefix}Hips", Godot.Animation.TrackType.Rotation3D);
			for (var track = 1; track < animationPositionTrackCount; track++)
			{
				animation.TrackRemoveKey(hipsRotationTrack, track);
			}
		}

		return player;
	}

	private static bool RootMotionAnimation(string animation)
	{
		return animation.Contains("Idle") || animation.Contains("Walk") || animation.Contains("idle") || animation.Contains("walk");
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

				var boneName = trackPath.Replace("Character/Skeleton3D:", "");
				var boneIndex = skeleton.FindBone(boneName);
				if (boneIndexes.Contains(boneIndex))
				{
					animation.CopyTrack(track, newAnimation);
				}
			}

			if (LoopModeAnimation(animationName))
			{
				// Check if the start and end position of the animation do not match
				//var startEndMismatch = false;
				//for (var track = 0; track < newAnimation.GetTrackCount(); track++)
				//{
				//	var startPos = newAnimation.TrackGetKeyValue(track, 0);  // Get the first key position
				//	var endPos = newAnimation.TrackGetKeyValue(track, newAnimation.TrackGetKeyCount(track) - 1);  // Get the last key position
				//	if (startPos.Equals(endPos)) continue;
				//	startEndMismatch = true;
				//	break; // No need to check further if one track doesn't match
				//}
				newAnimation.SetLoopMode(Godot.Animation.LoopModeEnum.Linear);
			}

			ResourceSaver.Save(newAnimation, $"res://Playable/BasicCharacter/Assets/Animations/{suffix}/{animationName}_{suffix}.res");
			animationLibrary.AddAnimation($"{animationName}_{suffix}", newAnimation);
		}
		ResourceSaver.Save(animationLibrary, $"res://Playable/BasicCharacter/Assets/Animations/{suffix}/animation_library_{suffix}.res");

		var animationPlayer = new AnimationPlayer();
		animationPlayer.AddAnimationLibrary("", animationLibrary);
		return animationPlayer;

	}

	private static bool LoopModeAnimation(string animation)
	{
		return animation.Contains("Idle") || animation.Contains("Walk") || animation.Contains("Run") || animation.Contains("idle") || animation.Contains("walk") || animation.Contains("run");
	}
	
	private static bool SkipAnimation(string animation)
	{
		return animation.Contains("Armature") || animation.Contains("Action") || animation.Contains("Layer") || animation.Contains("Character");
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
	
	private void SetAutomaticBlendingTimes()
	{
		if (SplitBodyAnimatorNode == null) return;
		var splitBodyAnimator = (SplitBodyAnimator)SplitBodyAnimatorNode;

		var player = FindAnimationPlayer(this);
		var animations = player.GetAnimationList();
		

		for (var i = 0; i < animations.Length; i++)
		{
			var fromAnim = animations[i];
			for (var j = i + 1; j < animations.Length; j++)
			{
				var toAnim = animations[j];
				var blendTime = CalculateBlendTime(fromAnim, toAnim, player);

				splitBodyAnimator.TorsoAnimationPlayer.SetBlendTime(fromAnim, toAnim, 0.3);
				splitBodyAnimator.TorsoAnimationPlayer.SetBlendTime(toAnim, fromAnim, 0.3);
				splitBodyAnimator.LegsAnimationPlayer.SetBlendTime(fromAnim, toAnim, 0.3);
				splitBodyAnimator.LegsAnimationPlayer.SetBlendTime(toAnim, fromAnim, 0.3);
			}
		}
	}

	[Export] public float SyncThreshold { get; set; }
	private float CalculateBlendTime(string fromAnim, string toAnim, AnimationPlayer player)
	{
	    var fromAnimation = player.GetAnimation(fromAnim);
	    var toAnimation = player.GetAnimation(toAnim);

	    var fromLength = fromAnimation.Length;
	    var toLength = toAnimation.Length;
	    var lengthDiff = Mathf.Abs(fromLength - toLength);

	    // Compare start and end poses for key bones (e.g., hips and torso)
	    var posesMatch = DoPosesMatch(fromAnimation, toAnimation, "mixamorig_Hips") &&
	                     DoPosesMatch(fromAnimation, toAnimation, "mixamorig_Spine");

	    return Mathf.Max(lengthDiff * SyncThreshold, posesMatch ? 0.3f : 0.6f);
	}

	private bool DoPosesMatch(Godot.Animation fromAnim, Godot.Animation toAnim, string boneName)
	{
	    var fromTrackIdx = fromAnim.FindTrack(boneName, Godot.Animation.TrackType.Position3D);
	    var toTrackIdx = toAnim.FindTrack(boneName, Godot.Animation.TrackType.Position3D);

	    if (fromTrackIdx == -1 || toTrackIdx == -1) return false;

	    // Get the first and last position key for both animations
	    var fromEndPos = fromAnim.TrackGetKeyValue(fromTrackIdx, fromAnim.TrackGetKeyCount(fromTrackIdx) - 1);
	    var toStartPos = toAnim.TrackGetKeyValue(toTrackIdx, 0);

	    // Compare the end of the fromAnim to the start of the toAnim
	    var positionMatch = ArePositionsClose(fromEndPos.AsVector3(), toStartPos.AsVector3());

	    // Perform similar checks for rotations if needed (use Rotation3D tracks)
	    var fromRotationTrackIdx = fromAnim.FindTrack(boneName, Godot.Animation.TrackType.Rotation3D);
	    var toRotationTrackIdx = toAnim.FindTrack(boneName, Godot.Animation.TrackType.Rotation3D);

	    if (fromRotationTrackIdx == -1 || toRotationTrackIdx == -1) return false;

	    var fromEndRot = fromAnim.TrackGetKeyValue(fromRotationTrackIdx, fromAnim.TrackGetKeyCount(fromRotationTrackIdx) - 1);
	    var toStartRot = toAnim.TrackGetKeyValue(toRotationTrackIdx, 0);

	    bool rotationMatch = AreRotationsClose(fromEndRot.AsQuaternion(), toStartRot.AsQuaternion());

	    return positionMatch && rotationMatch;
	}

	private bool ArePositionsClose(Vector3 pos1, Vector3 pos2)
	{
	    return pos1.DistanceTo(pos2) < 0.1f;
	}

	private bool AreRotationsClose(Quaternion rot1, Quaternion rot2)
	{
	    return rot1.IsEqualApprox(rot2);
	}
}
