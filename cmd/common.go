package cmd

import (
	"context"

	"github.com/OpenListTeam/OpenList/v5/cmd/flags"
	"github.com/OpenListTeam/OpenList/v5/internal/bootstrap"
	"github.com/sirupsen/logrus"
)

func Init(ctx context.Context) {
	if flags.Dev {
		flags.Debug = true
	}
	initLogrus()
	bootstrap.InitConfig()
	bootstrap.InitDriverPlugins()
}

func Release() {

}

func initLog(l *logrus.Logger) {
	if flags.Debug {
		l.SetLevel(logrus.DebugLevel)
		l.SetReportCaller(true)
	} else {
		l.SetLevel(logrus.InfoLevel)
		l.SetReportCaller(false)
	}
}
func initLogrus() {
	formatter := logrus.TextFormatter{
		ForceColors:               true,
		EnvironmentOverrideColors: true,
		TimestampFormat:           "2006-01-02 15:04:05",
		FullTimestamp:             true,
	}
	logrus.SetFormatter(&formatter)
	initLog(logrus.StandardLogger())
}
