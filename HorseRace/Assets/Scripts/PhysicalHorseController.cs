using UnityEngine;

namespace HorseRace
{
    public sealed class PhysicalHorseController : MonoBehaviour
    {
        public const float FinishDistance = PhysicalHorseTuning.FinishDistance;

        private const float PhysicsLaneGap = PhysicalHorseTuning.PhysicsLaneGap;
        private const float Gravity = PhysicalHorseTuning.GravityScale;

        private Rigidbody2D body;
        private Rigidbody2D neck;
        private Rigidbody2D head;
        private Rigidbody2D tail;
        private Rigidbody2D frontUpperLeg;
        private Rigidbody2D backUpperLeg;
        private Rigidbody2D frontLeg;
        private Rigidbody2D backLeg;
        private Rigidbody2D ground;
        private HingeJoint2D frontLegJoint;
        private HingeJoint2D backLegJoint;
        private HingeJoint2D frontKneeJoint;
        private HingeJoint2D backKneeJoint;

        private Collider2D[] colliders;
        private PhysicsMaterial2D gripFootMaterial;
        private PhysicsMaterial2D slideFootMaterial;
        private BoxCollider2D frontLegCollider;
        private BoxCollider2D backLegCollider;
        private HorseGenes genes;
        private float groundY;
        private float startX;
        private float startLeadingX;
        private BodySnapshot bodyStartPose;
        private BodySnapshot neckStartPose;
        private BodySnapshot headStartPose;
        private BodySnapshot tailStartPose;
        private BodySnapshot frontUpperLegStartPose;
        private BodySnapshot backUpperLegStartPose;
        private BodySnapshot frontLegStartPose;
        private BodySnapshot backLegStartPose;
        private bool running;
        private int frontLegDirection = 1;
        private int backLegDirection = -1;
        private float frontLegSpeed;
        private float backLegSpeed;
        private float frontLegTorque;
        private float backLegTorque;
        private float frontLegAngle;
        private float backLegAngle;
        private bool frontLegAtLimit;
        private bool backLegAtLimit;
        private bool frontLegStalled;
        private bool backLegStalled;
        private float frontLegStallTorqueCharge;
        private float backLegStallTorqueCharge;

        public float BodyX => body != null ? body.position.x - startX : 0f;
        public float LeadingX => Mathf.Max(0f, LeadingPartWorldX() - startLeadingX);
        public float Progress => Mathf.InverseLerp(0f, FinishDistance, LeadingX);
        public float GroundY => groundY;
        public HorseGenes Genes => genes;
        public float FrontLegSpeed => frontLegSpeed;
        public float BackLegSpeed => backLegSpeed;
        public float FrontLegTorque => frontLegTorque;
        public float BackLegTorque => backLegTorque;
        public float LegMaxAngularSpeed => genes.LegMaxAngularSpeed;
        public float FrontLegAngle => frontLegAngle;
        public float BackLegAngle => backLegAngle;
        public int FrontLegDirection => frontLegDirection;
        public int BackLegDirection => backLegDirection;
        public bool FrontLegAtLimit => frontLegAtLimit;
        public bool BackLegAtLimit => backLegAtLimit;
        public bool FrontLegStalled => frontLegStalled;
        public bool BackLegStalled => backLegStalled;

        public static PhysicalHorseController Create(int index, Color color, float speedSeed, float staminaSeed, float focusSeed, float chaosSeed)
        {
            GameObject root = new GameObject("PhysicalHorse_" + index);
            PhysicalHorseController controller = root.AddComponent<PhysicalHorseController>();
            controller.Initialize(index, color, speedSeed, staminaSeed, focusSeed, chaosSeed);
            return controller;
        }

        public void SetRunning(bool value)
        {
            bool starting = value && !running;
            running = value;
            if (starting)
            {
                RestoreStartPose();
                SetPartsSimulated(true);
                Physics2D.SyncTransforms();
                ResetRaceOrigin();
                ZeroAllVelocities();
                frontLegDirection = Random.value > 0.5f ? 1 : -1;
                backLegDirection = -frontLegDirection;
            }
            else if (!value)
            {
                ZeroAllVelocities();
                SetPartsSimulated(false);
            }

            frontLegStallTorqueCharge = 0f;
            backLegStallTorqueCharge = 0f;
            if (value)
            {
                WakeAll();
            }
        }

        public PartPose GetBodyPose()
        {
            return PoseFrom(body, genes.BodySize);
        }

        public PartPose GetNeckPose()
        {
            return PoseFrom(neck, genes.NeckSize);
        }


        public PartPose GetHeadPose()
        {
            return PoseFrom(head, genes.HeadSize);
        }

        public PartPose GetTailPose()
        {
            return PoseFrom(tail, genes.TailSize);
        }

        public PartPose GetFrontLegPose()
        {
            return PoseFrom(frontUpperLeg != null ? frontUpperLeg : frontLeg, frontUpperLeg != null ? genes.UpperLegSize : genes.LegSize);
        }

        public PartPose GetFrontLowerLegPose()
        {
            return frontUpperLeg != null ? PoseFrom(frontLeg, genes.LowerLegSize) : default;
        }

        public PartPose GetBackLegPose()
        {
            return PoseFrom(backUpperLeg != null ? backUpperLeg : backLeg, backUpperLeg != null ? genes.UpperLegSize : genes.LegSize);
        }

        public PartPose GetBackLowerLegPose()
        {
            return backUpperLeg != null ? PoseFrom(backLeg, genes.LowerLegSize) : default;
        }


        private void Initialize(int index, Color color, float speedSeed, float staminaSeed, float focusSeed, float chaosSeed)
        {
            genes = HorseGenes.Create(color, speedSeed, staminaSeed, focusSeed, chaosSeed);
            groundY = -index * PhysicsLaneGap;
            startX = 0f;

            gripFootMaterial = new PhysicsMaterial2D("HorseFootGrip_" + index)
            {
                friction = genes.GripFootFriction,
                bounciness = PhysicalHorseTuning.MaterialBounciness
            };

            slideFootMaterial = new PhysicsMaterial2D("HorseFootSlide_" + index)
            {
                friction = genes.SlideFootFriction,
                bounciness = PhysicalHorseTuning.MaterialBounciness
            };

            PhysicsMaterial2D bodyMaterial = new PhysicsMaterial2D("HorseBody_" + index)
            {
                friction = PhysicalHorseTuning.BodyFriction,
                bounciness = PhysicalHorseTuning.MaterialBounciness
            };

            ground = CreateBody("Ground", new Vector2(FinishDistance * 0.5f, groundY + PhysicalHorseTuning.GroundYOffset), 0f, new Vector2(FinishDistance + PhysicalHorseTuning.GroundExtraLength, PhysicalHorseTuning.GroundHeight), PhysicalHorseTuning.GroundDensity, bodyMaterial, RigidbodyType2D.Static);
            float bodyRotation = genes.InitialTilt;
            body = CreateBody("Body", new Vector2(startX, groundY + genes.LegSize.y + genes.BodySize.y * 0.5f + PhysicalHorseTuning.BodyGroundClearance), bodyRotation, genes.BodySize, genes.BodyDensity, bodyMaterial, RigidbodyType2D.Dynamic);
            Vector2 neckBodyAnchor = new Vector2(genes.BodySize.x * PhysicalHorseTuning.NeckBodyAnchorX, genes.BodySize.y * PhysicalHorseTuning.NeckBodyAnchorY);
            Vector2 neckBodyLocalAnchor = new Vector2(genes.NeckSize.x * PhysicalHorseTuning.NeckLocalBodyAnchorX, 0f);
            Vector2 neckHeadLocalAnchor = new Vector2(genes.NeckSize.x * PhysicalHorseTuning.NeckLocalHeadAnchorX, 0f);
            Vector2 headNeckAnchor = new Vector2(genes.HeadSize.x * PhysicalHorseTuning.HeadNeckAnchorX, 0f);
            float neckRotation = bodyRotation + PhysicalHorseTuning.Sample(PhysicalHorseTuning.NeckInitialRotationOffset);
            float headRotation = neckRotation + PhysicalHorseTuning.Sample(PhysicalHorseTuning.HeadInitialRotationOffset);
            float tailRotation = bodyRotation + PhysicalHorseTuning.Sample(PhysicalHorseTuning.TailInitialRotationOffset);
            neck = CreateBody("Neck", PositionFromAnchors(body.position, bodyRotation, neckBodyAnchor, neckRotation, neckBodyLocalAnchor), neckRotation, genes.NeckSize, genes.NeckDensity, bodyMaterial, RigidbodyType2D.Dynamic);
            head = CreateBody("Head", PositionFromAnchors(neck.position, neckRotation, neckHeadLocalAnchor, headRotation, headNeckAnchor), headRotation, genes.HeadSize, genes.HeadDensity, bodyMaterial, RigidbodyType2D.Dynamic);
            Vector2 tailBodyAnchor = new Vector2(genes.BodySize.x * PhysicalHorseTuning.TailBodyAnchorX, genes.BodySize.y * PhysicalHorseTuning.TailBodyAnchorY);
            Vector2 tailLocalAnchor = new Vector2(genes.TailSize.x * PhysicalHorseTuning.TailLocalAnchorX, 0f);
            tail = CreateBody("Tail", PositionFromAnchors(body.position, bodyRotation, tailBodyAnchor, tailRotation, tailLocalAnchor), tailRotation, genes.TailSize, genes.TailDensity, bodyMaterial, RigidbodyType2D.Dynamic);
            CreateLeg("Front", 1f, genes.FrontLegHasKnee, out frontUpperLeg, out frontLeg, out frontLegJoint, out frontKneeJoint, out frontLegCollider);
            CreateLeg("Back", -1f, genes.BackLegHasKnee, out backUpperLeg, out backLeg, out backLegJoint, out backKneeJoint, out backLegCollider);

            ConfigurePartPhysics();
            UpdateLegFriction();

            Connect(neck, body, neckBodyLocalAnchor, neckBodyAnchor, PhysicalHorseTuning.NeckJointMin, PhysicalHorseTuning.NeckJointMax);
            Connect(head, neck, headNeckAnchor, neckHeadLocalAnchor, PhysicalHorseTuning.HeadJointMin, PhysicalHorseTuning.HeadJointMax);
            Connect(tail, body, new Vector2(genes.TailSize.x * PhysicalHorseTuning.TailLocalAnchorX, 0f), new Vector2(genes.BodySize.x * PhysicalHorseTuning.TailBodyAnchorX, genes.BodySize.y * PhysicalHorseTuning.TailBodyAnchorY), PhysicalHorseTuning.TailJointMin, PhysicalHorseTuning.TailJointMax);

            colliders = GetComponentsInChildren<Collider2D>();
            IgnoreInternalCollisions();
            CaptureStartPose();
            ResetRaceOrigin();
            SetRunning(false);
        }

        private void FixedUpdate()
        {
            if (body == null)
            {
                return;
            }
            if (!running)
            {
                return;
            }

            DriveUpperBodyPosture();
            DriveSwingLeg(frontLegJoint, frontUpperLeg != null ? frontUpperLeg : frontLeg, body, ref frontLegDirection, genes.LegTorque, genes.LegMaxAngularSpeed, ref frontLegStallTorqueCharge, ref frontLegSpeed, ref frontLegTorque, ref frontLegAngle, ref frontLegAtLimit, ref frontLegStalled);
            DriveSwingLeg(backLegJoint, backUpperLeg != null ? backUpperLeg : backLeg, body, ref backLegDirection, genes.LegTorque, genes.LegMaxAngularSpeed, ref backLegStallTorqueCharge, ref backLegSpeed, ref backLegTorque, ref backLegAngle, ref backLegAtLimit, ref backLegStalled);
            DriveKneeJoints();
            UpdateLegFriction();

        }

        private void DriveUpperBodyPosture()
        {
            DrivePartTowardAngle(neck, body, genes.NeckLiftAngle, genes.NeckLiftSpring, genes.NeckLiftDamping, genes.NeckLiftMaxTorque);
            DrivePartTowardAngle(head, neck, genes.HeadLiftAngle, genes.HeadLiftSpring, genes.HeadLiftDamping, genes.HeadLiftMaxTorque);
        }


        private void StabilizeIdle()
        {
            DrivePartTowardAngle(frontUpperLeg != null ? frontUpperLeg : frontLeg, body, genes.LegNeutralAngle, PhysicalHorseTuning.IdleLegSpring, PhysicalHorseTuning.IdleLegDamping, PhysicalHorseTuning.IdleLegMaxTorque);
            DrivePartTowardAngle(backUpperLeg != null ? backUpperLeg : backLeg, body, genes.LegNeutralAngle, PhysicalHorseTuning.IdleLegSpring, PhysicalHorseTuning.IdleLegDamping, PhysicalHorseTuning.IdleLegMaxTorque);
            DriveKneeJoints();

        }

        private Rigidbody2D CreateBody(string name, Vector2 position, float rotation, Vector2 size, float density, PhysicsMaterial2D material, RigidbodyType2D bodyType)
        {
            GameObject part = new GameObject(name);
            part.transform.SetParent(transform, false);
            part.transform.position = position;
            part.transform.rotation = Quaternion.Euler(0f, 0f, rotation);

            Rigidbody2D rb = part.AddComponent<Rigidbody2D>();
            rb.bodyType = bodyType;
            rb.gravityScale = Gravity;
            rb.drag = PhysicalHorseTuning.DefaultDrag;
            rb.angularDrag = PhysicalHorseTuning.DefaultAngularDrag;
            rb.mass = Mathf.Max(PhysicalHorseTuning.MinimumBodyPartMass, size.x * size.y * density);
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            BoxCollider2D collider = part.AddComponent<BoxCollider2D>();
            collider.size = size;
            collider.sharedMaterial = material;
            return rb;
        }

        private void CreateLeg(string prefix, float xSign, bool hasKnee, out Rigidbody2D upperLeg, out Rigidbody2D lowerLeg, out HingeJoint2D hipJoint, out HingeJoint2D kneeJoint, out BoxCollider2D lowerCollider)
        {
            Vector2 hipAnchor = new Vector2(xSign * genes.BodySize.x * PhysicalHorseTuning.LegBodyXOffset, genes.BodySize.y * PhysicalHorseTuning.LegBodyAnchorY);
            if (hasKnee)
            {
                Vector2 upperTopAnchor = new Vector2(0f, genes.UpperLegSize.y * PhysicalHorseTuning.LegLocalAnchorY);
                Vector2 upperBottomAnchor = new Vector2(0f, -genes.UpperLegSize.y * PhysicalHorseTuning.LegLocalAnchorY);
                Vector2 lowerTopAnchor = new Vector2(0f, genes.LowerLegSize.y * PhysicalHorseTuning.LegLocalAnchorY);
                float upperRotation = body.rotation;
                float lowerRotation = body.rotation;
                Vector2 upperPosition = PositionFromAnchors(body.position, body.rotation, hipAnchor, upperRotation, upperTopAnchor);
                upperLeg = CreateBody(prefix + "UpperLeg", upperPosition, upperRotation, genes.UpperLegSize, genes.LegDensity, slideFootMaterial, RigidbodyType2D.Dynamic);
                lowerLeg = CreateBody(prefix + "LowerLeg", PositionFromAnchors(upperLeg.position, upperLeg.rotation, upperBottomAnchor, lowerRotation, lowerTopAnchor), lowerRotation, genes.LowerLegSize, genes.LegDensity, slideFootMaterial, RigidbodyType2D.Dynamic);
                hipJoint = Connect(upperLeg, body, upperTopAnchor, hipAnchor, PhysicalHorseTuning.LegJointMin(genes.LegNeutralAngle, genes.ForwardSwingLimit, genes.BackwardSwingLimit), PhysicalHorseTuning.LegJointMax(genes.LegNeutralAngle, genes.ForwardSwingLimit, genes.BackwardSwingLimit));
                kneeJoint = Connect(lowerLeg, upperLeg, lowerTopAnchor, upperBottomAnchor, PhysicalHorseTuning.KneeJointMin, PhysicalHorseTuning.KneeJointMax);
            }
            else
            {
                upperLeg = null;
                kneeJoint = null;
                lowerLeg = CreateBody(prefix + "Leg", PositionFromAnchors(body.position, body.rotation, hipAnchor, body.rotation, new Vector2(0f, genes.LegSize.y * PhysicalHorseTuning.LegLocalAnchorY)), body.rotation, genes.LegSize, genes.LegDensity, slideFootMaterial, RigidbodyType2D.Dynamic);
                hipJoint = Connect(lowerLeg, body, new Vector2(0f, genes.LegSize.y * PhysicalHorseTuning.LegLocalAnchorY), hipAnchor, PhysicalHorseTuning.LegJointMin(genes.LegNeutralAngle, genes.ForwardSwingLimit, genes.BackwardSwingLimit), PhysicalHorseTuning.LegJointMax(genes.LegNeutralAngle, genes.ForwardSwingLimit, genes.BackwardSwingLimit));
            }

            lowerCollider = lowerLeg.GetComponent<BoxCollider2D>();
        }


        private void ConfigurePartPhysics()
        {
            body.drag = PhysicalHorseTuning.BodyDrag;

            ConfigureLegPhysics(frontUpperLeg);
            ConfigureLegPhysics(backUpperLeg);
            ConfigureLegPhysics(frontLeg);
            ConfigureLegPhysics(backLeg);
        }

        private void ConfigureLegPhysics(Rigidbody2D leg)
        {
            if (leg == null)
            {
                return;
            }

            leg.drag = PhysicalHorseTuning.LegDrag;
            leg.mass = Mathf.Max(leg.mass, genes.LegMassFloor);
        }


        private static Vector2 PositionFromAnchors(Vector2 parentPosition, float parentRotation, Vector2 parentAnchor, float partRotation, Vector2 partAnchor)
        {
            return parentPosition + RotateLocal(parentAnchor, parentRotation) - RotateLocal(partAnchor, partRotation);
        }

        private static Vector2 RotateLocal(Vector2 local, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radians);
            float cos = Mathf.Cos(radians);
            return new Vector2(local.x * cos - local.y * sin, local.x * sin + local.y * cos);
        }
        private HingeJoint2D Connect(Rigidbody2D part, Rigidbody2D connectedBody, Vector2 anchor, Vector2 connectedAnchor, float min, float max)
        {
            HingeJoint2D joint = part.gameObject.AddComponent<HingeJoint2D>();
            joint.connectedBody = connectedBody;
            joint.anchor = anchor;
            joint.connectedAnchor = connectedAnchor;
            joint.autoConfigureConnectedAnchor = false;
            joint.enableCollision = false;
            joint.useLimits = true;
            joint.useMotor = false;
            joint.limits = new JointAngleLimits2D { min = min, max = max };
            return joint;
        }

        private void UpdateLegFriction()
        {
            SetLegFriction(frontLegCollider, frontLegDirection);
            SetLegFriction(backLegCollider, backLegDirection);
        }

        private void SetLegFriction(BoxCollider2D collider, int direction)
        {
            if (collider == null)
            {
                return;
            }

            collider.sharedMaterial = PhysicalHorseTuning.IsBackwardKick(direction) ? gripFootMaterial : slideFootMaterial;
        }

        private void DriveKneeJoints()
        {
            DriveKneeJoint(frontLeg, frontUpperLeg, frontKneeJoint, frontLegDirection);
            DriveKneeJoint(backLeg, backUpperLeg, backKneeJoint, backLegDirection);
        }

        private void DriveKneeJoint(Rigidbody2D lowerLeg, Rigidbody2D upperLeg, HingeJoint2D kneeJoint, int hipDirection)
        {
            if (lowerLeg == null || upperLeg == null)
            {
                return;
            }

            DrivePartTowardAngle(lowerLeg, upperLeg, genes.KneeRestAngle, genes.KneeSpring, genes.KneeDamping, genes.KneeMaxTorque);
            if (running && kneeJoint != null && PhysicalHorseTuning.IsForwardSwing(hipDirection) && kneeJoint.jointAngle > PhysicalHorseTuning.KneeJointMin)
            {
                ApplyInternalTorque(lowerLeg, upperLeg, -genes.KneeForwardSwingTorque);
            }
        }

        private void DriveSwingLeg(HingeJoint2D joint, Rigidbody2D part, Rigidbody2D parent, ref int direction, float driveTorque, float maxAngularSpeed, ref float stallTorqueCharge, ref float debugSpeed, ref float debugTorque, ref float debugAngle, ref bool debugAtLimit, ref bool debugStalled)
        {
            if (joint == null || part == null || parent == null)
            {
                stallTorqueCharge = 0f;
                debugSpeed = 0f;
                debugTorque = 0f;
                debugAngle = 0f;
                debugAtLimit = false;
                debugStalled = false;
                return;
            }

            float relativeVelocity = part.angularVelocity - parent.angularVelocity;
            float jointAngle = joint.jointAngle;
            JointAngleLimits2D limits = joint.limits;
            bool isNearlyStopped = Mathf.Abs(relativeVelocity) <= genes.LegStillAngularSpeed;
            bool reachedLimit = PhysicalHorseTuning.IsJointAtDriveLimit(jointAngle, direction, limits.min, limits.max);
            bool stalledOnDriveSide = isNearlyStopped && PhysicalHorseTuning.IsLegAngleOnDriveSide(jointAngle, direction, genes.LegNeutralAngle, genes.ForwardSwingLimit, genes.BackwardSwingLimit);
            bool turnedThisFrame = reachedLimit || stalledOnDriveSide;
            if (turnedThisFrame)
            {
                direction *= -1;
            }

            float speedInDriveDirection = relativeVelocity * PhysicalHorseTuning.AngleDirectionForDrive(direction);
            bool speedLimited = maxAngularSpeed > 0f && speedInDriveDirection >= maxAngularSpeed;
            if (turnedThisFrame || speedLimited || !isNearlyStopped)
            {
                stallTorqueCharge = 0f;
            }
            else
            {
                float maxCharge = Mathf.Max(0f, genes.LegStallTorqueMultiplierMax - 1f);
                stallTorqueCharge = Mathf.Min(maxCharge, stallTorqueCharge + genes.LegStallTorqueGrowSpeed * Time.fixedDeltaTime);
            }

            float torqueMultiplier = 1f + stallTorqueCharge;
            float torque = speedLimited ? 0f : direction * driveTorque * torqueMultiplier;
            debugSpeed = relativeVelocity;
            debugTorque = torque;
            debugAngle = jointAngle;
            debugAtLimit = reachedLimit;
            debugStalled = stalledOnDriveSide;
            ApplyInternalTorque(part, parent, torque);
        }


        private void DrivePartTowardAngle(Rigidbody2D part, Rigidbody2D parent, float targetAngle, float spring, float damping, float maxTorque)
        {
            if (part == null || parent == null)
            {
                return;
            }

            float relativeAngle = Mathf.DeltaAngle(parent.rotation, part.rotation);
            float error = Mathf.DeltaAngle(relativeAngle, targetAngle);
            float relativeVelocity = part.angularVelocity - parent.angularVelocity;
            float torque = Mathf.Clamp(error * spring - relativeVelocity * damping, -maxTorque, maxTorque);
            ApplyInternalTorque(part, parent, torque);
        }

        private void ApplyInternalTorque(Rigidbody2D part, Rigidbody2D parent, float torque)
        {
            part.AddTorque(torque);
            parent.AddTorque(-torque);
        }

        private void IgnoreInternalCollisions()
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                for (int j = i + 1; j < colliders.Length; j++)
                {
                    if (colliders[i].attachedRigidbody != ground && colliders[j].attachedRigidbody != ground)
                    {
                        Physics2D.IgnoreCollision(colliders[i], colliders[j]);
                    }
                }
            }
        }

        private void CaptureStartPose()
        {
            bodyStartPose = CapturePose(body);
            neckStartPose = CapturePose(neck);
            headStartPose = CapturePose(head);
            tailStartPose = CapturePose(tail);
            frontUpperLegStartPose = CapturePose(frontUpperLeg);
            backUpperLegStartPose = CapturePose(backUpperLeg);
            frontLegStartPose = CapturePose(frontLeg);
            backLegStartPose = CapturePose(backLeg);
        }

        private void RestoreStartPose()
        {
            RestorePose(body, bodyStartPose);
            RestorePose(neck, neckStartPose);
            RestorePose(head, headStartPose);
            RestorePose(tail, tailStartPose);
            RestorePose(frontUpperLeg, frontUpperLegStartPose);
            RestorePose(backUpperLeg, backUpperLegStartPose);
            RestorePose(frontLeg, frontLegStartPose);
            RestorePose(backLeg, backLegStartPose);
            Physics2D.SyncTransforms();
        }

        private void ResetRaceOrigin()
        {
            startX = body != null ? body.position.x : 0f;
            startLeadingX = LeadingPartWorldX();
        }

        private void SetPartsSimulated(bool simulated)
        {
            SetSimulated(body, simulated);
            SetSimulated(neck, simulated);
            SetSimulated(head, simulated);
            SetSimulated(tail, simulated);
            SetSimulated(frontUpperLeg, simulated);
            SetSimulated(backUpperLeg, simulated);
            SetSimulated(frontLeg, simulated);
            SetSimulated(backLeg, simulated);
        }

        private static void SetSimulated(Rigidbody2D rb, bool simulated)
        {
            if (rb != null)
            {
                rb.simulated = simulated;
            }
        }

        private void ZeroAllVelocities()
        {
            ZeroVelocity(body);
            ZeroVelocity(neck);
            ZeroVelocity(head);
            ZeroVelocity(tail);
            ZeroVelocity(frontUpperLeg);
            ZeroVelocity(backUpperLeg);
            ZeroVelocity(frontLeg);
            ZeroVelocity(backLeg);
        }

        private static void ZeroVelocity(Rigidbody2D rb)
        {
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }

        private static BodySnapshot CapturePose(Rigidbody2D rb)
        {
            return rb == null ? default : new BodySnapshot
            {
                Position = rb.position,
                Rotation = rb.rotation,
                Valid = true
            };
        }

        private static void RestorePose(Rigidbody2D rb, BodySnapshot pose)
        {
            if (rb != null && pose.Valid)
            {
                rb.position = pose.Position;
                rb.rotation = pose.Rotation;
            }
        }
        private void WakeAll()
        {
            body?.WakeUp();
            neck?.WakeUp();
            head?.WakeUp();
            tail?.WakeUp();
            frontUpperLeg?.WakeUp();
            backUpperLeg?.WakeUp();
            frontLeg?.WakeUp();
            backLeg?.WakeUp();
        }

        private float LeadingPartWorldX()
        {
            if (colliders == null || colliders.Length == 0)
            {
                return body != null ? body.position.x : startX;
            }

            float leading = float.NegativeInfinity;
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider2D partCollider = colliders[i];
                if (partCollider == null || partCollider.attachedRigidbody == ground)
                {
                    continue;
                }

                leading = Mathf.Max(leading, partCollider.bounds.max.x);
            }

            return float.IsNegativeInfinity(leading) ? (body != null ? body.position.x : startX) : leading;
        }


        private PartPose PoseFrom(Rigidbody2D rb, Vector2 size)
        {
            if (rb == null)
            {
                return default;
            }

            return new PartPose
            {
                Position = rb.position,
                Rotation = rb.rotation,
                Size = size,
                Visible = true
            };
        }

        private struct BodySnapshot
        {
            public Vector2 Position;
            public float Rotation;
            public bool Valid;
        }
        public struct PartPose
        {
            public Vector2 Position;
            public float Rotation;
            public Vector2 Size;
            public bool Visible;
        }

        public struct HorseGenes
        {
            public Vector2 BodySize;
            public Vector2 NeckSize;
            public Vector2 HeadSize;
            public Vector2 TailSize;
            public Vector2 LegSize;
            public Vector2 UpperLegSize;
            public Vector2 LowerLegSize;
            public float BodyDensity;
            public float NeckDensity;
            public float HeadDensity;
            public float TailDensity;
            public float LegDensity;
            public float GripFootFriction;
            public float SlideFootFriction;
            public float LegNeutralAngle;
            public float ForwardSwingLimit;
            public float BackwardSwingLimit;
            public float LegTorque;
            public float LegStillAngularSpeed;
            public float LegMaxAngularSpeed;
            public float LegStallTorqueGrowSpeed;
            public float LegStallTorqueMultiplierMax;
            public float LegMassFloor;
            public bool FrontLegHasKnee;
            public bool BackLegHasKnee;
            public float KneeRestAngle;
            public float KneeSpring;
            public float KneeDamping;
            public float KneeMaxTorque;
            public float KneeForwardSwingTorque;

            public bool BodyHasCurvedBelly;
            public float BodyBellyCurveDepth;
            public float HeadMouthLength;
            public float HeadMouthHeight;
            public float HeadEarHeight;
            public float HeadEarWidth;
            public float HeadEyeSize;
            public float HeadNostrilSize;
            public float TailBaseWidth;
            public float FootLengthScale;
            public float FootToeSlant;

            public int VisualSeed;
            public float BodySkinny;
            public int BodyPatternStyle;
            public float NeckGiraffe;
            public float HeadSquare;
            public bool HeadHasBack;
            public int EyeStyle;
            public int Bugeye;
            public float EyeBoxX;
            public float EyeBoxY;
            public float EyeBoxSize;
            public float PupilSize;
            public bool HasPupil;
            public float BrowSize;
            public float BrowSlant;
            public int EarStyle;
            public int EarShape;
            public float EarFlop;
            public float EarX;
            public float EarSize;
            public float EarAspect;
            public float EarSlant;
            public float EarInterior;
            public int TeethShape;
            public bool HasMouth;
            public float MouthY;
            public float MouthSize;
            public float Jaw;
            public bool TeethUpper;
            public bool TeethLower;
            public float Tongue;
            public int TongueSegs;
            public int NoseStyle;
            public bool NoseInny;
            public float NoseY;
            public float NoseSize;
            public float NoseInterior;
            public int TailShape;
            public float NeckLiftAngle;
            public float HeadLiftAngle;
            public float NeckLiftSpring;
            public float NeckLiftDamping;
            public float NeckLiftMaxTorque;
            public float HeadLiftSpring;
            public float HeadLiftDamping;
            public float HeadLiftMaxTorque;

            public float InitialTilt;
            public Color Color;

            public static HorseGenes Create(Color color, float speedSeed, float staminaSeed, float focusSeed, float chaosSeed)
            {
                float bodyLength = PhysicalHorseTuning.Sample(PhysicalHorseTuning.BodyLength);
                float bodyHeight = PhysicalHorseTuning.Sample(PhysicalHorseTuning.BodyHeight);
                float legLength = PhysicalHorseTuning.Sample(PhysicalHorseTuning.LegLength);
                float legThickness = PhysicalHorseTuning.Sample(PhysicalHorseTuning.LegThickness);
                float kneeUpperLegRatio = PhysicalHorseTuning.Sample(PhysicalHorseTuning.KneeUpperLegRatio);
                bool bodyHasCurvedBelly = Random.value < PhysicalHorseTuning.BodyCurvedBellyChance;
                float headWidth = PhysicalHorseTuning.Sample(PhysicalHorseTuning.HeadWidth);
                float headHeight = PhysicalHorseTuning.Sample(PhysicalHorseTuning.HeadHeight);
                float neckLength = PhysicalHorseTuning.Sample(PhysicalHorseTuning.NeckLength);
                float neckThickness = PhysicalHorseTuning.Sample(PhysicalHorseTuning.NeckThickness);
                float tailScale = PhysicalHorseTuning.Sample(PhysicalHorseTuning.TailScale);
                float legPower = PhysicalHorseTuning.Lerp(PhysicalHorseTuning.LegPowerFromSpeed, speedSeed) * PhysicalHorseTuning.Sample(PhysicalHorseTuning.LegPowerNoise);

                return new HorseGenes
                {
                    BodySize = new Vector2(bodyLength, bodyHeight),
                    NeckSize = new Vector2(neckLength, neckThickness),
                    HeadSize = new Vector2(headWidth, headHeight),
                    TailSize = new Vector2(PhysicalHorseTuning.TailBaseLength * tailScale, PhysicalHorseTuning.TailBaseThickness * PhysicalHorseTuning.Sample(PhysicalHorseTuning.TailThicknessScale)),
                    LegSize = new Vector2(legThickness, legLength),
                    UpperLegSize = new Vector2(legThickness, legLength * kneeUpperLegRatio),
                    LowerLegSize = new Vector2(legThickness, legLength * (1f - kneeUpperLegRatio)),
                    BodyDensity = PhysicalHorseTuning.Sample(PhysicalHorseTuning.BodyDensity),
                    NeckDensity = PhysicalHorseTuning.Sample(PhysicalHorseTuning.NeckDensity),
                    HeadDensity = PhysicalHorseTuning.Sample(PhysicalHorseTuning.HeadDensity),
                    TailDensity = PhysicalHorseTuning.Sample(PhysicalHorseTuning.TailDensity),
                    LegDensity = PhysicalHorseTuning.Sample(PhysicalHorseTuning.LegDensity),
                    GripFootFriction = PhysicalHorseTuning.Sample(PhysicalHorseTuning.GripFootFriction),
                    SlideFootFriction = PhysicalHorseTuning.Sample(PhysicalHorseTuning.SlideFootFriction),
                    LegNeutralAngle = PhysicalHorseTuning.Sample(PhysicalHorseTuning.LegNeutralAngle),
                    ForwardSwingLimit = PhysicalHorseTuning.Sample(PhysicalHorseTuning.ForwardSwingLimit) * PhysicalHorseTuning.Lerp(PhysicalHorseTuning.ForwardSwingFromChaos, chaosSeed),
                    BackwardSwingLimit = PhysicalHorseTuning.Sample(PhysicalHorseTuning.BackwardSwingLimit) * PhysicalHorseTuning.Lerp(PhysicalHorseTuning.BackwardSwingFromChaos, chaosSeed),
                    LegTorque = PhysicalHorseTuning.Sample(PhysicalHorseTuning.LegTorque) * legPower,
                    LegMaxAngularSpeed = PhysicalHorseTuning.Sample(PhysicalHorseTuning.LegMaxAngularSpeed),
                    LegStillAngularSpeed = PhysicalHorseTuning.Sample(PhysicalHorseTuning.LegStillAngularSpeed),
                    LegStallTorqueGrowSpeed = PhysicalHorseTuning.Sample(PhysicalHorseTuning.LegStallTorqueGrowSpeed),
                    LegStallTorqueMultiplierMax = PhysicalHorseTuning.Sample(PhysicalHorseTuning.LegStallTorqueMultiplierMax),
                    LegMassFloor = PhysicalHorseTuning.Sample(PhysicalHorseTuning.LegMassFloor),
                    FrontLegHasKnee = Random.value < PhysicalHorseTuning.KneeJointChance,
                    BackLegHasKnee = Random.value < PhysicalHorseTuning.KneeJointChance,
                    KneeRestAngle = PhysicalHorseTuning.Sample(PhysicalHorseTuning.KneeRestAngle),
                    KneeSpring = PhysicalHorseTuning.Sample(PhysicalHorseTuning.KneeSpring),
                    KneeDamping = PhysicalHorseTuning.Sample(PhysicalHorseTuning.KneeDamping),
                    KneeMaxTorque = PhysicalHorseTuning.Sample(PhysicalHorseTuning.KneeMaxTorque),
                    KneeForwardSwingTorque = PhysicalHorseTuning.Sample(PhysicalHorseTuning.KneeForwardSwingTorque),

                    BodyHasCurvedBelly = bodyHasCurvedBelly,
                    BodyBellyCurveDepth = bodyHasCurvedBelly ? PhysicalHorseTuning.Sample(PhysicalHorseTuning.BodyBellyCurveDepth) : 0f,
                    HeadMouthLength = PhysicalHorseTuning.Sample(PhysicalHorseTuning.HeadMouthLength),
                    HeadMouthHeight = PhysicalHorseTuning.Sample(PhysicalHorseTuning.HeadMouthHeight),
                    HeadEarHeight = PhysicalHorseTuning.Sample(PhysicalHorseTuning.HeadEarHeight),
                    HeadEarWidth = PhysicalHorseTuning.Sample(PhysicalHorseTuning.HeadEarWidth),
                    HeadEyeSize = PhysicalHorseTuning.Sample(PhysicalHorseTuning.HeadEyeSize),
                    HeadNostrilSize = PhysicalHorseTuning.Sample(PhysicalHorseTuning.HeadNostrilSize),
                    TailBaseWidth = PhysicalHorseTuning.Sample(PhysicalHorseTuning.TailBaseWidth),
                    FootLengthScale = PhysicalHorseTuning.Sample(PhysicalHorseTuning.FootLengthScale),
                    FootToeSlant = PhysicalHorseTuning.Sample(PhysicalHorseTuning.FootToeSlant),

                    VisualSeed = Random.Range(1000, 1000000),
                    BodySkinny = Random.Range(0f, 1f),
                    BodyPatternStyle = Random.Range(0, 3),
                    NeckGiraffe = Random.Range(0f, 1f),
                    HeadSquare = Random.value < 0.38f ? Random.Range(0.35f, 1f) : Random.Range(0f, 0.35f),
                    HeadHasBack = Random.value < 0.58f,
                    EyeStyle = Random.Range(0, 3),
                    Bugeye = Random.value < 0.22f ? Random.Range(1, 3) : 0,
                    EyeBoxX = Random.Range(0f, 1f),
                    EyeBoxY = Random.Range(0f, 1f),
                    EyeBoxSize = Random.Range(0.55f, 1.45f),
                    PupilSize = Random.Range(0.28f, 0.70f),
                    HasPupil = Random.value < 0.85f,
                    BrowSize = Random.value < 0.45f ? Random.Range(0.35f, 1.15f) : 0f,
                    BrowSlant = Random.Range(-18f, 18f),
                    EarStyle = Random.value < 0.16f ? 0 : Random.Range(1, 3),
                    EarShape = Random.Range(1, 5),
                    EarFlop = Random.Range(0f, 1f),
                    EarX = Random.Range(0f, 1f),
                    EarSize = Random.Range(0.65f, 1.55f),
                    EarAspect = Random.Range(0.75f, 1.65f),
                    EarSlant = Random.Range(-36f, 36f),
                    EarInterior = Random.Range(0f, 1f),
                    TeethShape = Random.Range(0, 4),
                    HasMouth = Random.value < 0.86f,
                    MouthY = Random.Range(0.40f, 1f),
                    MouthSize = Random.Range(0.55f, 1.55f),
                    Jaw = Random.Range(-0.35f, 0.35f),
                    TeethUpper = Random.value < 0.50f,
                    TeethLower = Random.value < 0.50f,
                    Tongue = Random.value < 0.30f ? Random.Range(0.35f, 1f) : 0f,
                    TongueSegs = Random.Range(0, 3),
                    NoseStyle = Random.Range(0, 4),
                    NoseInny = Random.value < 0.25f,
                    NoseY = Random.Range(0f, 1f),
                    NoseSize = Random.Range(0.55f, 1.55f),
                    NoseInterior = Random.Range(0f, 1f),
                    TailShape = Random.Range(0, 2),
                    NeckLiftAngle = PhysicalHorseTuning.Sample(PhysicalHorseTuning.NeckLiftAngle),
                    HeadLiftAngle = PhysicalHorseTuning.Sample(PhysicalHorseTuning.HeadLiftAngle),
                    NeckLiftSpring = PhysicalHorseTuning.Sample(PhysicalHorseTuning.NeckLiftSpring),
                    NeckLiftDamping = PhysicalHorseTuning.Sample(PhysicalHorseTuning.NeckLiftDamping),
                    NeckLiftMaxTorque = PhysicalHorseTuning.Sample(PhysicalHorseTuning.NeckLiftMaxTorque),
                    HeadLiftSpring = PhysicalHorseTuning.Sample(PhysicalHorseTuning.HeadLiftSpring),
                    HeadLiftDamping = PhysicalHorseTuning.Sample(PhysicalHorseTuning.HeadLiftDamping),
                    HeadLiftMaxTorque = PhysicalHorseTuning.Sample(PhysicalHorseTuning.HeadLiftMaxTorque),

                    InitialTilt = PhysicalHorseTuning.Sample(PhysicalHorseTuning.InitialTilt) + PhysicalHorseTuning.Lerp(PhysicalHorseTuning.InitialTiltFromChaos, chaosSeed),
                    Color = color
                };
            }
        }
    }
}




