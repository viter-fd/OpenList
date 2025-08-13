package model

type PluginReattachConfig struct {
	Protocol string `json:"protocol,omitempty"` // Protocol type (e.g., "grpc", "netrpc")
	Version  int    `json:"version,omitempty"`  // Protocol version
	Network  string `json:"network,omitempty"`  // Network type (e.g., "tcp", "unix")
	Address  string `json:"address,omitempty"`  // Address
}
