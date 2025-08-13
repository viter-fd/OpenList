package common

import (
	"strings"

	"github.com/OpenListTeam/OpenList/v5/cmd/flags"
	"github.com/OpenListTeam/OpenList/v5/internal/conf"
	"github.com/gin-gonic/gin"
	log "github.com/sirupsen/logrus"
)

// ErrorResp is used to return error response
// @param l: if true, log error
func ErrorResp(c *gin.Context, err error, code int, l ...bool) {
	ErrorWithDataResp(c, err, code, nil, l...)
}

func ErrorWithDataResp(c *gin.Context, err error, code int, data any, l ...bool) {
	if len(l) > 0 && l[0] {
		if flags.Debug {
			log.Errorf("%+v", err)
		} else {
			log.Errorf("%v", err)
		}
	}
	c.JSON(200, Resp[any]{
		Code:    code,
		Message: hidePrivacy(err.Error()),
		Data:    data,
	})
	c.Abort()
}

func ErrorStrResp(c *gin.Context, str string, code int, l ...bool) {
	if len(l) != 0 && l[0] {
		log.Error(str)
	}
	c.JSON(200, Resp[any]{
		Code:    code,
		Message: hidePrivacy(str),
		Data:    nil,
	})
	c.Abort()
}

func hidePrivacy(msg string) string {
	for _, r := range conf.PrivacyReg {
		msg = r.ReplaceAllStringFunc(msg, func(s string) string {
			return strings.Repeat("*", len(s))
		})
	}
	return msg
}
