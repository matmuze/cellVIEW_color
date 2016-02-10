using UnityEngine;

[ExecuteInEditMode]
public class ColorTest : MonoBehaviour
{
    [Range(0, 360)]
    public double hue;

    [Range(0f, 1f)]
    public double saturation;

    [Range(0f, 1f)]
    public double brightness;

	// Use this for initialization
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
	    GetComponent<MeshRenderer>().sharedMaterial.color = MyUtility.ColorFromHSV(hue, saturation, brightness);
    }

    
}
