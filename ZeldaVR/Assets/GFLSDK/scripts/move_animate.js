    var CurveSpeed :float= 2;
    var MoveSpeed:float = 1;

 	public var move_x_dist=0.0;
	public var move_y_dist=0.0; 	 	
 	public var move_z_dist=0.0;
 
    var  fTime:float;
    var vLastPos:Vector3;
 
    // Use this for initialization
    function  Start () 
    {
    fTime=0;
    vLastPos= Vector3.zero;
       vLastPos = transform.position;
       Application.targetFrameRate = 60;
    }
 
    // Update is called once per frame
    function  FixedUpdate () 
    {
       vLastPos = transform.position;
 
       fTime += Time.deltaTime * CurveSpeed;
 	
       var vSin = Vector3( Mathf.Sin(fTime)*move_x_dist, Mathf.Sin(fTime)*move_y_dist, Mathf.Sin(fTime)*move_z_dist);
       //var vSin = Vector3( 1,1,1);
       var vLin = Vector3(MoveSpeed, MoveSpeed, 0);
 
      // transform.position += (vSin + vLin) * Time.deltaTime;
 	transform.position += (vSin ) * Time.deltaTime;
 	
 
       //Debug.DrawLine(vLastPos, transform.position, Color.green, 100);
       //Debug.Log (" MOVE: "+vSin);
    }
