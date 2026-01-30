package service

import (
	"fmt"
	"log"
	"math/rand"
	"sync"
	"time"

	"tictactoe/internal/protocol"
	"tictactoe/internal/room"
	"tictactoe/internal/store"
)

type MatchmakingService struct {
	store *store.RoomStore
	mu    sync.Mutex
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

func (s *MatchmakingService) FindMatch(
	client room.ClientPort,
	setRoom func(*room.Room),
	setPlayer func(int),
	boardSize int,
) {

	s.mu.Lock()
	defer s.mu.Unlock()

	// First player sets board size
	if s.store.GetBoardSize() == 0 {
		s.store.SetBoardSize(boardSize)
	}

	size := s.store.GetBoardSize()

	waiting := s.store.PopWaitingRoom()

	// ---------------- JOIN ROOM ----------------
	if waiting != nil && waiting.CanJoin() {

		waiting.Join(client)

		setRoom(waiting)
		setPlayer(2)

		// Notify players
		for i, p := range waiting.Players {

			p.Send(protocol.Msg{
				Type:      protocol.MsgMatchFound,
				MatchID:   waiting.ID,
				YouAre:    i + 1,
				BoardSize: size,
			})
		}

		log.Println("Room started:", waiting.ID)
		return
	}

	// ---------------- CREATE ROOM ----------------

	roomID := newRoomID()

	newRoom := room.NewRoom(roomID, client, size)

	setRoom(newRoom)
	setPlayer(1)

	s.store.AddRoom(newRoom)

	client.Send(protocol.Msg{
		Type:      protocol.MsgWaiting,
		MatchID:   roomID,
		YouAre:    1,
		BoardSize: size,
	})

	log.Println("Created waiting room:", roomID)
}

func (s *MatchmakingService) HandleDisconnect(
	client room.ClientPort,
	r *room.Room,
) {

	if r == nil {
		return
	}

	s.mu.Lock()
	defer s.mu.Unlock()

	// Remove from room
	r.RemovePlayer(client)

	client.ClearRoom()

	// No players left → delete room
	if r.PlayerCount() == 0 {

		s.store.RemoveRoom(r.ID)

		log.Println("Room removed:", r.ID)
		return
	}

	// Notify remaining players
	for _, p := range r.Players {

		p.Send(protocol.Msg{
			Type: protocol.MsgOpponentLeft,
		})

		p.ClearRoom()
	}

	s.store.RemoveRoom(r.ID)

	log.Println("Match ended due to disconnect:", r.ID)
}
