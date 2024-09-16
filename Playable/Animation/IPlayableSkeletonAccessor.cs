using Godot;

namespace Common.Playable.Animation;

public interface IPlayableSkeletonAccessor
{
	public int[] GetHierarchyIndexes(Skeleton3D skeleton, int rootIdx);
	public int GetHipsIndex(Skeleton3D skeleton);
	public int[] GetTorsoBonesIndexes(Skeleton3D skeleton);
	public int[] GetLegBonesIndexes(Skeleton3D skeleton);
}
