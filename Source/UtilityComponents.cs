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
		public Quaternion RotationLock;
		
		void Update()
		{
			transform.rotation = RotationLock;
		}
		void FixedUpdate()
		{
			transform.rotation = RotationLock;
		}
		void LateUpdate()
		{
			transform.rotation = RotationLock;
		}
	}
	
	public class ReScaler : MonoBehaviour
	{	
		void Update()
		{
			transform.localScale = transform.parent.localScale;
		}
		void FixedUpdate()
		{
			transform.localScale = transform.parent.localScale;
		}
		void LateUpdate()
		{
			transform.localScale = transform.parent.localScale;
		}
	}
}

