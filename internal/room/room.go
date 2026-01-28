package room

import (
	"sync"

	"tictactoe/internal/domain"
	"tictactoe/internal/protocol"
)

type ClientPort interface {
	PlayerValue() int
	Send(protocol.Msg)
	SendError(string)
	ClearRoom()
}

type Room struct {
	ID    string
	P1    ClientPort
	P2    ClientPort
	Board [9]int
	Turn  int
	Mu    sync.Mutex
}

func NewRoom(id string, p1 ClientPort) *Room {
	return &Room{
		ID:   id,
		P1:   p1,
		P2:   nil,
		Turn: 1,
	}
}

func (r *Room) IsWaiting() bool {
	return r.P1 != nil && r.P2 == nil
}

func (r *Room) Join(p2 ClientPort) {
	r.P2 = p2
	r.Turn = 1
}

func (r *Room) Broadcast(m protocol.Msg) {
	if r.P1 != nil {
		r.P1.Send(m)
	}
	if r.P2 != nil {
		r.P2.Send(m)
	}
}

func (r *Room) TryMove(c ClientPort, cell int) {
	r.Mu.Lock()
	defer r.Mu.Unlock()

	if r.P2 == nil {
		c.SendError("waiting for opponent")
		return
	}
	if cell < 0 || cell >= 9 {
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

	w := domain.CheckWinner(r.Board)
	if w != 0 {
		r.Broadcast(protocol.Msg{Type: protocol.MsgGameOver, Winner: w})
		return
	}
	if domain.IsDraw(r.Board) {
		r.Broadcast(protocol.Msg{Type: protocol.MsgGameOver, Winner: 0})
	}
}
