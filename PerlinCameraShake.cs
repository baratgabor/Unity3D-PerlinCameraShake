using System.Collections;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Shakes the camera in an organic way, based on Perlin noise.
/// Supports any initial camera position and rotation, but camera should be steady, i.e. parented to another GameObject.
/// </summary>
[RequireComponent(typeof(Camera))]
public class PerlinCameraShake : MonoBehaviour
{
    //[SerializeField]
    //private FloatGameEvent _playerTrauma;

    [Header("Camera Shake Frequency:")]
    [SerializeField]
    [Tooltip("Defines the turbulence of the movement. Larger values cause more violent shaking.")]
    [Range(0, 50)]
    private float _frequency = 30f;

    [Header("Camera Movement:")]
    [SerializeField]
    [Tooltip("Specifies the maximum distance the camera can move from its original position.")]
    [Range(0, 10)]
    private float _translationMasterMagnitude = 1f;

    [Space(EDITOR_SPACING)]
    [SerializeField]
    private bool _translateAxisX = true;
    [SerializeField]
    private bool _translateAxisY = true;
    [SerializeField]
    private bool _translateAxisZ = false;

    [Space(EDITOR_SPACING)]
    [SerializeField]
    [Range(0, 1)]
    private float _translateAxisXMultiplier = 1f;
    [SerializeField]
    [Range(0, 1)]
    private float _translateAxisYMultiplier = 1f;
    [SerializeField]
    [Range(0, 1)]
    private float _translateAxisZMultiplier = 1f;

    [Header("Camera Rotation:")]
    [SerializeField]
    [Range(0, 40)]
    [Tooltip("Specifies the maximum rotational angle the camera can rotate from its original rotation.")]
    private float _rotationMasterMagnitude = 15;

    [Space(EDITOR_SPACING)]
    [SerializeField]
    private bool _rotateAxisX = true;
    [SerializeField]
    private bool _rotateAxisY = true;
    [SerializeField]
    private bool _rotateAxisZ = true;

    [Space(EDITOR_SPACING)]
    [SerializeField]
    [Range(0, 1)]
    private float _rotateAxisXMultiplier = 1f;
    [SerializeField]
    [Range(0, 1)]
    private float _rotateAxisYMultiplier = 1f;
    [SerializeField]
    [Range(0, 1)]
    private float _rotateAxisZMultiplier = 1f;

    [Header("Camera Shake Duration:")]
    [SerializeField]
    [Range(0.1f, 5)]
    [Tooltip("Specifies how fast the shaking will decay to zero.")]
    private float _decay = 2f;

    public float Trauma
    {
        get => _trauma;
        set => OnNewTrauma(value);
    }
    [Header("CURRENT TRAUMA (for testing):")]
    [SerializeField]
    [Range(0, 1)]
    private float _trauma = 0;

    private Camera _camera;
    private Vector3 _originalCamPosition;
    private Quaternion _originalCamRotation;

    private bool _shakeJobRunning = false;
    private float _sharedSamplingPoint;

    private const float VECTOR_EQUALITY_TOLERANCE = 0.001f;
    private const float DURATION_DECAY = 0.3f;
    private const float FREQUENCY_DECAY = 0.3f;
    private const float EDITOR_SPACING = 5f;

    // Unique seeds are important to ensure that no predictable patterns will emerge in the movement.
    // Also, using the same seed for the same translation/rotation ensures fluid motion.
    private const float TRANSLATION_X_SEED = 100;
    private const float TRANSLATION_Y_SEED = 200;
    private const float TRANSLATION_Z_SEED = 300;
    private const float ROTATION_X_SEED = 400;
    private const float ROTATION_Y_SEED = 500;
    private const float ROTATION_Z_SEED = 600;

#if UNITY_EDITOR 
    private void OnValidate()
    {
        if (GUI.changed && EditorApplication.isPlaying && _trauma > 0)
            OnNewTrauma(_trauma);
    }
#endif

    private void Start()
    {
        //if (_playerTrauma == null)
        //    throw new Exception("Component failed: GameEvent dependency not set.");

        _camera = GetComponent<Camera>();
        StoreCameraState();

        if (_trauma > 0)
            OnNewTrauma(_trauma);
    }

    //private void OnEnable()
    //    => _playerTrauma.Event += OnNewTrauma;

    //private void OnDisable()
    //    => _playerTrauma.Event -= OnNewTrauma;

    private void StoreCameraState()
    {
        _originalCamPosition = _camera.transform.localPosition;
        _originalCamRotation = _camera.transform.localRotation;
    }

    private void RestoreCameraState()
    {
        _camera.transform.localPosition = _originalCamPosition;
        _camera.transform.localRotation = _originalCamRotation;
    }

    private void OnNewTrauma(float value)
    {
        _trauma = Mathf.Clamp01(value);

        if (_trauma > 0 && !_shakeJobRunning && CanShake())
            StartCoroutine(ShakeJob());
    }

    /// <summary>
    /// Returns true if current settings actually call for a camera shake.
    /// </summary>
    private bool CanShake()
    {
        bool shouldTranslateX = _translateAxisX && _translateAxisXMultiplier > 0;
        bool shouldTranslateY = _translateAxisY && _translateAxisYMultiplier > 0;
        bool shouldTranslateZ = _translateAxisZ && _translateAxisZMultiplier > 0;

        bool shouldTranslateAnyAxis = shouldTranslateX || shouldTranslateY || shouldTranslateZ;

        if (_translationMasterMagnitude > 0 && shouldTranslateAnyAxis)
            return true;

        bool shouldRotateX = _rotateAxisX && _rotateAxisXMultiplier > 0;
        bool shouldRotateY = _rotateAxisY && _rotateAxisYMultiplier > 0;
        bool shouldRotateZ = _rotateAxisZ && _rotateAxisZMultiplier > 0;

        bool shouldRotateAnyAxis = shouldRotateX || shouldRotateY || shouldRotateZ;

        if (_rotationMasterMagnitude > 0 && shouldRotateAnyAxis)
            return true;

        return false;
    }

    /// <summary>
    /// Returns a Perlin noise float in the range of -1 and 1, based on a unique seed and a shared sampling point
    /// </summary>
    private float GetPerlin(float seed)
        => (Mathf.PerlinNoise(seed, _sharedSamplingPoint) - 0.5f) * 2f;

    /// <summary>
    /// Unity coroutine responsible for executing camera shake.
    /// Exits after trauma decays to zero, and camera is interpolated back to original position and rotation.
    /// </summary>
    private IEnumerator ShakeJob()
    {
        _shakeJobRunning = true;

        bool shouldTranslate = _translationMasterMagnitude > 0 && (_translateAxisX || _translateAxisY || _translateAxisZ);
        bool shouldRotate = _rotationMasterMagnitude > 0 && (_rotateAxisX || _rotateAxisY || _rotateAxisZ);

        // Prepare sampling point
        _sharedSamplingPoint = UnityEngine.Random.Range(0, 1000f);

        // Execute camera shake until trauma decays to zero
        ProcessTrauma:
        while (!Mathf.Approximately(_trauma, 0))
        {
            //TODO: Implement shake 'ramp-up', i.e. smooth out the transition to the first Perlin offset.

            // Offset sampling point
            _sharedSamplingPoint += Time.deltaTime * Mathf.Pow(_trauma, FREQUENCY_DECAY) * _frequency;

            // Set new position, relative to original position
            if (shouldTranslate)
            {
                Vector3 translationOffset = new Vector3(
                    _translateAxisX && _translateAxisXMultiplier > 0 ? GetOffset(TRANSLATION_X_SEED, _translateAxisXMultiplier) : 0,
                    _translateAxisY && _translateAxisYMultiplier > 0 ? GetOffset(TRANSLATION_Y_SEED, _translateAxisYMultiplier) : 0,
                    _translateAxisZ && _translateAxisZMultiplier > 0 ? GetOffset(TRANSLATION_Z_SEED, _translateAxisZMultiplier) : 0
                );

                _camera.transform.localPosition = translationOffset + _originalCamPosition;

                float GetOffset(float seed, float multiplier)
                    => GetPerlin(seed) * _translationMasterMagnitude * _trauma * multiplier;
            }

            // Set new rotation, relative to original rotation
            if (shouldRotate)
            {
                Vector3 rotationOffset = new Vector3(
                    _rotateAxisX && _rotateAxisXMultiplier > 0 ? GetOffset(ROTATION_X_SEED, _rotateAxisXMultiplier) : 0,
                    _rotateAxisY && _rotateAxisYMultiplier > 0 ? GetOffset(ROTATION_Y_SEED, _rotateAxisYMultiplier) : 0,
                    _rotateAxisZ && _rotateAxisZMultiplier > 0 ? GetOffset(ROTATION_Z_SEED, _rotateAxisZMultiplier) : 0
                );

                _camera.transform.localRotation = Quaternion.Euler(rotationOffset) * _originalCamRotation;

                float GetOffset(float seed, float multiplier)
                    => GetPerlin(seed) * _rotationMasterMagnitude * _trauma * multiplier;
            }

            // Decay trauma
            _trauma = Mathf.Clamp01(_trauma - (Time.deltaTime * _decay * (_trauma + DURATION_DECAY)));

            yield return null;
        }

        // Interpolate back to original state
        // - For the sake of simplicity only position equality is tested
        // - Rotation should be close enough too, without any visible jumps
        while (!ApproximatelyEqual(_camera.transform.localPosition, _originalCamPosition))
        {
            // If trauma was added meanwhile, reprocess
            if (_trauma != 0)
                goto ProcessTrauma;

            if (shouldTranslate)
                _camera.transform.localPosition = Vector3.MoveTowards(
                    current: _camera.transform.localPosition,
                    target: _originalCamPosition,
                    maxDistanceDelta: _translationMasterMagnitude * Time.deltaTime
                );

            if (shouldRotate)
                _camera.transform.localRotation = Quaternion.RotateTowards(
                    from: _camera.transform.localRotation,
                    to: _originalCamRotation,
                    maxDegreesDelta: _rotationMasterMagnitude * Time.deltaTime
                );

            yield return null;
        }

        // Ensure that camera state is restored precisely
        RestoreCameraState();

        _shakeJobRunning = false;
    }

    public bool ApproximatelyEqual(Vector3 firstVector, Vector3 secondVector)
    {
        var dx = firstVector.x - secondVector.x;
        if (Mathf.Abs(dx) > VECTOR_EQUALITY_TOLERANCE)
            return false;

        var dy = firstVector.y - secondVector.y;
        if (Mathf.Abs(dy) > VECTOR_EQUALITY_TOLERANCE)
            return false;

        var dz = firstVector.z - secondVector.z;
        if (Mathf.Abs(dz) > VECTOR_EQUALITY_TOLERANCE)
            return false;

        return true;
    }
}
