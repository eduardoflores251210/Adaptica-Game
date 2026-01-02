using System;
using UnityEngine;

public class Metaball : MonoBehaviour //No confundir con MeatBall
{
    public float Radius = 1f;
    public bool registered = false;
    private void Start()
    {
        transform.localScale = new Vector3 (1,0.5f,1)* Radius;
        try
        {
            if (MetaballManager.Instance == null)
                throw new NullReferenceException();
            MetaballManager.Instance.RegisterMetaball(this);
            registered = true;
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }
    private void Update()
    {
        if (!registered)
        {
            try
            {
				if (MetaballManager.Instance == null)
					throw new NullReferenceException();
				MetaballManager.Instance.RegisterMetaball(this);
                registered = true;
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
    private void OnDestroy()
    {
        MetaballManager.Instance?.UnregisterMetaball(this);
    }
}
