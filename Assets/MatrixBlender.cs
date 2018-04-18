using UnityEngine;
using System.Collections;
 
[RequireComponent (typeof(Camera))]
public class MatrixBlender : MonoBehaviour
{

	public Camera mainCam;

	void Start() {
		mainCam = GameObject.FindWithTag ("MainCamera").GetComponent<Camera> ();
	}

	public static Matrix4x4 MatrixLerp(Matrix4x4 from, Matrix4x4 to, float time)
	{
	        Matrix4x4 ret = new Matrix4x4();
	        for (int i = 0; i < 16; i++)
				ret[i] = Mathf.Lerp(from[i], to[i], time);
	        return ret;
	    }

	private IEnumerator LerpFromTo(Matrix4x4 src, Matrix4x4 dest, float duration)
	{
	        float startTime = Time.time;
	        while (Time.time - startTime < duration)
		        {
		            mainCam.projectionMatrix = MatrixLerp(src, dest, (Time.time - startTime) / duration);
		            yield return 1;
		        }
	        mainCam.projectionMatrix = dest;
	    }

	public Coroutine BlendToMatrix(Matrix4x4 targetMatrix, float duration)
	{
	        StopAllCoroutines();
	        return StartCoroutine(LerpFromTo(mainCam.projectionMatrix, targetMatrix, duration));
	    }
}