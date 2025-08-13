package server

import (
	"github.com/OpenListTeam/OpenList/v5/cmd/flags"
	"github.com/OpenListTeam/OpenList/v5/internal/conf"
	"github.com/OpenListTeam/OpenList/v5/server/handles"
	"github.com/gin-contrib/cors"
	"github.com/gin-gonic/gin"
)

func Init(e *gin.Engine) {
	Cors(e)
	switch conf.SitePath {
	case "", "/":
	default:
		e.GET("/", func(c *gin.Context) {
			c.Redirect(302, conf.SitePath)
		})
	}
	g := e.Group(conf.SitePath)
	g.Any("/ping", func(c *gin.Context) {
		c.String(200, "pong")
	})
	if flags.Dev {
		g.POST("/plugin/register", handles.PluginRegister)
	}
}

func Cors(r *gin.Engine) {
	config := cors.DefaultConfig()
	config.AllowOrigins = conf.Conf.Cors.AllowOrigins
	config.AllowHeaders = conf.Conf.Cors.AllowHeaders
	config.AllowMethods = conf.Conf.Cors.AllowMethods
	r.Use(cors.New(config))
}
