// ***********************************************************
// Written by Huiming Unity Studio
// ***********************************************************
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Gyroscope controller that works with any device orientation.
/// </summary>
public class GFLCamera_Zelda : MonoBehaviour 
{
	#region [Private fields]

	private bool gyroEnabled = true;
	private const float lowPassFilterFactor = 0.2f;

	private readonly Quaternion baseIdentity =  Quaternion.Euler(90, 0, 0);
	private readonly Quaternion landscapeRight =  Quaternion.Euler(0, 0, 90);
	private readonly Quaternion landscapeLeft =  Quaternion.Euler(0, 0, -90);
	private readonly Quaternion upsideDown =  Quaternion.Euler(0, 0, 180);
	
	private Quaternion cameraBase =  Quaternion.identity;
	private Quaternion calibration =  Quaternion.identity;
	private Quaternion baseOrientation =  Quaternion.Euler(90, 0, 0);
	private Quaternion baseOrientationRotationFix =  Quaternion.identity;

	private Quaternion referanceRotation = Quaternion.identity;
	private bool debug = true;

	#endregion
	
	#region [Public fields]

	public bool 			inputJoystickHeading = true;
	public Vector3			delta = Vector3.zero;
	public float			rotationX = 0;
	public float			rotationY = 0;
	public Quaternion		xQuaternion;
	public Quaternion		yQuaternion;
	public GameObject		camGrandParent;
	public Quaternion		quatMult = Quaternion.identity;
	public float			limitAngle = 70;
	public float			rotateSpeed = 50;

    public float            IPD = 0.06f;        // TODO
    public Transform        cameraRight;

	#endregion
	
	#region [Unity events]

	protected void Start () 
	{
		AttachGyro();
		camGrandParent = new GameObject("Grand Camera");
		camGrandParent.transform.position = transform.position;
		camGrandParent.transform.parent = transform.parent;
		transform.parent = camGrandParent.transform;
		Input.gyro.enabled = true;
	}

	protected void Update() 
	{

        if (ZeldaInput.GetButtonDown(ZeldaInput.Button.Extra))
        {
            gyroEnabled = !gyroEnabled;
        }

//Added
		if(inputJoystickHeading)
		{
            if (gyroEnabled)
            {
                transform.localRotation = Quaternion.Slerp(transform.localRotation,
                    cameraBase * (ConvertRotation(referanceRotation * Input.gyro.attitude) * quatMult), lowPassFilterFactor);
            }
            else
            {
                transform.localRotation = Quaternion.identity;
            }

			//delta = new Vector3( Input.GetAxis("Rotate X"), Input.GetAxis("Rotate Y"), 0);
            delta = new Vector3(ZeldaInput.GetAxis(ZeldaInput.Axis.LookHorizontal), 0, 0);

			if(delta != Vector3.zero)
			{
			   rotationX += delta.x * Time.deltaTime * rotateSpeed;
			   rotationY = delta.y * Time.deltaTime * rotateSpeed;
			   rotationX = rotationX % 360;
			   //rotationY = ClampAngle(rotationY, -limitAngle, limitAngle);
			   
			   xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
			   yQuaternion = Quaternion.AngleAxis(rotationY, Vector3.right);
			   		   
			   camGrandParent.transform.localRotation = Quaternion.Lerp(camGrandParent.transform.localRotation, xQuaternion, Time.deltaTime * 30);
			   if(Mathf.Abs(RealAngle(transform.eulerAngles.x)) <= limitAngle)
			   {
				    quatMult = Quaternion.Slerp(quatMult, quatMult * yQuaternion, Time.deltaTime * 30);
				    transform.localRotation = Quaternion.Lerp(transform.localRotation, transform.localRotation * yQuaternion, Time.deltaTime * 30);
			   }
			   transform.eulerAngles = new Vector3(Mathf.Clamp(RealAngle(transform.eulerAngles.x), -limitAngle, limitAngle),
													transform.eulerAngles.y, transform.eulerAngles.z);
			}
		}

		/*if(Input.GetButton("Home"))
		{
			Application.Quit();
		}*/
//End added
	}

	/*protected void OnGUI()
	{
		if (!debug)
			return;
	}*/

	#endregion

	#region [Public methods]

    public Vector3 LineOfSight { get { return cameraRight.transform.forward; } }

    public bool AttachGameObjectToCamera(ref GameObject gameObject)
    {
        if (cameraRight == null)
            return false;

        gameObject.transform.parent = cameraRight.transform;

        return true;
    }

    public void GetIPD(ref float ipd)
    {
        ipd = IPD;
    }

	float RealAngle(float angle)
	{
		if(angle > 180)
			return angle-360;
		return angle;
	}
	
	/// <summary>
	/// Attaches gyro controller to the transform.
	/// </summary>
	private void AttachGyro()
	{
		gyroEnabled = true;
		ResetBaseOrientation();
		UpdateCalibration(true);
		UpdateCameraBaseRotation(true);
		RecalculateReferenceRotation();
	}

	/// <summary>
	/// Detaches gyro controller from the transform
	/// </summary>
	private void DetachGyro()
	{
		gyroEnabled = false;
	}

	#endregion

	#region [Private methods]

	/// <summary>
	/// Update the gyro calibration.
	/// </summary>
	private void UpdateCalibration(bool onlyHorizontal)
	{
		if (onlyHorizontal)
		{
			var fw = (Input.gyro.attitude) * (-Vector3.forward);
			fw.z = 0;
			if (fw == Vector3.zero)
			{
				calibration = Quaternion.identity;
			}
			else
			{
				calibration = (Quaternion.FromToRotation(baseOrientationRotationFix * Vector3.up, fw));
			}
		}
		else
		{
			calibration = Input.gyro.attitude;
		}
	}
	
	/// <summary>
	/// Update the camera base rotation.
	/// </summary>
	/// <param name='onlyHorizontal'>
	/// Only y rotation.
	/// </param>
	private void UpdateCameraBaseRotation(bool onlyHorizontal)
	{
		if (onlyHorizontal)
		{
			var fw = transform.forward;
			fw.y = 0;
			if (fw == Vector3.zero)
			{
				cameraBase = Quaternion.identity;
			}
			else
			{
				cameraBase = Quaternion.FromToRotation(Vector3.forward, fw);
			}
		}
		else
		{
			cameraBase = transform.rotation;
		}
	}
	
	/// <summary>
	/// Converts the rotation from right handed to left handed.
	/// </summary>
	/// <returns>
	/// The result rotation.
	/// </returns>
	/// <param name='q'>
	/// The rotation to convert.
	/// </param>
	private static Quaternion ConvertRotation(Quaternion q)
	{
		return new Quaternion(q.x, q.y, -q.z, -q.w);	
	}
	
	/// <summary>
	/// Gets the rot fix for different orientations.
	/// </summary>
	/// <returns>
	/// The rot fix.
	/// </returns>
	private Quaternion GetRotFix()
	{
#if UNITY_3_5
		if (Screen.orientation == ScreenOrientation.Portrait)
			return Quaternion.identity;
		
		if (Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.Landscape)
			return landscapeLeft;
				
		if (Screen.orientation == ScreenOrientation.LandscapeRight)
			return landscapeRight;
				
		if (Screen.orientation == ScreenOrientation.PortraitUpsideDown)
			return upsideDown;
		return Quaternion.identity;
#else
		return Quaternion.identity;
#endif
	}
	
	/// <summary>
	/// Recalculates reference system.
	/// </summary>
	private void ResetBaseOrientation()
	{
		baseOrientationRotationFix = GetRotFix();
		baseOrientation = baseOrientationRotationFix * baseIdentity;
	}

	/// <summary>
	/// Recalculates reference rotation.
	/// </summary>
	private void RecalculateReferenceRotation()
	{
		referanceRotation = Quaternion.Inverse(baseOrientation)*Quaternion.Inverse(calibration);
	}

	#endregion
}
