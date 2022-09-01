using HarmonyLib;
using BepInEx;
using UnityEngine;
using TMPro;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Steamworks;
using Steamworks.Data;

namespace PlayerCap.Patches
{
    [HarmonyPatch]
    class BreakPlayerCap
    {
		public static int players = 32;
		public static bool capIncreased = false;

		[HarmonyPatch(typeof(SteamMatchmaking), "CreateLobbyAsync")]
		static void Prefix(ref int maxMembers)
		{
			maxMembers = players;
		}

		[HarmonyPatch(typeof(LobbyVisuals), "OpenLobby")]
		static void Postfix(LobbyVisuals __instance)
		{
			JoeScript(__instance, false);

			Server.MaxPlayers = players;
			SteamLobby.lobbySize = players;
		}


		[HarmonyPatch(typeof(LobbyVisuals), "SpawnLobbyPlayer")]
		[HarmonyPostfix]
		static void CreateLobbyStates(LobbyVisuals __instance)
		{
			if (!capIncreased)
			{

				for (int i = 8; i < players; i++)
				{
					GameObject clone = LobbyVisuals.Instantiate(GameObject.Find("/LobbyVisuals/OnlinePlayer"));
					clone.transform.parent = GameObject.Find("LobbyVisuals").transform;
					clone.transform.position = GameObject.Find("/LobbyVisuals/OnlinePlayer").transform.position + new Vector3((i / 2) * .25f, 1, 0);
					clone.name = $"OnlinePlayer ({i})";
					clone.SetActive(false);
					__instance.lobbyPlayers = new List<GameObject>(__instance.lobbyPlayers) { clone }.ToArray();
					__instance.lobbyPlayerNames = new List<OnlyActivateForHost>(__instance.lobbyPlayerNames) { __instance.lobbyPlayerNames[__instance.lobbyPlayerNames.Length - 1] }.ToArray();
					__instance.playerNames = new List<TextMeshProUGUI>(__instance.playerNames) { __instance.playerNames[__instance.playerNames.Length - 1] }.ToArray();
				}
				capIncreased = true;
				float normalizedHeight = 21.519f;
				Vector3 bounds = new Vector3(98, normalizedHeight, 667);
				int rows = 0;
				for (int i = 0; i < players; i++ ) {
					__instance.lobbyPlayers[i].transform.position = bounds;
					// 8 is the max players allowed in a row
					if ( (i%8) == 0 )
                    {
						rows += 1;
						bounds = new Vector3(98, normalizedHeight, 667 + (rows * 2.5f));
                    }
					bounds += new Vector3(2, 0, 0);
				}
			}
			int totalPlayers = 0;
			foreach (GameObject obj in __instance.lobbyPlayers)
			{
				if (!obj.activeInHierarchy)
				{
					break;
				}
				totalPlayers += 1;

			}
			if (totalPlayers > 8)
			{
				__instance.playerNames[totalPlayers - 1].text = $"... and {totalPlayers - 7} more";
			}
		}




		/*
		public static void AddComponents(TestRagdoll ragdoll, Transform p, Rigidbody parent, Vector3 dir)
        {
			p.gameObject.layer = LayerMask.NameToLayer("GroundAndObjectOnly");
			Rigidbody rigidbody = p.gameObject.AddComponent<Rigidbody>();
			if (!rigidbody)
			{
				rigidbody = p.GetComponent<Rigidbody>();
				rigidbody.isKinematic = false;
				rigidbody.useGravity = true;
			}
			rigidbody.velocity = dir.normalized * 10f;
			rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
			rigidbody.angularDrag = 1f;
			rigidbody.drag = 0.2f;
			p.gameObject.AddComponent<SphereCollider>().material = ragdoll.mat;
			if (parent != null)
			{
				CharacterJoint characterJoint = p.gameObject.AddComponent<CharacterJoint>();
				characterJoint.connectedBody = parent;
				characterJoint.enableProjection = true;
			}
		}

		public static void Ragdoll (int index, TestRagdoll ragdoll, Transform part, Vector3 dir)
        {
			part.gameObject.layer = LayerMask.NameToLayer("GroundAndObjectOnly");
			for (int i = 0; i < part.childCount; i++)
			{
				Transform child = part.GetChild(i);
				if (!child.CompareTag("Ignore"))
				{
					Vector3 center = new Vector3(103, 23, 680);
					Vector3 cPos = LobbyVisuals.Instance.lobbyPlayers[index].GetComponent<Rigidbody>().position;
					AddComponents(ragdoll, child, part.GetComponent<Rigidbody>(), new Vector3(center.x - cPos.x + 1, center.y - cPos.y + 1, center.z - cPos.z + 1));
					Ragdoll(index, ragdoll, child, new Vector3(center.x - cPos.x + 1, center.y - cPos.y + 1, center.z - cPos.z + 1));
				}
			}
		}
		*/


		public static void JoeScript (LobbyVisuals __instance, bool active=true)
        {
			if (active)
            {
				for (int i = 0; i < players; i++)
				{
					int nextId = __instance.GetNextId();
					__instance.lobbyPlayers[nextId].SetActive(true);
					__instance.lobbyPlayers[nextId].GetComponentInChildren<TextMeshProUGUI>().text = $"Joe Mama {nextId}";
					__instance.playerNames[nextId].text = $"Joe Mama {nextId}";
					int totalPlayers = 0;
					foreach (GameObject obj in __instance.lobbyPlayers)
					{
						if (!obj.activeInHierarchy)
						{
							break;
						}
						totalPlayers += 1;

					}
					if (totalPlayers - 1 > 6)
					{
						__instance.playerNames[totalPlayers - 1].text = $"... and {totalPlayers - 7} more";
					}
				}
			}
		}
		[HarmonyPatch(typeof(LobbyVisuals), "DespawnLobbyPlayer")]
		[HarmonyPostfix]
		static void RefreshPlayerCount(LobbyVisuals __instance)
		{
			int totalPlayers = 0;
			foreach (GameObject obj in __instance.lobbyPlayers)
			{
				if (!obj.activeInHierarchy)
				{
					break;
				}
				totalPlayers += 1;

			}
			if (totalPlayers - 1 > 6)
			{
				__instance.playerNames[totalPlayers - 1].text = $"... and {totalPlayers - 6} more";
			}
		}

		[HarmonyPatch(typeof(LoadingScreen), "Show")]
		[HarmonyPostfix]
		static void ResetCapIncreased(LoadingScreen __instance)
		{
			capIncreased = false;
		}

	}
}


