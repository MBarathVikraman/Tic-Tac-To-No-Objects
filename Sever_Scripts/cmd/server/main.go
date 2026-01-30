package main

import (
	"log"
	"net/http"

	"tictactoe/internal/service"
	"tictactoe/internal/store"
	"tictactoe/internal/transport"
)

func main() {
	rs := store.NewRoomStore()
	mm := service.NewMatchmakingService(rs)
	ws := transport.NewWSHandler(mm)

	mux := http.NewServeMux()
	mux.Handle("/ws", ws)

	log.Println("listening :8080")
	log.Fatal(http.ListenAndServe(":8080", mux))
}
