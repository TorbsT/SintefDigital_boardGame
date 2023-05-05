﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Network;
using TMPro;

namespace View
{
    internal class TurnManager : MonoBehaviour
    {
        public static TurnManager Instance { get; private set; }

        public bool IsMyTurn { get; private set; }
        public event Action TurnChanged;
        public event Action<List<NetworkData.DistrictModifier>> orchestratorTurnChange;

        [SerializeField] private Button endTurnButton;
        [SerializeField] private TextMeshProUGUI turnText;
        [SerializeField] private PlayerOwned playerOwned;
        private bool sending;
        private string prevTurnRole;
        public bool isOrchestratorsTurn;
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
            if (state == null) return;
            string turnRoleName = state.Value.current_players_turn;
            NetworkData.InGameID turnRole = (NetworkData.InGameID)Enum.Parse(typeof(NetworkData.InGameID), turnRoleName);
            isOrchestratorsTurn = turnRole == NetworkData.InGameID.Orchestrator;
            NetworkData.Player? turnPlayer = null;

            foreach (var player in state.Value.players) {
                if (turnRoleName == player.in_game_id)
                {
                    turnPlayer = player;
                    break;
                }
            }
            if (turnPlayer == null) Debug.LogError("There is nobody's turn: "+turnRole);

            string txt = "";
            IsMyTurn = turnRoleName == GameStateSynchronizer.Instance.Me.in_game_id;
            if (IsMyTurn)
            {
                txt = "Your turn";
                if (turnRole != NetworkData.InGameID.Orchestrator)
                    txt += $" ({turnPlayer.Value.remaining_moves} moves left)";
            }
            else
            {
                if (turnRoleName == NetworkData.InGameID.Orchestrator.ToString())
                    txt = "Orchestrator ";
                txt += $"{turnPlayer.Value.name}'s turn";
            }
            playerOwned.Owner = turnRole;
            turnText.text = txt;
            endTurnButton.interactable = IsMyTurn && !sending;

            if (turnRoleName != prevTurnRole)
            {
                TurnChanged?.Invoke();
                orchestratorTurnChange?.Invoke(GameStateSynchronizer.Instance.GameState.Value.district_modifiers);
             
            }
        prevTurnRole = turnRoleName;
        }
        public void Endturn()
        {
            endTurnButton.interactable = false;
            sending = true;
            NetworkData.PlayerInput input = new()
            {
                player_id = NetworkData.Instance.UniqueID,
                game_id = GameStateSynchronizer.Instance.LobbyId.Value,
                input_type = NetworkData.PlayerInputType.NextTurn.ToString(),
                related_role = GameStateSynchronizer.Instance.Me.in_game_id,
            };
            RestAPI.Instance.SendPlayerInput(success => { sending = false; UndoSystem.Instance.MovesDone = 0; }, failure => { endTurnButton.interactable = true; sending = false; }, input);
        }
    }
}
