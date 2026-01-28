package domain

func CheckWinner(b [9]int) int {
	wins := [8][3]int{
		{0, 1, 2}, {3, 4, 5}, {6, 7, 8},
		{0, 3, 6}, {1, 4, 7}, {2, 5, 8},
		{0, 4, 8}, {2, 4, 6},
	}
	for _, w := range wins {
		a, c, d := w[0], w[1], w[2]
		if b[a] != 0 && b[a] == b[c] && b[c] == b[d] {
			return b[a]
		}
	}
	return 0
}

func IsDraw(b [9]int) bool {
	for i := 0; i < 9; i++ {
		if b[i] == 0 {
			return false
		}
	}
	return true
}
