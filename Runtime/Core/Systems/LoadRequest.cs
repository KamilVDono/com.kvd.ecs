using System.IO;
using UnityEngine;

namespace KVD.ECS.Core.Systems
{
	public class LoadRequest : MonoBehaviour
	{
		private BinaryReader Reader{ get; set; }

		public void SetRequest(BinaryReader reader)
		{
			Reader = reader;
		}

		public BinaryReader Consume()
		{
			var reader = Reader;
			Reader = null;
			Destroy(gameObject);
			return reader;
		}
	}
} 