using UnityEngine;

namespace KVD.ECS.GeneralTests
{
	public class ComponentsStorageTestsRunner : MonoBehaviour
	{
		void Update()
		{
			var tests = new ComponentsStorageTests();
			tests.Setup();
			tests.IsAlive_Inserted_Alive();
			tests.TearDown();
			Debug.LogError("Done!");
		}
	}
}
