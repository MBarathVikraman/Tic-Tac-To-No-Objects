package protocol

type Msg struct {
	Type      string `json:"type"`
	Cell      int    `json:"cell,omitempty"`
	Value     int    `json:"value,omitempty"`
	NextTurn  int    `json:"nextTurn,omitempty"`
	MatchID   string `json:"matchId,omitempty"`
	YouAre    int    `json:"youAre,omitempty"`
	Message   string `json:"message,omitempty"`
	Winner    int    `json:"winner,omitempty"`
	BoardSize int    `json:"boardSize,omitempty"`
}

const (
	MsgFindMatch    = "find_match"
	MsgWaiting      = "waiting"
	MsgMatchFound   = "match_found"
	MsgMove         = "move"
	MsgGameOver     = "game_over"
	MsgOpponentLeft = "opponent_left"
	MsgError        = "error"
)
