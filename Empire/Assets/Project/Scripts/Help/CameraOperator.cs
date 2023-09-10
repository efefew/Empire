using System.Collections;

using UnityEngine;

/// <summary>
/// Оператор камеры
/// </summary>
public class CameraOperator : MonoBehaviour
{
    #region Enums

    public enum Limits
    {
        square,
        circle,
        point
    }

    #endregion Enums

    #region Fields

    private const float distanceForChangeTypeLimit = 0.1f;
    private const int Z = -10;
    public float SpeedScale, SpeedMove, MaxRadius, MinZoom, MaxZoom;
    public Transform CamTr, TargetTr;
    public Camera MyCam;
    public float timeSpeed, whenChangeTiledImage;
    public Limits limit;

    #endregion Fields

    #region Methods

    private void OnEnable()
    {
        if (TargetTr != null)
            CamTr.position = new Vector3(TargetTr.position.x, TargetTr.position.y, Z);
    }

    private void Update() => CameraControler();

    private void NormalizePos(Transform trNorm, float height = Z) => trNorm.position = new Vector3(trNorm.position.x, trNorm.position.y, height);

    private void SquareLimit()
    {
        Move();
        if (TargetTr)
        {
            if (CamTr.position.x > TargetTr.position.x + MaxRadius)
                CamTr.position = new Vector3(TargetTr.position.x + MaxRadius, CamTr.position.y, CamTr.position.z);
            if (CamTr.position.x < TargetTr.position.x - MaxRadius)
                CamTr.position = new Vector3(TargetTr.position.x - MaxRadius, CamTr.position.y, CamTr.position.z);
            if (CamTr.position.y > TargetTr.position.y + MaxRadius)
                CamTr.position = new Vector3(CamTr.position.x, TargetTr.position.y + MaxRadius, CamTr.position.z);
            if (CamTr.position.y < TargetTr.position.y - MaxRadius)
                CamTr.position = new Vector3(CamTr.position.x, TargetTr.position.y - MaxRadius, CamTr.position.z);
        }
    }

    private void CircleLimit()
    {
        Move();
        if (TargetTr)
        {
            Vector2 position = CamTr.position - TargetTr.position;
            float rot = CamTr.eulerAngles.z;
            if (Vector2.Distance(CamTr.position, TargetTr.position) > MaxRadius)
            {
                Vector3 relative = CamTr.InverseTransformPoint(TargetTr.position);
                CamTr.position = TargetTr.position;
                CamTr.eulerAngles = new Vector3(0, 0, -Mathf.Atan2(relative.x, relative.y) * Mathf.Rad2Deg);
                CamTr.position -= CamTr.up * MaxRadius;
                NormalizePos(CamTr);
                CamTr.eulerAngles = new Vector3(0, 0, rot);
            }
        }
    }

    private void PointLimit()
    {
        if (TargetTr)
        {
            CamTr.position = Vector3.Lerp(CamTr.position, TargetTr.position, timeSpeed);
            NormalizePos(CamTr);
        }
    }

    private void Move()
    {
        if (Input.GetKey(KeyCode.D))
            CamTr.position += SpeedMove * CamTr.right * (MyCam.orthographicSize / MaxZoom);

        if (Input.GetKey(KeyCode.A))
            CamTr.position -= SpeedMove * CamTr.right * (MyCam.orthographicSize / MaxZoom);

        if (Input.GetKey(KeyCode.W))
            CamTr.position += SpeedMove * CamTr.up * (MyCam.orthographicSize / MaxZoom);

        if (Input.GetKey(KeyCode.S))
            CamTr.position -= SpeedMove * CamTr.up * (MyCam.orthographicSize / MaxZoom);
        if (Input.GetKey(KeyCode.Mouse1))
        {
            CamTr.position -= CamTr.up * Input.GetAxis("Mouse Y") * SpeedMove * MyCam.orthographicSize / MaxZoom;
            CamTr.position -= CamTr.right * Input.GetAxis("Mouse X") * SpeedMove * MyCam.orthographicSize / MaxZoom;
        }
    }

    private void Zoom()
    {
        if (/*(AndroidInput.touchCountSecondary > 1 && AndroidInput.GetSecondaryTouch(1).range < 0) || */(Input.GetKey(KeyCode.E) && !(limit == Limits.point)) || Input.GetAxis("Mouse ScrollWheel") >= 0.1)
        {
            MyCam.orthographicSize -= SpeedScale * (MyCam.orthographicSize / MaxZoom);
            if (MyCam.orthographicSize < MinZoom)
                MyCam.orthographicSize = MinZoom;
        }

        if (/*(AndroidInput.touchCountSecondary > 1 && AndroidInput.GetSecondaryTouch(1).range > 0) || */(Input.GetKey(KeyCode.Q) && !(limit == Limits.point)) || Input.GetAxis("Mouse ScrollWheel") <= -0.1)
        {
            MyCam.orthographicSize += SpeedScale * (MyCam.orthographicSize / MaxZoom);
            if (MyCam.orthographicSize > MaxZoom)
                MyCam.orthographicSize = MaxZoom;
        }
    }

    private void CameraControler()
    {
        Zoom();
        switch (limit)
        {
            case Limits.square:
                SquareLimit();
                break;

            case Limits.circle:
                CircleLimit();
                break;

            case Limits.point:
                PointLimit();
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// Куротин тряски камеры
    /// </summary>
    /// <param name="duration">длительность тряски</param>
    /// <param name="magnitude">расстояние от состояния покоя</param>
    /// <param name="noize">сила тряски</param>
    private IEnumerator ShakeCameraCoroutine(float duration, float magnitude, float noize)
    {
        //Инициализируем счётчиков прошедшего времени
        float elapsed = 0f;
        //Генерируем две точки на "текстуре" шума Перлина
        Vector2 noizeStartPoint0 = UnityEngine.Random.insideUnitCircle * noize;
        Vector2 noizeStartPoint1 = UnityEngine.Random.insideUnitCircle * noize;

        //Выполняем код до тех пор пока не иссякнет время
        while (elapsed < duration)
        {
            //Генерируем две очередные координаты на текстуре Перлина в зависимости от прошедшего времени
            Vector2 currentNoizePoint0 = Vector2.Lerp(noizeStartPoint0, Vector2.zero, elapsed / duration);
            Vector2 currentNoizePoint1 = Vector2.Lerp(noizeStartPoint1, Vector2.zero, elapsed / duration);
            //Создаём новую дельту для камеры и умножаем её на длину дабы учесть желаемый разброс
            Vector2 cameraPostionDelta = new(Mathf.PerlinNoise(currentNoizePoint0.x, currentNoizePoint0.y), Mathf.PerlinNoise(currentNoizePoint1.x, currentNoizePoint1.y));
            cameraPostionDelta *= magnitude * (duration - elapsed);

            //Перемещаем камеру в нувую координату
            CamTr.localPosition += (Vector3)cameraPostionDelta;

            //Увеличиваем счётчик прошедшего времени
            elapsed += Time.deltaTime;
            //Приостанавливаем выполнение корутины, в следующем кадре она продолжит выполнение с данной точки
            yield return null;
        }
    }

    public void GoToTarget()
    {
        if (TargetTr == null)
            return;
        CamTr.position = new Vector3(TargetTr.position.x, TargetTr.position.y, Z);
        limit = Limits.circle;
    }

    public void TargetPos(Transform tr, bool changeTarget = false)
    {
        if (tr == null)
            return;
        TargetTr = tr;
        if (!changeTarget)
            CamTr.position = new Vector3(TargetTr.position.x, TargetTr.position.y, Z);
    }

    /// <summary>
    /// Тряска камеры
    /// </summary>
    /// <param name="duration">длительность тряски</param>
    /// <param name="magnitude">расстояние от состояния покоя</param>
    /// <param name="noize">сила тряски</param>
    public void ShakeCamera(float duration, float magnitude, float noize) => StartCoroutine(ShakeCameraCoroutine(duration, magnitude * 5, noize * 800));

    /// <summary>
    /// Тряска камеры
    /// </summary>
    /// <param name="duration">длительность тряски</param>
    /// <param name="magnitude">расстояние от состояния покоя</param>
    /// <param name="noize">сила тряски</param>
    /// <param name="point">иточник тряски</param>
    /// <param name="maxForce">максимально возможная тряска</param>
    public void ShakeCamera(float duration, float magnitude, float noize, Vector2 point, float maxForce = 3)
    {
        float distance = Vector2.Distance(CamTr.position, point);
        float force = Mathf.Min(100 / distance, maxForce);
        _ = StartCoroutine(ShakeCameraCoroutine(duration, force * magnitude, force * noize));
    }

    #endregion Methods
}