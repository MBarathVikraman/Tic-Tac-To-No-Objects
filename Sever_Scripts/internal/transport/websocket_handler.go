package transport

import (
	"encoding/json"
	"net/http"

	"github.com/gorilla/websocket"

	"tictactoe/internal/protocol"
	"tictactoe/internal/room"
	"tictactoe/internal/service"
)

type WSHandler struct {
	upgrader websocket.Upgrader
	mm       *service.MatchmakingService
}

func NewWSHandler(mm *service.MatchmakingService) *WSHandler {
	return &WSHandler{
		upgrader: websocket.Upgrader{
			CheckOrigin: func(r *http.Request) bool { return true },
		},
		mm: mm,
	}
}

type Client struct {
	conn   *websocket.Conn
	send   chan []byte
	room   *room.Room
	player int
}

func (c *Client) PlayerValue() int { return c.player }

func (c *Client) Send(m protocol.Msg) {
	b, _ := json.Marshal(m)
	select {
	case c.send <- b:
	default:
	}
}
func (c *Client) SendRaw(b []byte) {
	c.send <- b
}

func (c *Client) SendError(text string) {
	c.Send(protocol.Msg{Type: protocol.MsgError, Message: text})
}

func (c *Client) ClearRoom() {
	c.room = nil
}

func (h *WSHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {
	conn, err := h.upgrader.Upgrade(w, r, nil)
	if err != nil {
		return
	}

	client := &Client{
		conn: conn,
		send: make(chan []byte, 32),
	}

	go func() {
		defer conn.Close()
		for msg := range client.send {
			_ = conn.WriteMessage(websocket.TextMessage, msg)
		}
	}()

	for {
		_, data, err := conn.ReadMessage()
		if err != nil {
			break
		}

		var m protocol.Msg
		if err := json.Unmarshal(data, &m); err != nil {
			client.SendError("bad json")
			continue
		}

		switch m.Type {
		case protocol.MsgFindMatch:
			h.mm.FindMatch(
				client,
				func(r *room.Room) { client.room = r },
				func(p int) { client.player = p },
				m.BoardSize,
			)

		case protocol.MsgMove:
			if client.room == nil {
				client.SendError("not in room")
				continue
			}
			client.room.TryMove(client, m.Cell)

		default:
			client.SendError("unknown type")
		}
	}

	close(client.send)
	h.mm.HandleDisconnect(client, client.room)
}
