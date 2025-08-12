package flags

import (
	"os"
	"path/filepath"

	"github.com/sirupsen/logrus"
)

var (
	ConfigFile  string
	Debug       bool
	NoPrefix    bool
	Dev         bool
	ForceBinDir bool
	LogStd      bool

	pwd string
)

// Program working directory
func PWD() string {
	if pwd != "" {
		return pwd
	}
	if ForceBinDir {
		ex, err := os.Executable()
		if err != nil {
			logrus.Fatal(err)
		}
		pwd = filepath.Dir(ex)
		return pwd
	}
	d, err := os.Getwd()
	if err != nil {
		logrus.Fatal(err)
	}
	pwd = d
	return d
}
