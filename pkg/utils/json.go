package utils

import (
	stdjson "encoding/json"
	"fmt"
	"os"

	json "github.com/json-iterator/go"
)

var Json = json.ConfigCompatibleWithStandardLibrary

// WriteJsonToFile write struct to json file
func WriteJsonToFile(dst string, data any, std ...bool) error {
	var (
		str []byte
		err error
	)
	if len(std) > 0 && std[0] {
		str, err = stdjson.MarshalIndent(data, "", "  ")
	} else {
		str, err = json.MarshalIndent(data, "", "  ")
	}
	if err != nil {
		return fmt.Errorf("marshal json: %w", err)
	}
	err = os.WriteFile(dst, str, 0777)
	if err != nil {
		return fmt.Errorf("write file: %w", err)
	}
	return nil
}
