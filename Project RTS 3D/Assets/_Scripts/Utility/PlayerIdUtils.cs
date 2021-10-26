using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


static public class PlayerIdUtils
{
	static public bool doesPlayerNumMatchLocalPlayer(Photon.Realtime.Player photonPlayer, Player playerId)
	{
		if (!PhotonNetwork.IsConnected)
		{
			return playerId == Player.Player01;
		}

		//Debug.Log("localPlayer.ActorNumber = " + localPlayer.ActorNumber + ", (int)playerOwning = " + (int)PlayerOwning + ", transform.parent.gameObject.name = " + transform.parent.gameObject.name);

		return (photonPlayer.ActorNumber == ((int)playerId + 1));
	}
}
