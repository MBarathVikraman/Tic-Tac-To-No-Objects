package room

import (
	"encoding/json"
	"sync"

	"tictactoe/internal/domain"
	"tictactoe/internal/protocol"
)

type ClientPort interface {
	PlayerValue() int
	Send(protocol.Msg)
	SendRaw([]byte)
	SendError(string)
	ClearRoom()
}

type Room struct {
	ID      string
	Players []ClientPort
	Board   []int
	Size    int
	Turn    int
	Mu      sync.Mutex
}

const MaxPlayers = 2 // current game limit

func NewRoom(id string, p1 ClientPort, size int) *Room {
	return &Room{
		ID:      id,
		Players: []ClientPort{p1},
		Size:    size,
		Board:   make([]int, size*size),
		Turn:    1,
	}
}

func (r *Room) PlayerCount() int {
	return len(r.Players)
}

func (r *Room) IsWaiting() bool {
	return len(r.Players) == 1
}

func (r *Room) CanJoin() bool {
	return len(r.Players) < MaxPlayers
}

func (r *Room) Join(p ClientPort) {
	r.Mu.Lock()
	defer r.Mu.Unlock()

	if len(r.Players) >= MaxPlayers {
		return
	}

	r.Players = append(r.Players, p)
	r.Turn = 1
}

func (r *Room) Broadcast(msg protocol.Msg) {

	b, _ := json.Marshal(msg)

	for _, p := range r.Players {
		if p == nil {
			continue
		}
		p.SendRaw(b)
	}
}

func (r *Room) RemovePlayer(p ClientPort) {

	r.Mu.Lock()
	defer r.Mu.Unlock()

	for i, pl := range r.Players {
		if pl == p {
			r.Players = append(r.Players[:i], r.Players[i+1:]...)
			break
		}
	}
}

func (r *Room) TryMove(c ClientPort, cell int) {

	r.Mu.Lock()
	defer r.Mu.Unlock()

	// Need both players
	if len(r.Players) < 2 {
		c.SendError("waiting for opponent")
		return
	}

	if cell < 0 || cell >= len(r.Board) {
		c.SendError("invalid cell")
		return
	}

	if r.Turn != c.PlayerValue() {
		c.SendError("not your turn")
		return
	}

	if r.Board[cell] != 0 {
		c.SendError("cell not empty")
		return
	}

	player := c.PlayerValue()
	r.Board[cell] = player

	// Switch turn (still 2 players only for now)
	if r.Turn == 1 {
		r.Turn = 2
	} else {
		r.Turn = 1
	}

	r.Broadcast(protocol.Msg{
		Type:     protocol.MsgMove,
		Cell:     cell,
		Value:    player,
		NextTurn: r.Turn,
	})

	w := domain.CheckWinner(r.Board, r.Size)

	if w != 0 {
		r.Broadcast(protocol.Msg{
			Type:   protocol.MsgGameOver,
			Winner: w,
		})
		return
	}

	if domain.IsDraw(r.Board, r.Size) {
		r.Broadcast(protocol.Msg{
			Type:   protocol.MsgGameOver,
			Winner: 0,
		})
	}
}
