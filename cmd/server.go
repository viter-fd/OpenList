package cmd

import (
	"context"
	"errors"
	"fmt"
	"net"
	"net/http"
	"os"
	"os/signal"
	"strconv"
	"sync"
	"syscall"
	"time"

	"github.com/OpenListTeam/OpenList/v5/cmd/flags"
	"github.com/OpenListTeam/OpenList/v5/internal/conf"
	"github.com/OpenListTeam/OpenList/v5/server"
	"github.com/gin-gonic/gin"
	log "github.com/sirupsen/logrus"
	"github.com/spf13/cobra"
	"golang.org/x/net/http2"
	"golang.org/x/net/http2/h2c"
)

// ServerCmd represents the server command
var ServerCmd = &cobra.Command{
	Use:   "server",
	Short: "Start the server at the specified address",
	Long: `Start the server at the specified address
the address is defined in config file`,
	Run: func(_ *cobra.Command, args []string) {
		serverCtx, serverCancel := context.WithCancel(context.Background())
		defer serverCancel()
		Init(serverCtx)

		if !flags.Debug {
			gin.SetMode(gin.ReleaseMode)
		}
		r := gin.New()
		r.Use(gin.LoggerWithWriter(log.StandardLogger().Out))
		r.Use(gin.RecoveryWithWriter(log.StandardLogger().Out))
		server.Init(r)

		var httpHandler http.Handler = r
		if conf.Conf.Scheme.EnableH2c {
			httpHandler = h2c.NewHandler(r, &http2.Server{})
		}
		var httpSrv, httpsSrv, unixSrv *http.Server
		if conf.Conf.Scheme.HttpPort > 0 {
			httpBase := fmt.Sprintf("%s:%d", conf.Conf.Scheme.Address, conf.Conf.Scheme.HttpPort)
			log.Infoln("start HTTP server", "@", httpBase)
			httpSrv = &http.Server{Addr: httpBase, Handler: httpHandler}
			go func() {
				err := httpSrv.ListenAndServe()
				if err != nil && !errors.Is(err, http.ErrServerClosed) {
					log.Errorln("start HTTP server", ":", err)
					serverCancel()
				}
			}()
		}
		if conf.Conf.Scheme.HttpsPort > 0 {
			httpsBase := fmt.Sprintf("%s:%d", conf.Conf.Scheme.Address, conf.Conf.Scheme.HttpsPort)
			log.Infoln("start HTTPS server", "@", httpsBase)
			httpsSrv = &http.Server{Addr: httpsBase, Handler: r}
			go func() {
				err := httpsSrv.ListenAndServeTLS(conf.Conf.Scheme.CertFile, conf.Conf.Scheme.KeyFile)
				if err != nil && !errors.Is(err, http.ErrServerClosed) {
					log.Errorln("start HTTPS server", ":", err)
					serverCancel()
				}
			}()
		}
		if conf.Conf.Scheme.UnixFile != "" {
			log.Infoln("start Unix server", "@", conf.Conf.Scheme.UnixFile)
			unixSrv = &http.Server{Handler: httpHandler}
			go func() {
				listener, err := net.Listen("unix", conf.Conf.Scheme.UnixFile)
				if err != nil {
					log.Errorln("start Unix server", ":", err)
					serverCancel()
					return
				}

				mode, err := strconv.ParseUint(conf.Conf.Scheme.UnixFilePerm, 8, 32)
				if err != nil {
					log.Errorln("parse unix_file_perm", ":", err)
				} else {
					err = os.Chmod(conf.Conf.Scheme.UnixFile, os.FileMode(mode))
					if err != nil {
						log.Errorln("chmod socket file", ":", err)
					}
				}

				err = unixSrv.Serve(listener)
				if err != nil && !errors.Is(err, http.ErrServerClosed) {
					log.Errorln("start Unix server", ":", err)
					serverCancel()
				}
			}()
		}

		quit := make(chan os.Signal, 1)
		// kill (no param) default send syscanll.SIGTERM
		// kill -2 is syscall.SIGINT
		// kill -9 is syscall. SIGKILL but can"t be catch, so don't need add it
		signal.Notify(quit, syscall.SIGINT, syscall.SIGTERM)
		select {
		case <-quit:
		case <-serverCtx.Done():
		}

		log.Println("shutdown server...")
		Release()

		quitCtx, quitCancel := context.WithTimeout(context.Background(), time.Second)
		defer quitCancel()
		var wg sync.WaitGroup
		if httpSrv != nil {
			wg.Add(1)
			go func() {
				defer wg.Done()
				if err := httpSrv.Shutdown(quitCtx); err != nil {
					log.Errorln("shutdown HTTP server", ":", err)
				}
			}()
		}
		if httpsSrv != nil {
			wg.Add(1)
			go func() {
				defer wg.Done()
				if err := httpsSrv.Shutdown(quitCtx); err != nil {
					log.Errorln("shutdown HTTPS server", ":", err)
				}
			}()
		}
		if unixSrv != nil {
			wg.Add(1)
			go func() {
				defer wg.Done()
				if err := unixSrv.Shutdown(quitCtx); err != nil {
					log.Errorln("shutdown Unix server", ":", err)
				}
			}()
		}
		wg.Wait()
		log.Println("server exit")
	},
}

func init() {
	RootCmd.AddCommand(ServerCmd)
}

// OutOpenListInit 暴露用于外部启动server的函数
func OutOpenListInit() {
	var (
		cmd  *cobra.Command
		args []string
	)
	ServerCmd.Run(cmd, args)
}
