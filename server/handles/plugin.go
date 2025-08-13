package handles

import (
	"context"
	"fmt"
	"net"

	"github.com/OpenListTeam/OpenList/v5/internal/driver"
	"github.com/OpenListTeam/OpenList/v5/server/common"
	driverS "github.com/OpenListTeam/OpenList/v5/shared/driver"
	"github.com/OpenListTeam/OpenList/v5/shared/model"
	"github.com/gin-gonic/gin"
	"github.com/hashicorp/go-plugin"
	"github.com/hashicorp/go-plugin/runner"
	"github.com/sirupsen/logrus"
)

func PluginRegister(c *gin.Context) {
	var config model.PluginReattachConfig
	err := c.ShouldBind(&config)
	if err != nil {
		common.ErrorStrResp(c, "Invalid request: "+err.Error(), 400, true)
		return
	}

	protocol := plugin.Protocol(config.Protocol)
	if protocol != plugin.ProtocolGRPC {
		common.ErrorStrResp(c, "Unsupported protocol: "+config.Protocol, 400, true)
		return
	}

	var addr net.Addr
	switch config.Network {
	case "tcp":
		addr, err = net.ResolveTCPAddr("tcp", config.Address)
	case "unix":
		addr, err = net.ResolveUnixAddr("unix", config.Address)
	default:
		common.ErrorStrResp(c, "Unknown network type: "+config.Network, 400, true)
		return
	}
	if err != nil {
		common.ErrorStrResp(c, "Failed to resolve address: "+err.Error(), 400, true)
		return
	}
	reattach := &plugin.ReattachConfig{
		Protocol:        protocol,
		ProtocolVersion: config.Version,
		Addr:            addr,
		Test:            true,
		ReattachFunc: func() (runner.AttachedRunner, error) {
			conn, err := net.Dial(addr.Network(), addr.String())
			if err != nil {
				return nil, plugin.ErrProcessNotFound
			}
			_ = conn.Close()

			return &attachedRunner{
				id:  addr.String(),
				ctx: c.Request.Context(),
			}, nil

		},
	}
	storage, err := client(reattach)
	if err != nil {
		common.ErrorStrResp(c, "Failed to connect to plugin: "+err.Error(), 500, true)
		return
	}

	driverConf, err := storage.Config(c.Request.Context())
	if err != nil {
		common.ErrorStrResp(c, "Failed to get plugin config: "+err.Error(), 500, true)
		return
	}
	logrus.Infof("Plugin config: %+v", driverConf)
	<-c.Request.Context().Done()
}

func client(reattach *plugin.ReattachConfig) (driverS.PluginIF, error) {
	client := plugin.NewClient(&plugin.ClientConfig{
		HandshakeConfig:  driverS.GetHandshakeConfig(),
		Plugins:          driver.PluginMap,
		Reattach:         reattach,
		AllowedProtocols: []plugin.Protocol{plugin.ProtocolGRPC},
		SyncStdout:       logrus.StandardLogger().Out,
		SyncStderr:       logrus.StandardLogger().Out,
	})
	clientProtocol, err := client.Client()
	if err != nil {
		return nil, fmt.Errorf("create plugin client: %w", err)
	}
	rawIF, err := clientProtocol.Dispense("grpc")
	if err != nil {
		return nil, fmt.Errorf("dispense plugin interface: %w", err)
	}
	driverIF, ok := rawIF.(driverS.PluginIF)
	if !ok {
		return nil, fmt.Errorf("invalid plugin interface type: %T", rawIF)
	}
	return driverIF, nil
}

type attachedRunner struct {
	id  string
	ctx context.Context
	addrTranslator
}

func (c *attachedRunner) Wait(_ context.Context) error {
	<-c.ctx.Done()
	return nil
}

func (c *attachedRunner) Kill(_ context.Context) error {
	return nil
}

func (c *attachedRunner) ID() string {
	return c.id
}

type addrTranslator struct{}

func (*addrTranslator) PluginToHost(pluginNet, pluginAddr string) (string, string, error) {
	return pluginNet, pluginAddr, nil
}

func (*addrTranslator) HostToPlugin(hostNet, hostAddr string) (string, string, error) {
	return hostNet, hostAddr, nil
}
