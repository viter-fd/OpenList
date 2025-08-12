package bootstrap

import (
	"path/filepath"

	"github.com/OpenListTeam/OpenList/v5/cmd/flags"
	"github.com/OpenListTeam/OpenList/v5/internal/conf"
	log "github.com/sirupsen/logrus"
)

func InitConfig() {
	if !filepath.IsAbs(flags.ConfigFile) {
		flags.ConfigFile = filepath.Join(flags.PWD(), flags.ConfigFile)
	}
	log.Infof("reading config file: %s", flags.ConfigFile)
	conf.Conf = conf.DefaultConfig()

	// convert abs path
	configDir := filepath.Dir(flags.ConfigFile)
	convertAbsPath := func(path *string) {
		if !filepath.IsAbs(*path) {
			*path = filepath.Join(configDir, *path)
		}
	}
	convertAbsPath(&conf.Conf.TempDir)
	log.Debugf("config: %+v", conf.Conf)
}
