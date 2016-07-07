using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour {

    private int _mapX = 100;
    private int _mapZ = 100;
    private int _expectRoomNum = 5;
    private int _roomMinRange = 4;
    private int _roomMaxRange = 12;
	private int _corridorMinLength = 3;
	private int _corridorMaxLength = 10;

    private int[,] _floorInfo;
    private int[,] _mapInfo;//0:none -1:wall N:room N

	private class cCorridorExit {
		public cCorridorExit (
			int xValue,
			int zValue,
			int delX,
			int delZ
		) {
			X = xValue;
			Z = zValue;
			deltaX = delX;
			deltaZ = delZ;
		}
		public int X;
		public int Z;
		public int deltaX;
		public int deltaZ;
	}

    private class cRoom {
		public cRoom (
			int idValue,
			int xValue,
			int zValue,
			int widthValue,
			int lengthValue
		) {
			id = idValue;
			x = xValue;
			z = zValue;
			xWidth = widthValue;
			zLength = lengthValue;
		}
        public int id;
        public int x;
        public int z;
        public int xWidth;
        public int zLength; 
    }

    private class cCorridor {
		public cCorridor (
			int idValue,
			int xValue,
			int zValue,
			int delX,
			int delZ,
			int corridorLength
		) {
			id = idValue;
			x = xValue;
			z = zValue;
			deltaX = delX;
			deltaZ = delZ;
			length = corridorLength;
			exitList = new List<cCorridorExit>();
			exitList.Add(new cCorridorExit(
				x+deltaX*length,
				z+deltaZ*length,
				deltaX,
				deltaZ
			));
			int wallDelX,wallDelZ;
			wallDelX = Mathf.Abs(deltaX) ^ 1;
			wallDelZ = Mathf.Abs(deltaZ) ^ 1;
			exitList.Add(new cCorridorExit(
				x+deltaX*(length-1)+wallDelX,
				z+deltaZ*(length-1)+wallDelZ,
				wallDelX,
				wallDelZ
			));
			wallDelX = -wallDelX;
			wallDelZ = -wallDelZ;
			exitList.Add(new cCorridorExit(
				x+deltaX*(length-1)+wallDelX,
				z+deltaZ*(length-1)+wallDelZ,
				wallDelX,
				wallDelZ
			));
		}
		public int id;
        public int x;
        public int z;
        public int deltaX;
        public int deltaZ;
		public int length;
		public List<cCorridorExit> exitList;
    }

	int currentMapElemId;
    cRoom currentRoom;
	cCorridor currentCorridor;
    List<cRoom> roomList;
	List<cCorridor> corridorList;

    void Start() {
		InitMapInfo();
		GenerateMap();
        PrintLogMapInfo();
		GetComponent<MapDrawer> ().Draw (_mapInfo);
    }

    public void SetMapParams (int mapWidth, int mapLength, int roomNum, int roomMin, int roomMax) {
        _mapX = mapWidth;
        _mapZ = mapLength;
        _expectRoomNum = roomNum;
        _roomMinRange = roomMin;
        _roomMaxRange = roomMax;
    }

    void InitMapInfo() {
        _floorInfo = new int[_mapX, _mapZ];
        _mapInfo = new int[_mapX, _mapZ];
        for (int i=0; i<_mapX; i++) {
            for (int j=0; j<_mapZ; j++) {
                _floorInfo[i, j] = 0;
                _mapInfo[i, j] = 0;
            }
        }
    }

    void GenerateMap() {
		currentMapElemId = 1;
        roomList = new List<cRoom>();
		corridorList = new List<cCorridor> ();

		currentRoom = new cRoom(
			currentMapElemId,
			Random.Range(_mapX / 3, _mapX / 3 * 2),
			Random.Range(_mapZ / 3, _mapZ / 3 * 2),
			Random.Range(_roomMinRange, _roomMaxRange),
			Random.Range(_roomMinRange, _roomMaxRange)
		);

        UpdateMapInfo(currentRoom);
        roomList.Add(currentRoom);
		CreateCorridorRecursivly (currentRoom);

		CreateCorridorRecursivly (currentRoom);
		CreateCorridorRecursivly (currentCorridor);

        for (int n=_expectRoomNum; n>0; n--) {
			cCorridor tmpCorridor = corridorList [Random.Range (0, corridorList.Count - 1)];
			if (tmpCorridor.exitList.Count>0) {
				CreateRoomRecursivly (tmpCorridor);
			}
        }
    }

    void UpdateMapInfo (cRoom room) {
        for (int i=room.x; i<room.x+room.xWidth; i++) {
            for (int j=room.z; j<room.z+room.zLength; j++) {
                if (i == room.x ||
                    i == room.x+room.xWidth - 1 ||
                    j == room.z ||
                    j == room.z+room.zLength - 1)
                {
                    _mapInfo[i, j] = -1;
                }
                else
                {
                    _mapInfo[i, j] = room.id;
                }
            }
        }
    }

	void UpdateMapInfo (cCorridor corridor) {
		int index = 0;
		int wallDelX = Mathf.Abs(corridor.deltaX) ^ 1;
		int wallDelZ = Mathf.Abs(corridor.deltaZ) ^ 1;
		while (index<corridor.length) {
			_mapInfo [corridor.x + corridor.deltaX * index, corridor.z + corridor.deltaZ * index] = corridor.id;
			_mapInfo [corridor.x + corridor.deltaX * index + wallDelX, corridor.z + corridor.deltaZ * index + wallDelZ] = -1;
			_mapInfo [corridor.x + corridor.deltaX * index - wallDelX, corridor.z + corridor.deltaZ * index - wallDelZ] = -1;
			index++;
		}
		_mapInfo [corridor.x + corridor.deltaX * index, corridor.z + corridor.deltaZ * index] = -1;
		_mapInfo [corridor.x + corridor.deltaX * index + wallDelX, corridor.z + corridor.deltaZ * index + wallDelZ] = -1;
		_mapInfo [corridor.x + corridor.deltaX * index - wallDelX, corridor.z + corridor.deltaZ * index - wallDelZ] = -1;
	}

    void PrintLogMapInfo() {
        string debugStr;
        for (int i = 0; i < _mapX; i++)
        {
            debugStr = " ";
            for (int j = 0; j < _mapZ; j++)
            {
                debugStr += _mapInfo[i, j].ToString();
                debugStr += " ";
            }
            Debug.Log(debugStr);
        }
    }

	bool CreateCorridorRecursivly (cRoom room) {
        ////randomly choose a room
        //cRoom room = roomList[Random.Range(0, roomList.Count - 1)];

		int deltaX, deltaZ;
		int wallX, wallZ;//target wall pos
		switch (Random.Range(1,4)) {
		//randomly choose a direction (for corridor)
		case 1:
			//up
			deltaX = 0;
			deltaZ = 1;
			wallX = Random.Range (room.x + 1, room.x + room.xWidth - 2);
			wallZ = room.z + room.zLength - 1;
			if (_mapInfo[wallX,wallZ] != -1 || _mapInfo[wallX-1,wallZ] != -1 || _mapInfo[wallX+1,wallZ] != -1) {
				return false;
			}
			break;
		case 2:
			//down
			deltaX = 0;
			deltaZ = -1;
			wallX = Random.Range (room.x + 1, room.x + room.xWidth - 2);
			wallZ = room.z;
			if (_mapInfo[wallX,wallZ] != -1 || _mapInfo[wallX-1,wallZ] != -1 || _mapInfo[wallX+1,wallZ] != -1) {
				return false;
			}
			break;
		case 3:
			//left
			deltaX = -1;
			deltaZ = 0;
			wallX = room.x;
			wallZ = Random.Range (room.z + 1, room.z + room.zLength - 2);
			if (_mapInfo[wallX,wallZ] != -1 || _mapInfo[wallX,wallZ-1] != -1 || _mapInfo[wallX,wallZ+1] != -1) {
				return false;
			}
			break;
		case 4:
			//right
			deltaX = 1;
			deltaZ = 0;
			wallX = room.x+room.xWidth-1;
			wallZ = Random.Range (room.z + 1, room.z + room.zLength - 2);
			if (_mapInfo[wallX,wallZ] != -1 || _mapInfo[wallX,wallZ-1] != -1 || _mapInfo[wallX,wallZ+1] != -1) {
				return false;
			}
			break;
		default:
			deltaX = 0;
			deltaZ = 0;
			wallX = 0;
			wallZ = 0;
			Debug.LogError ("Invalid Direction!");
			break;
		}
		int corridorLength = Random.Range (_corridorMinLength, _corridorMaxLength);
		if (!CheckCorridorSpace(
			wallX + deltaX,
			wallZ + deltaZ,
			deltaX,
			deltaZ,
			corridorLength
		)) {
			return false;
		}
		currentMapElemId++;
		currentCorridor = new cCorridor (
			currentMapElemId,
			wallX + deltaX,
			wallZ + deltaZ,
			deltaX,
			deltaZ,
			corridorLength
		);
		_mapInfo [wallX, wallZ] = room.id;//remove wall
		UpdateMapInfo (currentCorridor);
		corridorList.Add (currentCorridor);	
        return true;
    }

	bool CheckCorridorSpace (int startX, int startZ, int delX, int delZ, int corridorLength) {
		
		int index = 0;
		int wallDelX = Mathf.Abs(delX) ^ 1;
		int wallDelZ = Mathf.Abs(delZ) ^ 1;
		if (startX+wallDelX<0 || 
			startZ+wallDelZ<0 || 
			startX+wallDelX>_mapX-1 || 
			startZ+wallDelZ>_mapZ-1 ||
			startX-wallDelX<0 || 
			startZ-wallDelZ<0 || 
			startX-wallDelX>_mapX-1 || 
			startZ-wallDelZ>_mapZ-1 ||
			startX+delX*corridorLength<0 || 
			startZ+delZ*corridorLength<0 || 
			startX+delX*corridorLength>_mapX-1 || 
			startZ+delZ*corridorLength>_mapZ-1
		) {
			return false;
		}
		while (index<corridorLength) {
			if (
				_mapInfo[startX+delX*index, startZ+delZ*index]!=0 ||
				_mapInfo[startX+delX*index+wallDelX, startZ+delZ*index+wallDelZ]!=0 ||
				_mapInfo[startX+delX*index-wallDelX, startZ+delZ*index-wallDelZ]!=0
			) {
				return false;
			}
			index++;
		}
		if (
			_mapInfo[startX+delX*index, startZ+delZ*index]!=0 ||
			_mapInfo[startX+delX*index+wallDelX, startZ+delZ*index+wallDelZ]!=0 ||
			_mapInfo[startX+delX*index-wallDelX, startZ+delZ*index-wallDelZ]!=0
		) {
			return false;
		}
		return true;
	}

	bool CheckRoomSpace (int startX, int startZ, int widthX, int lengthZ) {
		if (startX<0 || startZ<0 || startX+widthX>_mapX-1 || startZ>_mapZ-1) {
			return false;
		}
		for (int i=startX; i<startX+widthX; i++) {
			for (int j=startZ; j<startZ+lengthZ; j++) {
				if (_mapInfo[i,j]!=0) {
					return false;
				}
			}
		}
		return true;
	}

	bool CreateRoomRecursivly (cCorridor corridor) {
		int jointX = corridor.x + corridor.deltaX * (corridor.length+1);//wall length add 1 for wall
		int jointZ = corridor.z + corridor.deltaZ * (corridor.length+1);
		int roomX, roomZ;
		int widthX = Random.Range (_roomMinRange, _roomMaxRange);
		int lengthZ = Random.Range (_roomMinRange, _roomMaxRange);
		if (corridor.deltaX == 0) {
			if (corridor.deltaZ == 1) {
				//joint on room down
				roomX = jointX - Random.Range(1, widthX-2);
				roomZ = jointZ;
			} else {
				//joint on room up
				roomX = jointX - Random.Range(1, widthX-2);
				roomZ = jointZ - lengthZ + 1;
			}
		} else {
			if (corridor.deltaX == 1) {
				//joint on room left
				roomX = jointX;
				roomZ = jointZ - Random.Range (1, lengthZ-2);
			} else {
				//joint on room right
				roomX = jointX - widthX + 1;
				roomZ = jointZ - Random.Range (1, lengthZ-2);
			}
		}
		if (!CheckRoomSpace(
			roomX,
			roomZ,
			widthX,
			lengthZ
		)) {
			return false;
		}
		currentMapElemId++;
		currentRoom = new cRoom(
			currentMapElemId,
			roomX,
			roomZ,
			widthX,
			lengthZ
		);
		UpdateMapInfo(currentRoom);
		roomList.Add(currentRoom);
		_mapInfo [corridor.x + corridor.deltaX * corridor.length, corridor.z + corridor.deltaZ * corridor.length] = corridor.id;
		_mapInfo [jointX, jointZ] = currentRoom.id;
		corridor.exitList.Clear ();
		return true;
	}

	bool CreateCorridorRecursivly (cCorridor corridor) {
		cCorridorExit corExit = corridor.exitList[Random.Range(0,corridor.exitList.Count-1)];
		int corridorLength = Random.Range(_corridorMinLength, _corridorMaxLength);
		if (!CheckCorridorSpace(
			corExit.X + corExit.deltaX,
			corExit.Z + corExit.deltaZ,
			corExit.deltaX,
			corExit.deltaZ,
			corridorLength
		)) {
			return false;
		}
		currentMapElemId++;
		currentCorridor = new cCorridor (
			currentMapElemId,
			corExit.X + corExit.deltaX,
			corExit.Z + corExit.deltaZ,
			corExit.deltaX,
			corExit.deltaZ,
			corridorLength
		);
		_mapInfo [corExit.X, corExit.Z] = corridor.id;//remove wall
		UpdateMapInfo (currentCorridor);
		corridorList.Add (currentCorridor);	
		corridor.exitList.Remove (corExit);
		return true;
	}
}
