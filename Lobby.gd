class_name Lobby
extends Node

var players = {}

signal server_gave_id (id)
signal player_count_changed (players)
signal connected_to_server
signal disconnected_from_server

func _player_connected(id: int) -> void:
	add_player_to_lobby(id)
	rpc("_receive_players", NetworkManager.serialise_players(players))
	emit_signal("player_count_changed", players)

func _player_disconnected(id: int) -> void:
	rpc("remove_player_from_lobby", id)

func _connected_ok() -> void:
	print("Connected!")
	set_player_name(get_tree().get_network_unique_id(), NetworkManager.player_name)
	rpc("_request_players")
	emit_signal("connected_to_server")

func _connected_fail() -> void:
	print("Connection Failed!")
	emit_signal("disconnected_from_server")
	NetworkManager.cancel_network()

func _server_disconnected() -> void:
	print("Server disconnected!")
	emit_signal("disconnected_from_server")
	NetworkManager.cancel_network()

master func _request_players():
	print('players requested')
	rpc("_receive_players", NetworkManager.serialise_players())

puppetsync func _receive_players(_players: Dictionary):
	print("players received: ", _players)
	update_players_locally(NetworkManager.deserialise_players(_players))

remotesync func _get_id(id: int) -> void:
	emit_signal("server_gave_id", id)

remotesync func update_players_locally(players: Dictionary):
	self.players = players
	emit_signal("player_count_changed", players)

puppetsync func request_players():
	rpc("_request_players")

master func add_player_to_lobby(id: int) -> void:
	players[id] = PlayerNetworkInfo.new()
	emit_signal("player_count_changed", players)

remotesync func remove_player_from_lobby(id) -> void:
	players.erase(id)
	emit_signal("player_count_changed", players)

master func _master_set_player_name(id: int, name: String) -> void:
	if id in players.keys():
		players[id].name = name
	emit_signal("player_count_changed", players)
	rpc("_receive_players", NetworkManager.serialise_players())


func set_player_name(id: int, name: String) -> void:
	rpc_id(1, "_master_set_player_name", id, name)
