using System.Collections;
using UnityEngine;

namespace Project.Projectile
{
    public class ProjectileInfo : MonoBehaviour
    {
        private UnitInfo target;

        private float speed;
        private float arcAngle;
        private float lifeTime;
        private float castInterval;
        private float animationTime;

        private Vector3 startPosition;
        private Vector3 targetPosition;

        private IProjectileEnd projectileEnd;

        private IProjectileCast projectileCast;

        private Coroutine projectileCastCoroutine;

        private ProjectileData projectileData;

        private static GameObject projectilePrefab;

        private const float PROJECTILE_DISTANCE_END = 4f;
        private const string PROJECTILE_PREFAB_PATH = "Other\\Projectile";

        /// <summary>
        /// Creates a projectile and launches at a specific unit.
        /// </summary>
        public static ProjectileInfo CreateAndLaunch(Vector3 start, string model, Quaternion direction, UnitInfo target, IProjectileEnd myInterface, ProjectileData projectileData, float speed, float arcAngle = 0f)
        {
            PrefabSetup();
            ProjectileInfo projectileInfo = GameObject.Instantiate(projectilePrefab, start, direction).GetComponent<ProjectileInfo>();
            projectileInfo.target = target;
            projectileInfo.StartProjectile(model, target.transform.position, myInterface, projectileData, speed, arcAngle);
            return projectileInfo;
        }
        /// <summary>
        /// Creates a projectile and launches at a specific point.
        /// </summary>
        public static ProjectileInfo CreateAndLaunch(Vector3 start, string model, Quaternion direction, Vector3 target, IProjectileEnd myInterface, ProjectileData projectileData, float speed, float arcAngle = 0f)
        {
            PrefabSetup();
            ProjectileInfo projectileInfo = GameObject.Instantiate(projectilePrefab, start, direction).GetComponent<ProjectileInfo>();
            projectileInfo.StartProjectile(model, target, myInterface, projectileData, speed, arcAngle);
            return projectileInfo;
        }
        /// <summary>
        /// Creates a projectile and launches in the direction for a while.
        /// </summary>
        public static ProjectileInfo CreateAndLaunch(Vector3 start, string model, Quaternion direction, float lifeTime, IProjectileEnd myInterface, ProjectileData projectileData, float speed)
        {
            PrefabSetup();
            ProjectileInfo projectileInfo = GameObject.Instantiate(projectilePrefab, start, direction).GetComponent<ProjectileInfo>();
            projectileInfo.StartProjectileWithTime(model, lifeTime, myInterface, projectileData, speed);
            return projectileInfo;
        }

        //Specifies information about the prefab when this method is first run.
        private static void PrefabSetup()
        {
            if (projectilePrefab == null)
            {
                projectilePrefab = (GameObject)Resources.Load(PROJECTILE_PREFAB_PATH);
                if (projectilePrefab == null)
                {
                    Debug.LogWarning("The projectile cannot be created. The path to the projectile prefab is corrupted. Please check PROJECTILE_PREFAB_PATH in ProjectileInfo.cs");
                    return;
                }
            }
        }

        private void SetModel(string model)
        {
            if (model != null && model != "")
            {
                //Set projectile model
            }
        }

        #region Target = Point
        private void StartProjectile(string model, Vector3 point, IProjectileEnd myInterface, ProjectileData projectileData, float speed, float arcAngle)
        {
            startPosition = transform.position;
            targetPosition = point;
            projectileEnd = myInterface;
            this.projectileData = projectileData;
            this.arcAngle = arcAngle;
            this.speed = Mathf.Clamp(speed, 0, 300);
            SetModel(model);
            StartCoroutine(Fly());
        }

        //Basic conditions necessary for projectile
        private bool ProjectileCondition()
        {
            if (!target?.IsUnitAlive() ?? false) return false;
            return true;
        }

        private Vector3 GetTargetPoint()
        {
            if (target != null)
            {
                return target.transform.position;
            }
            else
            {
                return targetPosition;
            }
        }

        //The projectile flies towards the target
        private IEnumerator Fly()
        {
            while (ProjectileCondition() && GlobalMethods.DistanceBetweenPoints(transform.position, GetTargetPoint()) > PROJECTILE_DISTANCE_END)
            {
                MoveProjectile();
                yield return null;
            }
            if (!ProjectileCondition())
            {
                Destroy(gameObject);
            }
            if (GlobalMethods.DistanceBetweenPoints(transform.position, GetTargetPoint()) <= PROJECTILE_DISTANCE_END)
            {
                projectileEnd?.ProjectileEnd(projectileData);
                Destroy(gameObject);
            }
        }

        //Moves the projectile towards the target
        //It needs to be optimized.
        private void MoveProjectile()
        {
            //Vector3 dir = GetTargetPoint() - transform.position;
            //transform.Translate(dir.normalized * speed * Time.deltaTime, Space.World);

            Vector3 targetPoint = GetTargetPoint();
            float step = speed * Time.deltaTime;

            transform.position = Parabola(startPosition, targetPoint, arcAngle, step);

            Vector3 targetDir = targetPoint - transform.position;
            Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0);
            transform.rotation = Quaternion.LookRotation(newDir);
        }

        //Returns the new position for the projectile.
        //It needs to be optimized.
        private Vector3 Parabola(Vector3 start, Vector3 end, float height, float step)
        {
            animationTime += step * Time.deltaTime;
            animationTime = animationTime % 5f;
            float f = animationTime / 5;
            float r = -4 * height * f * f + 4 * height * f;
            var mid = Vector3.Lerp(start, end, f);

            return new Vector3(mid.x, r + Mathf.Lerp(start.y, end.y, f), mid.z);
        }
        #endregion

        #region LifeTime
        private void StartProjectileWithTime(string model, float time, IProjectileEnd myInterface, ProjectileData projectileData, float speed)
        {
            this.projectileEnd = myInterface;
            this.projectileData = projectileData;
            this.lifeTime = time;
            this.speed = speed;
            SetModel(model);
            StartCoroutine(FlyTime());
        }

        private IEnumerator FlyTime()
        {
            while (lifeTime > 0)
            {
                transform.Translate(0, 0, speed * Time.deltaTime, Space.Self);
                lifeTime -= Time.deltaTime;
                yield return null;
            }
            projectileEnd?.ProjectileEnd(projectileData);
            Destroy(gameObject);
        }
        #endregion

        #region Projectile Cast
        /// <summary>
        /// Allows the projectile to pulse, which activates the method every interval seconds.
        /// </summary>
        public void SetProjectileCast(IProjectileCast projectileCast, float interval)
        {
            castInterval = interval;
            this.projectileCast = projectileCast;
            if (projectileCastCoroutine != null)
            {
                StopCoroutine(projectileCastCoroutine);
            }
            projectileCastCoroutine = StartCoroutine(Cast());
        }

        private IEnumerator Cast()
        {
            while (true)
            {
                yield return new WaitForSeconds(castInterval);
                projectileCast.ProjectileCast(projectileData, transform.position);
            }
        }
        #endregion
    }

    /// <summary>
    /// The method is triggered when the projectile reaches the desired target.
    /// </summary>
    public interface IProjectileEnd
    {
        void ProjectileEnd(ProjectileData projectileData);
    }

    /// <summary>
    /// The method fires every time the interval time passes. Use SetProjectileCast to trigger an interval method activating.
    /// </summary>
    public interface IProjectileCast
    {
        void ProjectileCast(ProjectileData projectileData, Vector3 projectilePoint);
    }

    /// <summary>
    /// This structure stores all data that can be changed during the flight of the projectile.
    /// Use this structure to keep the data as it was when the projectile was launched.
    /// </summary>
    public struct ProjectileData
    {
        public UnitInfo target;
        public float damage;
        public AttackType attackType;
        public Vector3 point;
    }

}
