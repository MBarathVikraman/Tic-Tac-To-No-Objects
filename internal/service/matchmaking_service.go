package service

import (
	"fmt"
	"log"
	"math/rand"
	"time"

	"tictactoe/internal/protocol"
	"tictactoe/internal/room"
	"tictactoe/internal/store"
)

type MatchmakingService struct {
	store *store.RoomStore
}

func NewMatchmakingService(store *store.RoomStore) *MatchmakingService {
	return &MatchmakingService{store: store}
}

func newRoomID() string {
	return fmt.Sprintf(
		"%s-%04d",
		time.Now().Format("20060102150405.000000000"),
		rand.Intn(10000),
	)
}

func (s *MatchmakingService) FindMatch(client room.ClientPort, setRoom func(*room.Room), setPlayer func(int)) {
	waiting := s.store.PopWaitingRoom()
	if waiting != nil {
		waiting.Join(client)
		setRoom(waiting)
		setPlayer(2)
		waiting.P1.Send(protocol.Msg{Type: protocol.MsgMatchFound, MatchID: waiting.ID, YouAre: 1})
		waiting.P2.Send(protocol.Msg{Type: protocol.MsgMatchFound, MatchID: waiting.ID, YouAre: 2})
		log.Println("Room started:", waiting.ID)
		return
	}

	roomID := newRoomID()
	newRoom := room.NewRoom(roomID, client)
	setRoom(newRoom)
	setPlayer(1)
	s.store.AddRoom(newRoom)
	client.Send(protocol.Msg{Type: protocol.MsgWaiting, MatchID: roomID, YouAre: 1})
	log.Println("Created waiting room:", roomID)
}

func (s *MatchmakingService) HandleDisconnect(client room.ClientPort, r *room.Room) {
	if r == nil {
		return
	}

	if r.P1 == client && r.P2 == nil {
		s.store.RemoveRoom(r.ID)
		client.ClearRoom()
		log.Println("Waiting player disconnected, removed room:", r.ID)
		return
	}

	var opp room.ClientPort
	if r.P1 == client {
		opp = r.P2
		r.P1 = nil
	} else if r.P2 == client {
		opp = r.P1
		r.P2 = nil
	}

	if opp != nil {
		opp.Send(protocol.Msg{Type: protocol.MsgOpponentLeft})
		opp.ClearRoom()
	}

	s.store.RemoveRoom(r.ID)
	client.ClearRoom()
	log.Println("Active match ended due to disconnect, removed room:", r.ID)
}
