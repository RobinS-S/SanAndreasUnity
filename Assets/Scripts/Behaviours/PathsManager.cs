using Assets.Scripts.Importing.Paths;
using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
	public class PathsManager : MonoBehaviour
	{
		public static PathsManager Instance { get; private set; }
		private static List<NodeFile> Nodes { get; set; }

		public static void AddNode(NodeFile node)
		{
			if (Nodes == null) Nodes = new List<NodeFile>();
			Nodes.Add(node);
		}


		public static void SpawnThem()
		{
			int peds = 0;
			Nodes.ForEach(n =>
			{
				peds += n.PathNodes.Count;
			});
			Debug.Log("peds to spawn:" + peds);
			Nodes[0].PathNodes.ForEach(node =>
				{
					Ped.SpawnPed(16, node.Position, new Quaternion(), false);
					Ped.Instance.Teleport(node.Position);
				});
		}
		
		void Awake()
		{
			Instance = this;
		}
	}
}
