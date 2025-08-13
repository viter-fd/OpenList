package utils

import (
	"fmt"
	"os"
	"path/filepath"
)

// Exists determine whether the file exists
func Exists(name string) bool {
	if _, err := os.Stat(name); err != nil {
		if os.IsNotExist(err) {
			return false
		}
	}
	return true
}

// CreateNestedFile create nested file
func CreateNestedFile(path string) (*os.File, error) {
	basePath := filepath.Dir(path)
	if err := os.MkdirAll(basePath, 0700); err != nil {
		return nil, fmt.Errorf("create nested dir: %w", err)
	}
	fi, err := os.Create(path)
	if err != nil {
		return nil, fmt.Errorf("create file: %w", err)
	}
	return fi, nil
}
