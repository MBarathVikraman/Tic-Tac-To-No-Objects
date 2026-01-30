package domain

func CheckWinner(board []int, size int) int {

	// rows
	for r := 0; r < size; r++ {
		win := true

		for c := 0; c < size; c++ {
			if board[r*size+c] == 0 ||
				board[r*size+c] != board[r*size] {
				win = false
				break
			}
		}

		if win {
			return board[r*size]
		}
	}

	// columns
	for c := 0; c < size; c++ {
		win := true

		for r := 0; r < size; r++ {
			if board[r*size+c] == 0 ||
				board[r*size+c] != board[c] {
				win = false
				break
			}
		}

		if win {
			return board[c]
		}
	}

	// diag \
	win := true
	for i := 0; i < size; i++ {
		if board[i*size+i] == 0 ||
			board[i*size+i] != board[0] {
			win = false
			break
		}
	}
	if win {
		return board[0]
	}

	// diag /
	win = true
	for i := 0; i < size; i++ {
		idx := i*size + (size - 1 - i)

		if board[idx] == 0 ||
			board[idx] != board[size-1] {
			win = false
			break
		}
	}
	if win {
		return board[size-1]
	}

	return 0
}

func IsDraw(b []int, size int) bool {
	for i := 0; i < size*size; i++ {
		if b[i] == 0 {
			return false
		}
	}
	return true
}
