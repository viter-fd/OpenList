package conf

type Scheme struct {
	Address      string `json:"address" env:"ADDR"`
	HttpPort     uint16 `json:"http_port" env:"HTTP_PORT"`
	HttpsPort    uint16 `json:"https_port" env:"HTTPS_PORT"`
	ForceHttps   bool   `json:"force_https" env:"FORCE_HTTPS"`
	CertFile     string `json:"cert_file" env:"CERT_FILE"`
	KeyFile      string `json:"key_file" env:"KEY_FILE"`
	UnixFile     string `json:"unix_file" env:"UNIX_FILE"`
	UnixFilePerm string `json:"unix_file_perm" env:"UNIX_FILE_PERM"`
	EnableH2c    bool   `json:"enable_h2c" env:"ENABLE_H2C"`
}
type Cors struct {
	AllowOrigins []string `json:"allow_origins" env:"ALLOW_ORIGINS"`
	AllowMethods []string `json:"allow_methods" env:"ALLOW_METHODS"`
	AllowHeaders []string `json:"allow_headers" env:"ALLOW_HEADERS"`
}

type Config struct {
	TempDir string `json:"temp_dir" env:"TEMP_DIR"`
	SiteURL string `json:"site_url" env:"SITE_URL"`
	Scheme  Scheme `json:"scheme"`
	Cors    Cors   `json:"cors" envPrefix:"CORS_"`
}

func DefaultConfig() *Config {
	return &Config{
		TempDir: "temp",
		Scheme: Scheme{
			Address:  "0.0.0.0",
			HttpPort: 5244,
		},
		Cors: Cors{
			AllowOrigins: []string{"*"},
			AllowMethods: []string{"*"},
			AllowHeaders: []string{"*"},
		},
	}
}
