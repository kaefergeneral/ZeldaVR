using UnityEngine;


public class StereoscopicGUI : MonoBehaviour
{

    public float zOffset = 0.0f;


    OVRCameraController _ovrCameraController = null;
    GFLCamera_Zelda _gflCameraController = null;

    OVRGUI _guiHelper = new OVRGUI();
    GameObject _guiRenderObject = null;
    RenderTexture _guiRenderTexture = null;

    float _renderObjLocalZStartValue;


    //public OVRCameraController CameraController { get { return _ovrCameraController; } }
    public OVRGUI GuiHelper { get { return _guiHelper; } }
    public GameObject GuiRenderObject { get { return _guiRenderObject; } }
    public RenderTexture GuiRenderTexture { get { return _guiRenderTexture; } }


    public void ActivateRenderObject(bool activate = true)
    {
        _guiRenderObject.SetActive(activate);
    }

    void Awake()
    {
        if (ZeldaConfig.Instance.isGflBuild)
        {
            InitGFLCamera();
        }
        else
        {
            InitOVRCamera();
        }
    }

    void InitOVRCamera()
    {
        GameObject g = GameObject.Find("OVRCameraController");
        if (g == null)
        {
            Debug.LogWarning("StereoscopicGUI: Couldn't find an GameObject named OVRCameraController.");
        }
        else
        {
            _ovrCameraController = g.GetComponent<OVRCameraController>();
            if (_ovrCameraController == null)
            {
                Debug.LogWarning("StereoscopicGUI: Couldn't find an OVRCameraController.");
            }
        }
    }

    void InitGFLCamera()
    {
        GameObject g = GameObject.Find("GFL_Camera_Zelda");
        _gflCameraController = g.GetComponent<GFLCamera_Zelda>();
    }

	void Start ()
    {
        // Ensure that camera controller variables have been properly initialized before we start reading them
        if (!ZeldaConfig.Instance.isGflBuild)
        {
            if (_ovrCameraController != null)
            {
                _ovrCameraController.InitCameraControllerVariables();
                _guiHelper.SetCameraController(ref _ovrCameraController);
            }
        }

        float vertScaleAmount = 1.8f;

        // Set the GUI target 
        _guiRenderObject = GameObject.Instantiate(Resources.Load("OVRGUIObjectMain")) as GameObject;

        if (_guiRenderObject != null)
        {
            if (_guiRenderTexture == null)
            {
                int w = Screen.width;
                int h = Screen.height;

                if (_ovrCameraController != null && _ovrCameraController.PortraitMode == true)
                {
                    int t = h;
                    h = w;
                    w = t;
                }

                // We don't need a depth buffer on this texture
                _guiRenderTexture = new RenderTexture(w, h, 0);
                _guiHelper.SetPixelResolution(w, h * vertScaleAmount);
                _guiHelper.SetDisplayResolution(OVRDevice.HResolution, OVRDevice.VResolution);
            }
        }

        // Attach GUI texture to GUI object and GUI object to Camera
        if (_guiRenderTexture != null && _guiRenderObject != null)
        {
            _guiRenderObject.renderer.material.mainTexture = _guiRenderTexture;

            //if (_ovrCameraController != null)
            {
                Transform t = _guiRenderObject.transform;
                if (ZeldaConfig.Instance.isGflBuild)
                {
                    _gflCameraController.AttachGameObjectToCamera(ref _guiRenderObject);
                }
                else
                {
                    _ovrCameraController.AttachGameObjectToCamera(ref _guiRenderObject);
                }

                // Reset the transform values (we will be maintaining state of the GUI object in local state)
                OVRUtils.SetLocalTransform(ref _guiRenderObject, ref t); 

                // We will move the position of everything over to the left, so get IPD / 2 and position camera towards negative X
                Vector3 lp = _guiRenderObject.transform.localPosition;
                float ipd = 0.0f;
                if (ZeldaConfig.Instance.isGflBuild)
                {
                    _gflCameraController.GetIPD(ref ipd);
                }
                else
                {
                    _ovrCameraController.GetIPD(ref ipd);
                }
                lp.x -= ipd * 0.5f;
                _guiRenderObject.transform.localPosition = lp;

                // Note:  You may want to deactive the render object if there is nothing being rendered into the UI
                ActivateRenderObject();
            }
        }

        ApplyVerticalScaling(vertScaleAmount);

        _renderObjLocalZStartValue = _guiRenderObject.transform.localPosition.z;
	}

    void ApplyVerticalScaling(float amount)
    {
        Transform t = _guiRenderObject.transform;
        Vector3 s = t.localScale;
        s.z *= amount;
        t.localScale = s;

        t.AddToLocalZ(0.3f);
        //t.AddToLocalY(-0.6f);
    }

    void Update()
    {
        _guiRenderObject.transform.SetLocalZ(_renderObjLocalZStartValue + zOffset);
    }

    void OnGUI()
    {
        // Important to keep from skipping render events
        if (Event.current.type != EventType.Repaint)
            return;

        /*if (_alphaFadeValue > 0.0f)
        {
            FadeInScreen();
            return;
        }*/

        // Set the GUI matrix to deal with portrait mode
        Vector3 scale = Vector3.one;
        if (_ovrCameraController != null && _ovrCameraController.PortraitMode == true)
        {
            float h = OVRDevice.HResolution;
            float v = OVRDevice.VResolution;
            scale.x = v / h; 					// calculate hor scale
            scale.y = h / v; 					// calculate vert scale
        }
        Matrix4x4 savedMat = GUI.matrix; // save current matrix
        // Substitute matrix - only scale is altered from standard
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
        RenderTexture previousActive = RenderTexture.active;    // Cache current active render texture
        {
            // If set, we will render to this texture
            if (_guiRenderTexture != null)
            {
                RenderTexture.active = _guiRenderTexture;
                GL.Clear(false, true, new Color(0.0f, 0.0f, 0.0f, 0.0f));
            }

            // Update OVRGUI functions (will be deprecated eventually when 2D rendering is removed from GUI)
            //GuiHelper.SetFontReplace(FontReplace);

            gameObject.SendMessage("OnStereoscopicGUI", this, SendMessageOptions.RequireReceiver);
        }
        RenderTexture.active = previousActive;  // Restore active render texture
        GUI.matrix = savedMat;                  // Restore previous GUI matrix
    }


    public static Texture CreateBlackTexture()
    {
        return CreateColoredTexture(Color.black);
    }
    public static Texture CreateColoredTexture(Color color)
    {
        Texture2D tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return tex;
    }


    /*float _alphaFadeValue = 1.0f;
    void FadeInScreen()
    {
        _alphaFadeValue -= Mathf.Clamp01(Time.deltaTime / FadeInTime);
        if (_alphaFadeValue < 0.0f)
        {
            _alphaFadeValue = 0.0f;
        }
            
        GUI.color = new Color(0, 0, 0, _alphaFadeValue);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), FadeInTexture);
    }*/
}
