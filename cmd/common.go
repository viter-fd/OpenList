package cmd

import (
	"github.com/OpenListTeam/OpenList/v5/cmd/flags"
	"github.com/OpenListTeam/OpenList/v5/internal/bootstrap"
	"github.com/sirupsen/logrus"
)

func Init() {
	InitLogrus()
	bootstrap.InitConfig()
}

func initLog(l *logrus.Logger) {
	if flags.Debug || flags.Dev {
		l.SetLevel(logrus.DebugLevel)
		l.SetReportCaller(true)
	} else {
		l.SetLevel(logrus.InfoLevel)
		l.SetReportCaller(false)
	}
}
func InitLogrus() {
	formatter := logrus.TextFormatter{
		ForceColors:               true,
		EnvironmentOverrideColors: true,
		TimestampFormat:           "2006-01-02 15:04:05",
		FullTimestamp:             true,
	}
	logrus.SetFormatter(&formatter)
	initLog(logrus.StandardLogger())
}
