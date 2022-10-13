extends Node

var peer: WebSocketMultiplayerPeer
onready var lobby = $Lobby
var id: int = -1
var player_name = ''

func inititalise_server() -> void:
	peer = WebSocketServer.new()

	peer.listen(5432, [], true)

	get_tree().connect("network_peer_connected", lobby, "_player_connected")
	get_tree().connect("network_peer_disconnected", lobby, "_player_disconnected")

	get_tree().network_peer = peer
	yield(get_tree().create_timer(1), "timeout")
	lobby._player_connected(get_tree().get_network_unique_id())
	lobby.set_network_master(1)
	set_player_name("server")


func initialise_client(address: String) -> void:
	peer = WebSocketClient.new()
	get_tree().connect("connected_to_server", lobby, "_connected_ok")
	get_tree().connect("connection_failed", lobby, "_connected_fail")
	get_tree().connect("server_disconnected", lobby, "_server_disconnected")
	peer.set_target_peer(NetworkedMultiplayerPeer.TARGET_PEER_SERVER)

	var error = peer.connect_to_url(address + ":5432", [],  true)

	if error:
		print("Could not connect: " + error)
		get_tree().disconnect("connected_to_server", lobby, "_connected_ok")
		get_tree().disconnect("connection_failed", lobby, "_connected_fail")
		get_tree().disconnect("server_disconnected", lobby, "_server_disconnected")

		return

	get_tree().network_peer = peer
	lobby.set_network_master(1)


func cancel_network() -> void:

	if peer == null:
		return

	if get_tree().is_connected("network_peer_connected", lobby, "_player_connected"):
		get_tree().disconnect("network_peer_connected", lobby, "_player_connected")
		get_tree().disconnect("network_peer_disconnected", lobby, "_player_disconnected")
	if get_tree().is_connected("connected_to_server", lobby, "_connected_ok"):
		get_tree().disconnect("connected_to_server", lobby, "_connected_ok")
		get_tree().disconnect("connection_failed", lobby, "_connected_fail")
		get_tree().disconnect("server_disconnected", lobby, "_server_disconnected")

	if peer is WebSocketClient:
		peer.disconnect_from_host(1000, "Disconnected from lobby")
	elif peer is WebSocketServer:
		peer.stop()
	else:
		peer.close_connection()
	get_tree().network_peer = null
	peer = null


func _on_Lobby_server_gave_id(id) -> void:
	self.id = id


func set_player_name(name: String) -> void:
	player_name = name
	var id = get_tree().get_network_unique_id()
	lobby.set_player_name(id, name)


func serialise_players(players=lobby.players):
	var _players = {}
	for key in players.keys():
		_players[key] = inst2dict(players[key])
	return _players


func deserialise_players(players: Dictionary):
	var _players = {}
	for key in players.keys():
		_players[key] = dict2inst(players[key])
	return _players
