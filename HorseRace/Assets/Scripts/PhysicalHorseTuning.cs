using UnityEngine;

namespace HorseRace
{
    internal static class PhysicalHorseTuning
    {
        // 跑道终点在物理世界里的 X 距离，越大比赛越长。
        public const float FinishDistance = 15.5f;

        // 隐藏物理世界里每条赛道之间的 Y 间隔，避免不同赛道的马互相碰撞。
        public const float PhysicsLaneGap = 7f;

        // 所有马部件使用的重力缩放。调大更沉，调小更飘。
        public const float GravityScale = 0.7f;

        // 关节角度方向：当前观察到 Unity 关节负角度是“腿向前摆”。
        public const int ForwardSwingDirection = -1;

        // 扭矩方向：当前观察到正扭矩会让 jointAngle 变小，也就是腿向前摆。
        // 它和 ForwardSwingDirection 分开配置，避免把扭矩符号误当成角度符号。
        public const int ForwardTorqueDirection = 1;

        // 地面刚体尺寸：终点之外额外延伸多少，避免马快到终点时踩空。
        public const float GroundExtraLength = 12f;

        // 地面厚度。
        public const float GroundHeight = 0.24f;

        // 地面相对赛道基准线向下的偏移。
        public const float GroundYOffset = -0.12f;

        // 身体材质摩擦。身体碰到地面时使用，过高会容易卡住。
        public const float BodyFriction = 0.15f;

        // Rigidbody2D 默认线性阻力。
        public const float DefaultDrag = 0.0f;

        // Rigidbody2D 默认角阻力。
        public const float DefaultAngularDrag = 0.0f;

        // 身体线性阻力。
        public const float BodyDrag = 0.0f;

        // 腿部线性阻力。
        public const float LegDrag = 0.0f;

        // 低于这个质量的部件会被抬到该质量，防止小部件太轻。
        public const float MinimumBodyPartMass = 0.05f;

        // 腿部空闲时回到中立角的弹簧强度。
        public const float IdleLegSpring = 3.2f;

        // 腿部空闲时回到中立角的阻尼。
        public const float IdleLegDamping = 0.18f;

        // 腿部空闲回正的最大扭矩。
        public const float IdleLegMaxTorque = 10f;


        // 判断腿是否到达当前摆动方向限位时使用的角度容差。
        public const float LegLimitTurnTolerance = 1.5f;
        // 所有物理材质的弹性。少量弹性会让脚踩地时有一点反弹。
        public const float MaterialBounciness = 0.48f;

        // 地面刚体密度。静态刚体质量不参与运动，保留为统一参数。
        public const float GroundDensity = 1f;

        // 身体离地的额外高度，防止开局身体直接压进地面。
        public const float BodyGroundClearance = 0.12f;



        // 尾巴基础长度乘数。
        public const float TailBaseLength = 0.58f;

        // 尾巴基础厚度乘数。
        public const float TailBaseThickness = 0.12f;

        // 脖子连接身体的位置：身体长度方向偏移。
        public const float NeckBodyAnchorX = 0.50f;

        // 脖子连接身体的位置：身体高度方向偏移。
        public const float NeckBodyAnchorY = 0.16f;

        // 脖子靠身体一端的本地 X 锚点。
        public const float NeckLocalBodyAnchorX = -0.48f;

        // 脖子靠头一端的本地 X 锚点。
        public const float NeckLocalHeadAnchorX = 0.48f;

        // 头部连接脖子的位置：头部本地 X 锚点。
        public const float HeadNeckAnchorX = -0.46f;

        // 尾巴出生位置：身体长度方向偏移。
        public const float TailBodyXOffset = 0.56f;

        // 尾巴出生位置：尾巴自身长度方向偏移。
        public const float TailSelfXOffset = 0.42f;

        // 尾巴出生位置：身体高度方向偏移。
        public const float TailBodyYOffset = 0.10f;

        // 腿出生位置：前/后腿相对身体中心的水平偏移。
        public const float LegBodyXOffset = 0.28f;

        // 腿出生位置：身体高度方向偏移，-0.5 表示身体底部。
        public const float LegBodyYOffset = -0.5f;

        // 腿出生位置：腿自身高度方向偏移，-0.5 表示腿中心在挂点下方半条腿。
        public const float LegSelfYOffset = -0.5f;

        // 脖子关节最小角度。
        public const float NeckJointMin = -55f;

        // 脖子关节最大角度。
        public const float NeckJointMax = 85f;

        // 头关节最小角度。
        public const float HeadJointMin = -55f;

        // 头关节最大角度。
        public const float HeadJointMax = 55f;

        // 尾巴关节在尾巴本地坐标的 X 锚点。
        public const float TailLocalAnchorX = 0.48f;

        // 尾巴关节在身体本地坐标的 X 锚点。
        public const float TailBodyAnchorX = -0.50f;

        // 尾巴关节在身体本地坐标的 Y 锚点。
        public const float TailBodyAnchorY = 0.05f;

        // 尾巴关节最小角度。
        public const float TailJointMin = -85f;

        // 尾巴关节最大角度。
        public const float TailJointMax = 85f;

        // 腿关节在腿本地坐标的 Y 锚点，接近 0.5 表示挂在腿顶端。
        public const float LegLocalAnchorY = 0.48f;

        // 腿关节在身体本地坐标的 Y 锚点。
        public const float LegBodyAnchorY = -0.46f;

        // 每条腿随机拥有膝关节的概率。
        public const float KneeJointChance = 0.45f;

        // 膝关节最小角度。不允许任何反向余量，膝盖只能向后弯曲。
        public const float KneeJointMin = 0f;

        // 膝关节最大角度。当前观察到正角度是向后弯曲。
        public const float KneeJointMax = 78f;


        // 身体长度范围。
        public static readonly FloatRange BodyLength = new FloatRange(1.5f, 4f);

        // 身体高度范围。
        public static readonly FloatRange BodyHeight = new FloatRange(0.8f, 1.8f);

        // 腿长范围。
        public static readonly FloatRange LegLength = new FloatRange(0.78f, 1.34f);

        // 腿粗范围。越粗越重，也更容易和地面产生明显碰撞。
        public static readonly FloatRange LegThickness = new FloatRange(0.4f, 0.6f);

        // 头部宽度范围。越宽面积越大，质量也越大。
        public static readonly FloatRange HeadWidth = new FloatRange(0.6f, 1f);

        // 头部高度范围。越高面积越大，质量也越大。
        public static readonly FloatRange HeadHeight = new FloatRange(0.3f, 1f);

        // 脖子长度范围。有的马脖子会特别长，容易改变重心和摔倒姿态。
        public static readonly FloatRange NeckLength = new FloatRange(0.4f, 1.45f);

        // 脖子粗细范围。越粗面积越大，质量也越大。
        public static readonly FloatRange NeckThickness = new FloatRange(0.3f, 0.6f);


        // 尾巴长度缩放范围。尾巴越重越会影响平衡。
        public static readonly FloatRange TailScale = new FloatRange(1.8f, 2.5f);

        // 尾巴厚度缩放范围。
        public static readonly FloatRange TailThicknessScale = new FloatRange(1.5f, 2f);

        // 身体密度范围，质量按面积乘密度计算。
        public static readonly FloatRange BodyDensity = new FloatRange(1.00f, 1.38f);

        // 头部密度范围。
        public static readonly FloatRange HeadDensity = new FloatRange(0.85f, 1.85f);

        // 脖子密度范围。质量 = 脖子面积 * 这个密度。
        public static readonly FloatRange NeckDensity = new FloatRange(0.75f, 1.75f);

        // 尾巴密度范围。
        public static readonly FloatRange TailDensity = new FloatRange(0.50f, 1.65f);

        // 腿部密度范围。
        public static readonly FloatRange LegDensity = new FloatRange(1f, 1.2f);

        // 身体出现弧形肚子的概率；未命中时身体保持矩形。
        public const float BodyCurvedBellyChance = 0.55f;

        // 弧形肚子的深度，按身体高度比例计算。
        public static readonly FloatRange BodyBellyCurveDepth = new FloatRange(0.35f, 0.42f);

        // 头部嘴巴长度比例。越大，马脸越向前伸。
        public static readonly FloatRange HeadMouthLength = new FloatRange(0.18f, 0.36f);

        // 头部嘴巴厚度比例。
        public static readonly FloatRange HeadMouthHeight = new FloatRange(0.16f, 0.30f);

        // 耳朵高度比例。
        public static readonly FloatRange HeadEarHeight = new FloatRange(0.22f, 0.42f);

        // 耳朵宽度比例。
        public static readonly FloatRange HeadEarWidth = new FloatRange(0.12f, 0.24f);

        // 眼睛大小比例。
        public static readonly FloatRange HeadEyeSize = new FloatRange(0.08f, 0.15f);

        // 鼻孔大小比例。
        public static readonly FloatRange HeadNostrilSize = new FloatRange(0.05f, 0.11f);

        // 尾巴远端底边宽度比例，尾巴根部始终是很尖的顶点。
        public static readonly FloatRange TailBaseWidth = new FloatRange(0.45f, 0.92f);

        // 脚掌长度相对腿粗的倍率。
        public static readonly FloatRange FootLengthScale = new FloatRange(1.1f, 1.5f);

        // 脚掌脚趾斜边的倾斜比例。
        public static readonly FloatRange FootToeSlant = new FloatRange(0.16f, 0.34f);

        // 膝关节在腿长上的位置比例。0.5 表示上下腿等长。
        public static readonly FloatRange KneeUpperLegRatio = new FloatRange(0.46f, 0.58f);

        // 膝盖自然休息角度，正值表示向后微弯。
        public static readonly FloatRange KneeRestAngle = new FloatRange(6f, 20f);

        // 膝关节回到自然角度的弹簧力度。
        public static readonly FloatRange KneeSpring = new FloatRange(2.0f, 4.8f);

        // 膝关节阻尼。
        public static readonly FloatRange KneeDamping = new FloatRange(0.12f, 0.34f);

        // 膝关节最大恢复扭矩。
        public static readonly FloatRange KneeMaxTorque = new FloatRange(3.5f, 8.0f);

        // 上腿向前摆时给小腿的辅助前摆扭矩，数值要明显小于腿部主扭矩。
        public static readonly FloatRange KneeForwardSwingTorque = new FloatRange(0.55f, 1.35f);

        // 后蹬阶段脚底高摩擦范围。越大越不容易打滑。
        public static readonly FloatRange GripFootFriction = new FloatRange(5f, 8f);

        // 前摆回收阶段脚底低摩擦范围。太大回收会拖地，太小会像滑冰。
        public static readonly FloatRange SlideFootFriction = new FloatRange(0.35f, 0.90f);

        // 腿部中立角范围。影响默认站姿。
        public static readonly FloatRange LegNeutralAngle = new FloatRange(-4f, 4f);

        // 腿向前摆的最大角度范围。应该大于后摆角度。
        public static readonly FloatRange ForwardSwingLimit = new FloatRange(36f, 48f);

        // 腿向后蹬的最大角度范围。应该小于前摆角度。
        public static readonly FloatRange BackwardSwingLimit = new FloatRange(12f, 22f);

        // 腿部基础扭矩范围。比赛中会作为恒定内力矩施加在身体和腿之间。
        public static readonly FloatRange LegTorque = new FloatRange(10f, 15f);

        // 腿部相对身体的最大角速度，单位是度/秒；超过后本帧不再继续施加同方向扭矩。
        public static readonly FloatRange LegMaxAngularSpeed = new FloatRange(95f, 145f);

        // 低于这个相对角速度会被视为腿卡住，单位是度/秒。
        public static readonly FloatRange LegStillAngularSpeed = new FloatRange(1f, 1.5f);

        // 腿几乎不动且还没满足换向条件时，每秒增加的扭矩倍率。越大越容易从静摩擦里顶起来。
        public static readonly FloatRange LegStallTorqueGrowSpeed = new FloatRange(1.8f, 3.2f);

        // 腿卡住时允许增加到的最大扭矩倍率。2 表示最多用基础扭矩的两倍。
        public static readonly FloatRange LegStallTorqueMultiplierMax = new FloatRange(2.0f, 3.0f);

        // 腿摆到当前方向限位的这个比例以后，才算处在当前摆动方向的后半侧。
        public const float LegTurnSideThresholdRatio = 0.1f;

        // 腿部质量下限范围，防止腿太轻导致抽搐。
        public static readonly FloatRange LegMassFloor = new FloatRange(0.32f, 0.56f);

        // 头部初始随机角度偏移，影响开局时头重/前倾的程度。
        public static readonly FloatRange HeadInitialRotationOffset = new FloatRange(-8f, 8f);

        // 脖子初始随机角度偏移，长脖子马更容易因此改变平衡。
        public static readonly FloatRange NeckInitialRotationOffset = new FloatRange(-10f, 14f);

        // 脖子相对身体的上抬目标角度，单位是度；用软扭矩追这个角度，不会锁死。
        public static readonly FloatRange NeckLiftAngle = new FloatRange(52f, 68f);

        // 头相对脖子的目标角度，负值会让头稍微向前低一点。
        public static readonly FloatRange HeadLiftAngle = new FloatRange(-32f, -8f);

        // 脖子抬起弹簧力度。越大越努力抬头，但太大会抖。
        public static readonly FloatRange NeckLiftSpring = new FloatRange(3.5f, 7.5f);

        // 脖子抬起阻尼。越大越稳，但会显得更僵。
        public static readonly FloatRange NeckLiftDamping = new FloatRange(0.22f, 0.48f);

        // 脖子抬起最大扭矩。限制软弹簧最多能用多大力。
        public static readonly FloatRange NeckLiftMaxTorque = new FloatRange(7f, 14f);

        // 头部姿态弹簧力度。
        public static readonly FloatRange HeadLiftSpring = new FloatRange(2.0f, 5.0f);

        // 头部姿态阻尼。
        public static readonly FloatRange HeadLiftDamping = new FloatRange(0.16f, 0.38f);

        // 头部姿态最大扭矩。
        public static readonly FloatRange HeadLiftMaxTorque = new FloatRange(3.5f, 9f);

        // 尾巴初始随机角度偏移，影响开局时尾巴给身体的扰动。
        public static readonly FloatRange TailInitialRotationOffset = new FloatRange(-18f, 18f);


        // 初始身体倾角范围。
        public static readonly FloatRange InitialTilt = new FloatRange(-7f, 7f);

        // 腿部力量受 speedSeed 影响的映射。
        public static readonly FloatRange LegPowerFromSpeed = new FloatRange(0.78f, 1.28f);

        // 腿部力量额外随机倍率。
        public static readonly FloatRange LegPowerNoise = new FloatRange(0.86f, 1.14f);

        // 初始倾角随 chaosSeed 额外偏移的范围。
        public static readonly FloatRange InitialTiltFromChaos = new FloatRange(-5f, 5f);


        // 前摆角度随 chaosSeed 缩放的范围。
        public static readonly FloatRange ForwardSwingFromChaos = new FloatRange(0.95f, 1.12f);

        // 后摆角度随 chaosSeed 缩放的范围。
        public static readonly FloatRange BackwardSwingFromChaos = new FloatRange(0.92f, 1.08f);


        public static float Sample(FloatRange range)
        {
            return Random.Range(range.Min, range.Max);
        }

        public static float Lerp(FloatRange range, float value)
        {
            return Mathf.Lerp(range.Min, range.Max, value);
        }

        public static bool IsForwardSwing(int direction)
        {
            return direction == ForwardTorqueDirection;
        }

        public static bool IsBackwardKick(int direction)
        {
            return direction == -ForwardTorqueDirection;
        }

        public static bool IsLegAngleOnDriveSide(float relativeAngle, int direction, float neutralAngle, float forwardLimit, float backwardLimit)
        {
            float angleFromNeutral = Mathf.DeltaAngle(neutralAngle, relativeAngle);
            float limit = IsForwardSwing(direction) ? forwardLimit : backwardLimit;
            float requiredAngle = Mathf.Max(0f, limit * LegTurnSideThresholdRatio);
            return angleFromNeutral * AngleDirectionForDrive(direction) >= requiredAngle;
        }

        public static bool IsJointAtDriveLimit(float jointAngle, int direction, float minLimit, float maxLimit)
        {
            return AngleDirectionForDrive(direction) < 0 ? jointAngle <= minLimit + LegLimitTurnTolerance : jointAngle >= maxLimit - LegLimitTurnTolerance;
        }

        public static int AngleDirectionForDrive(int direction)
        {
            return IsForwardSwing(direction) ? ForwardSwingDirection : -ForwardSwingDirection;
        }


        public static float LegJointMin(float neutralAngle, float forwardLimit, float backwardLimit)
        {
            return ForwardSwingDirection < 0 ? neutralAngle - forwardLimit : neutralAngle - backwardLimit;
        }

        public static float LegJointMax(float neutralAngle, float forwardLimit, float backwardLimit)
        {
            return ForwardSwingDirection < 0 ? neutralAngle + backwardLimit : neutralAngle + forwardLimit;
        }

        internal struct FloatRange
        {
            public readonly float Min;
            public readonly float Max;

            public FloatRange(float min, float max)
            {
                Min = min;
                Max = max;
            }
        }
    }
}
