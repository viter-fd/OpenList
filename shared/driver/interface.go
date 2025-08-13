package driver

import (
	"context"

	"github.com/OpenListTeam/OpenList/v5/proto/driver"
	"github.com/hashicorp/go-plugin"
	"google.golang.org/grpc"
)

func GetHandshakeConfig() plugin.HandshakeConfig {
	return plugin.HandshakeConfig{
		ProtocolVersion:  2,
		MagicCookieKey:   "OpenListTeam",
		MagicCookieValue: "openlist",
	}
}

type PluginIF interface {
	Config(ctx context.Context) (Config, error)
}

type Plugin struct {
	plugin.Plugin
	Impl PluginIF
}

func (p *Plugin) GRPCServer(broker *plugin.GRPCBroker, s *grpc.Server) error {
	driver.RegisterDriverServer(s, &GRPCServer{Impl: p.Impl})
	return nil
}

func (p *Plugin) GRPCClient(ctx context.Context, broker *plugin.GRPCBroker, c *grpc.ClientConn) (any, error) {
	return &GRPCClient{client: driver.NewDriverClient(c)}, nil
}

var _ plugin.GRPCPlugin = (*Plugin)(nil)
