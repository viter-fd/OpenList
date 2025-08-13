package bootstrap

import (
	"github.com/OpenListTeam/OpenList/v5/internal/driver"
	driverS "github.com/OpenListTeam/OpenList/v5/shared/driver"
	"github.com/hashicorp/go-plugin"
)

func InitDriverPlugins() {
	driver.PluginMap = map[string]plugin.Plugin{
		"grpc": &driverS.Plugin{},
	}
}
