package driver

import (
	"context"

	"github.com/OpenListTeam/OpenList/v5/proto/driver"
	"google.golang.org/protobuf/types/known/emptypb"
)

type GRPCServer struct {
	Impl PluginIF
}
type GRPCClient struct {
	client driver.DriverClient
}

func (s *GRPCServer) Config(ctx context.Context, in *emptypb.Empty) (*driver.Config, error) {
	config, err := s.Impl.Config(ctx)
	if err != nil {
		return nil, err
	}
	return &driver.Config{
		Name:        config.Name,
		Version:     config.Version,
		DefaultRoot: config.DefaultRoot,
	}, nil
}
func (p *GRPCClient) Config(ctx context.Context) (Config, error) {
	resp, err := p.client.Config(ctx, &emptypb.Empty{})
	if err != nil {
		return Config{}, err
	}
	return Config{
		Name:        resp.Name,
		Version:     resp.Version,
		DefaultRoot: resp.DefaultRoot,
	}, nil
}

var _ driver.DriverServer = (*GRPCServer)(nil)
var _ PluginIF = (*GRPCClient)(nil)
