﻿using Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace View
{
    internal class MovementSystem : MonoBehaviour
    {
        public static MovementSystem Instance { get; private set; }
        private Dictionary<string, int> playerPositions = new();
        private void Awake()
        {
            Instance = this;
        }
        private void Start()
        {
            GameStateSynchronizer.Instance.StateChanged += StateChanged;
        }
        private void StateChanged(NetworkData.GameState? state)
        {
            foreach (var player in state.Value.players)
            {
                string role = player.in_game_id;
                if (role == NetworkData.InGameID.Orchestrator.ToString())
                    continue;
                int newPos = player.position_node_id.Value;
                if (!playerPositions.ContainsKey(role))
                    playerPositions.Add(role, newPos);
                int oldPos = playerPositions[role];
                if (newPos != oldPos)
                {
                    Debug.LogWarning("A PLAYER MOVED!!!!");
                    playerPositions[role] = newPos;
                    if (ObjectiveVisualizer.Instance == null) break;
                    Transform playerTransform = ObjectiveVisualizer.Instance.GetPlayerGO(role).transform;
                    Transform targetTransform = GraphManager.Instance.GetNode(newPos).gameObject.transform;
                    playerTransform.parent = targetTransform;
                    Animation<Vector2> moveAnimation = new()
                    {
                        StartValue = playerTransform.position,
                        EndValue = targetTransform.position,
                        Duration = AnimationPresets.Instance.PlayerMoveDuration,
                        Curve = AnimationPresets.Instance.PlayerMoveCurve,
                        Action = (value) => { playerTransform.position = value; }
                    };
                    moveAnimation.Start();
                }
            }
        }
        public void ClickNode(NodeTraversal trav)
        {
            NetworkData.PlayerInput input = new()
            {
                player_id = NetworkData.Instance.UniqueID,
                game_id = GameStateSynchronizer.Instance.LobbyId.Value,
                input_type = NetworkData.PlayerInputType.Movement.ToString(),
                related_role = GameStateSynchronizer.Instance.Me.in_game_id,
                related_node_id = trav.Id,
            };
            RestAPI.Instance.SendPlayerInput(success =>
            {
                Debug.Log("success!!!!");
                UndoSystem.Instance.MovesDone++;
            }, failure =>
            {
                Debug.Log(failure);
            }
            , input);
            //Debug.Log("clicked");

            //player.transform.localPosition = new Vector3(0, 0, 0);
        }
    }
}
