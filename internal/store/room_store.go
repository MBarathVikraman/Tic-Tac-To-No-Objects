package store

import (
	"sync"

	"tictactoe/internal/room"
)

type RoomStore struct {
	mu           sync.Mutex
	rooms        map[string]*room.Room
	waitingRooms []string
	boardSize    int
}

func NewRoomStore() *RoomStore {
	return &RoomStore{
		rooms: make(map[string]*room.Room),
	}
}
func (s *RoomStore) SetBoardSize(size int) {
	s.boardSize = size
}

func (s *RoomStore) GetBoardSize() int {
	return s.boardSize
}

func (s *RoomStore) AddRoom(r *room.Room) {
	s.mu.Lock()
	defer s.mu.Unlock()
	s.rooms[r.ID] = r
	s.waitingRooms = append(s.waitingRooms, r.ID)
}

func (s *RoomStore) PopWaitingRoom() *room.Room {
	s.mu.Lock()
	defer s.mu.Unlock()

	for len(s.waitingRooms) > 0 {
		last := len(s.waitingRooms) - 1
		roomID := s.waitingRooms[last]
		s.waitingRooms = s.waitingRooms[:last]

		r, ok := s.rooms[roomID]
		if !ok {
			continue
		}
		if r.IsWaiting() {
			return r
		}
	}
	return nil
}

func (s *RoomStore) RemoveRoom(id string) {
	s.mu.Lock()
	defer s.mu.Unlock()
	delete(s.rooms, id)
}
