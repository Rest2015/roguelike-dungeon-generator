using UnityEngine;
using System.Collections;

public class MapDrawer : MonoBehaviour {

	public Transform prefab;

	public void Draw (int[,] mapInfo) {
		for (int i=0; i<mapInfo.GetLength(0); i++) {
			for (int j=0; j<mapInfo.GetLength(1); j++) {
				if (mapInfo[i,j] == -1) {
					Instantiate (prefab, new Vector3(i,1,j), Quaternion.identity);
				}
				if (mapInfo[i,j] != 0) {
					Instantiate (prefab, new Vector3(i,0,j), Quaternion.identity);
				}
			}
		}
	}
}
