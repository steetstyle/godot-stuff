using Godot;
using Godot.Collections;

namespace Common.Playable.Animation.BoneModifiers
{
    public partial class BMBlendSpace2D : BoneModifier
    {
        private Dictionary<Vector2, string> _animations;
        
        private double _timeAccumulator;
        private double _averageAnimationLength;
        private double _deltaTime;
        private double _lastProcessingTime;
        private BlendSpace2DTransform _blendSpace2DTransform;
        private Dictionary<string, Godot.Animation> _skeletonAnimations;

        public override void UpdateParameters(Vector2 inputVector)
        {
            LastInputVector = inputVector;
            UpdateTime();
            
            var nearestPoints = _blendSpace2DTransform.GetLastNearestPoints();
            var weight = _blendSpace2DTransform.GetLastCalculatedWeights();
            if (nearestPoints == null)
            {
                nearestPoints = _blendSpace2DTransform.FindNearestPoints(inputVector);
                weight = _blendSpace2DTransform.CalculateWeights(inputVector, nearestPoints);
            }
            CalculateAverageAnimationLength(nearestPoints, weight);
        }

        public void Transition(Dictionary<Vector2, string> animations)
        {
            _animations = animations;

            GenerateBlendSpace2D();

            _timeAccumulator = 0;
            _lastProcessingTime = 0;

            _skeletonAnimations = new Dictionary<string, Godot.Animation>();
        }

        private void GenerateBlendSpace2D()
        {
            var poses = new Dictionary<Vector2, Dictionary<int, Transform3D>>();
            foreach (var (input, animationName) in _animations)
            {
                var animation = Animator.GetAnimation(animationName);

                var transforms = new Dictionary<int, Transform3D>();
                for (var boneIndex = 0; boneIndex < Skeleton.GetBoneCount(); boneIndex++)
                {
                    var originTrack = animation.FindTrack(BoneToTrackName(boneIndex), Godot.Animation.TrackType.Position3D);
                    var basisTrack = animation.FindTrack(BoneToTrackName(boneIndex), Godot.Animation.TrackType.Rotation3D);
                
                    var transform3D = new Transform3D
                    {
                        Origin =  originTrack != -1 ? animation.PositionTrackInterpolate(originTrack, GetAnimationProgress()) : Vector3.Inf,
                        Basis = basisTrack != -1 ? new Basis(animation.RotationTrackInterpolate(basisTrack, GetAnimationProgress()).Normalized()) : Basis.Identity
                    };
                    
                    transforms.Add(boneIndex, transform3D);
                
                }
                poses.Add(input, transforms);
            }
            
            _blendSpace2DTransform = new BlendSpace2DTransform(poses);
        }

        private void CalculateAverageAnimationLength(Vector2[] nearestPoints, float[] weight)
        {
            var totalWeight = 0f;
            _averageAnimationLength = 0;
            foreach (var (posePosition, animationName) in _animations)
            {
                if (!_skeletonAnimations.TryGetValue(animationName, out var animation))
                {
                    animation = Animator.GetAnimation(animationName);
                    _skeletonAnimations.Add(animationName, animation);
                }

                for (var i = 0; i < nearestPoints.Length; i++)
                {
                    if (posePosition != nearestPoints[i]) continue;
                    var currentWeight = weight[i]; 
                    _averageAnimationLength += animation!.Length * currentWeight; 
                    totalWeight += currentWeight;
                    break;
                }
            }

            if (totalWeight > 0)
                _averageAnimationLength /= totalWeight;
        }

        private void UpdateTime()
        {
            var now = Time.GetUnixTimeFromSystem();
            _deltaTime = now - _lastProcessingTime;
            _lastProcessingTime = now;
        }
        
        private double GetAnimationProgress()
        {
            _timeAccumulator += _deltaTime;

            if (_timeAccumulator > _averageAnimationLength)
                _timeAccumulator = Mathf.PosMod(_timeAccumulator, _averageAnimationLength);

            return _timeAccumulator / _averageAnimationLength;
        }
        
        public override Transform3D SuggestBonePose(int boneIndex)
        {
            if (_animations == null || _animations.Count == 0) return new Transform3D();
            var blendedPoses = _blendSpace2DTransform.GetBlendedPose(LastInputVector);
            return blendedPoses[boneIndex];
        }
    }
}
