п»їusing System.Collections;

using UnityEngine;

/// <summary>
/// РћРїРµСЂР°С‚РѕСЂ РєР°РјРµСЂС‹
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

    private const float DISTANCE_FOR_CHANGE_TYPE_LIMIT = 0.1f;
    private const int HEIGHT = -10;
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
            CamTr.position = new Vector3(TargetTr.position.x, TargetTr.position.y, HEIGHT);
    }

    private void Update() => CameraControler();

    private void NormalizePos(Transform trNorm, float height = HEIGHT) => trNorm.position = new Vector3(trNorm.position.x, trNorm.position.y, height);

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
    /// РљСѓСЂРѕС‚РёРЅ С‚СЂСЏСЃРєРё РєР°РјРµСЂС‹
    /// </summary>
    /// <param name="duration">РґР»РёС‚РµР»СЊРЅРѕСЃС‚СЊ С‚СЂСЏСЃРєРё</param>
    /// <param name="magnitude">СЂР°СЃСЃС‚РѕСЏРЅРёРµ РѕС‚ СЃРѕСЃС‚РѕСЏРЅРёСЏ РїРѕРєРѕСЏ</param>
    /// <param name="noize">СЃРёР»Р° С‚СЂСЏСЃРєРё</param>
    private IEnumerator ShakeCameraCoroutine(float duration, float magnitude, float noize)
    {
        //Р�РЅРёС†РёР°Р»РёР·РёСЂСѓРµРј СЃС‡С‘С‚С‡РёРєРѕРІ РїСЂРѕС€РµРґС€РµРіРѕ РІСЂРµРјРµРЅРё
        float elapsed = 0f;
        //Р“РµРЅРµСЂРёСЂСѓРµРј РґРІРµ С‚РѕС‡РєРё РЅР° "С‚РµРєСЃС‚СѓСЂРµ" С€СѓРјР° РџРµСЂР»РёРЅР°
        Vector2 noizeStartPoint0 = UnityEngine.Random.insideUnitCircle * noize;
        Vector2 noizeStartPoint1 = UnityEngine.Random.insideUnitCircle * noize;

        //Р’С‹РїРѕР»РЅСЏРµРј РєРѕРґ РґРѕ С‚РµС… РїРѕСЂ РїРѕРєР° РЅРµ РёСЃСЃСЏРєРЅРµС‚ РІСЂРµРјСЏ
        while (elapsed < duration)
        {
            //Р“РµРЅРµСЂРёСЂСѓРµРј РґРІРµ РѕС‡РµСЂРµРґРЅС‹Рµ РєРѕРѕСЂРґРёРЅР°С‚С‹ РЅР° С‚РµРєСЃС‚СѓСЂРµ РџРµСЂР»РёРЅР° РІ Р·Р°РІРёСЃРёРјРѕСЃС‚Рё РѕС‚ РїСЂРѕС€РµРґС€РµРіРѕ РІСЂРµРјРµРЅРё
            Vector2 currentNoizePoint0 = Vector2.Lerp(noizeStartPoint0, Vector2.zero, elapsed / duration);
            Vector2 currentNoizePoint1 = Vector2.Lerp(noizeStartPoint1, Vector2.zero, elapsed / duration);
            //РЎРѕР·РґР°С‘Рј РЅРѕРІСѓСЋ РґРµР»СЊС‚Сѓ РґР»СЏ РєР°РјРµСЂС‹ Рё СѓРјРЅРѕР¶Р°РµРј РµС‘ РЅР° РґР»РёРЅСѓ РґР°Р±С‹ СѓС‡РµСЃС‚СЊ Р¶РµР»Р°РµРјС‹Р№ СЂР°Р·Р±СЂРѕСЃ
            Vector2 cameraPostionDelta = new(Mathf.PerlinNoise(currentNoizePoint0.x, currentNoizePoint0.y), Mathf.PerlinNoise(currentNoizePoint1.x, currentNoizePoint1.y));
            cameraPostionDelta *= magnitude * (duration - elapsed);

            //РџРµСЂРµРјРµС‰Р°РµРј РєР°РјРµСЂСѓ РІ РЅСѓРІСѓСЋ РєРѕРѕСЂРґРёРЅР°С‚Сѓ
            CamTr.localPosition += (Vector3)cameraPostionDelta;

            //РЈРІРµР»РёС‡РёРІР°РµРј СЃС‡С‘С‚С‡РёРє РїСЂРѕС€РµРґС€РµРіРѕ РІСЂРµРјРµРЅРё
            elapsed += Time.deltaTime;
            //РџСЂРёРѕСЃС‚Р°РЅР°РІР»РёРІР°РµРј РІС‹РїРѕР»РЅРµРЅРёРµ РєРѕСЂСѓС‚РёРЅС‹, РІ СЃР»РµРґСѓСЋС‰РµРј РєР°РґСЂРµ РѕРЅР° РїСЂРѕРґРѕР»Р¶РёС‚ РІС‹РїРѕР»РЅРµРЅРёРµ СЃ РґР°РЅРЅРѕР№ С‚РѕС‡РєРё
            yield return null;
        }
    }

    public void GoToTarget()
    {
        if (TargetTr == null)
            return;
        CamTr.position = new Vector3(TargetTr.position.x, TargetTr.position.y, HEIGHT);
        limit = Limits.circle;
    }

    public void TargetPos(Transform tr, bool changeTarget = false)
    {
        if (tr == null)
            return;
        TargetTr = tr;
        if (!changeTarget)
            CamTr.position = new Vector3(TargetTr.position.x, TargetTr.position.y, HEIGHT);
    }

    /// <summary>
    /// РўСЂСЏСЃРєР° РєР°РјРµСЂС‹
    /// </summary>
    /// <param name="duration">РґР»РёС‚РµР»СЊРЅРѕСЃС‚СЊ С‚СЂСЏСЃРєРё</param>
    /// <param name="magnitude">СЂР°СЃСЃС‚РѕСЏРЅРёРµ РѕС‚ СЃРѕСЃС‚РѕСЏРЅРёСЏ РїРѕРєРѕСЏ</param>
    /// <param name="noize">СЃРёР»Р° С‚СЂСЏСЃРєРё</param>
    public void ShakeCamera(float duration, float magnitude, float noize) => StartCoroutine(ShakeCameraCoroutine(duration, magnitude * 5, noize * 800));

    /// <summary>
    /// РўСЂСЏСЃРєР° РєР°РјРµСЂС‹
    /// </summary>
    /// <param name="duration">РґР»РёС‚РµР»СЊРЅРѕСЃС‚СЊ С‚СЂСЏСЃРєРё</param>
    /// <param name="magnitude">СЂР°СЃСЃС‚РѕСЏРЅРёРµ РѕС‚ СЃРѕСЃС‚РѕСЏРЅРёСЏ РїРѕРєРѕСЏ</param>
    /// <param name="noize">СЃРёР»Р° С‚СЂСЏСЃРєРё</param>
    /// <param name="point">РёС‚РѕС‡РЅРёРє С‚СЂСЏСЃРєРё</param>
    /// <param name="maxForce">РјР°РєСЃРёРјР°Р»СЊРЅРѕ РІРѕР·РјРѕР¶РЅР°СЏ С‚СЂСЏСЃРєР°</param>
    public void ShakeCamera(float duration, float magnitude, float noize, Vector2 point, float maxForce = 3)
    {
        float distance = Vector2.Distance(CamTr.position, point);
        float force = Mathf.Min(100 / distance, maxForce);
        _ = StartCoroutine(ShakeCameraCoroutine(duration, force * magnitude, force * noize));
    }

    #endregion Methods
}