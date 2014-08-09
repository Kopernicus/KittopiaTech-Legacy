using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;
using KSP.IO;

namespace PFUtilityAddon
{
	public class AngleLocker : MonoBehaviour
	{	
		void Update()
		{
			transform.localRotation = Quaternion.Euler(transform.localRotation.x ,0 , 0);
		}
		void FixedUpdate()
		{
			transform.localRotation = Quaternion.Euler(transform.localRotation.x ,0 , 0);
		}
		void LateUpdate()
		{
			transform.localRotation = Quaternion.Euler(transform.localRotation.x ,0 , 0);
		}
	}
}

