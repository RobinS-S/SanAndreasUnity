using Assets.Scripts.Importing.Paths;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
	public class PathsManager : MonoBehaviour
	{
		public static PathsManager Instance { get; private set; }

		public async static void SpawnThem()
		{
			foreach (NodeFile file in NodeReader.Nodes)
			{
				foreach (PathNode node in file.PathNodes.Where(pn => pn.IsPed))
				{
					for (int k = 0; k < node.LinkCount; k++)
					{
						try
						{
							int linkArrayIndex = node.BaseLinkID + k;
							PathNode targetNode = NodeReader.Nodes.Single(nf => nf.Id == file.NodeLinks[linkArrayIndex].AreaID).PathNodes.ElementAt(file.NodeLinks[linkArrayIndex].NodeID);
							int length = file.NodeLinks[node.BaseLinkID + k].Length;
							Debug.LogError("Found node link " + node.NodeID + " to " + targetNode.NodeID);
							Ped.Instance.Teleport(targetNode.Position);
							await System.Threading.Tasks.Task.Delay(System.TimeSpan.FromMilliseconds (750));
						}
						catch (Exception)
						{
							Debug.LogError($"Error at node id {node.NodeID}, linkarrayindex {node.BaseLinkID + k}, desired file id {file.NodeLinks[node.BaseLinkID + k].AreaID}. loaded files: {NodeReader.Nodes.Count()}");
						}
					}
				}
			}
		}
		
		void Awake()
		{
			Instance = this;
		}
	}
}
