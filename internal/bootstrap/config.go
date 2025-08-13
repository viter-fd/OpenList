package bootstrap

import (
	"net/url"
	"os"
	"path/filepath"
	"strings"

	"github.com/OpenListTeam/OpenList/v5/cmd/flags"
	"github.com/OpenListTeam/OpenList/v5/internal/conf"
	"github.com/OpenListTeam/OpenList/v5/pkg/utils"
	log "github.com/sirupsen/logrus"
)

func InitConfig() {
	if !filepath.IsAbs(flags.ConfigFile) {
		flags.ConfigFile = filepath.Join(flags.PWD(), flags.ConfigFile)
	}
	log.Infoln("reading config file", "@", flags.ConfigFile)

	if !utils.Exists(flags.ConfigFile) {
		log.Infoln("config file not exists, creating default config file")
		_, err := utils.CreateNestedFile(flags.ConfigFile)
		if err != nil {
			log.Fatalln("create config file", ":", err)
		}
		conf.Conf = conf.DefaultConfig()
		err = utils.WriteJsonToFile(flags.ConfigFile, conf.Conf)
		if err != nil {
			log.Fatalln("save default config file", ":", err)
		}
	} else {
		configBytes, err := os.ReadFile(flags.ConfigFile)
		if err != nil {
			log.Fatalln("reading config file", ":", err)
		}
		conf.Conf = conf.DefaultConfig()
		err = utils.Json.Unmarshal(configBytes, conf.Conf)
		if err != nil {
			log.Fatalln("unmarshal config", ":", err)
		}
		err = utils.WriteJsonToFile(flags.ConfigFile, conf.Conf)
		if err != nil {
			log.Fatalln("update config file", ":", err)
		}
	}

	// convert abs path
	configDir := filepath.Dir(flags.ConfigFile)
	convertAbsPath := func(path *string) {
		if *path != "" && !filepath.IsAbs(*path) {
			*path = filepath.Join(configDir, *path)
		}
	}
	convertAbsPath(&conf.Conf.TempDir)
	convertAbsPath(&conf.Conf.Scheme.CertFile)
	convertAbsPath(&conf.Conf.Scheme.KeyFile)
	convertAbsPath(&conf.Conf.Scheme.UnixFile)
	log.Debugf("config: %+v", conf.Conf)

	initSitePath()
}

func initSitePath() {
	if !strings.Contains(conf.Conf.SiteURL, "://") {
		conf.Conf.SiteURL = utils.FixAndCleanPath(conf.Conf.SiteURL)
	}
	u, err := url.Parse(conf.Conf.SiteURL)
	if err != nil {
		log.Fatalln("parse site_url", ":", err)
	}
	conf.SitePath = u.Path
}
