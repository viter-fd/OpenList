package conf

type Config struct {
	TempDir string `json:"temp_dir" env:"TEMP_DIR"`
}

func DefaultConfig() *Config {
	return &Config{
		TempDir: "temp",
	}
}

var Conf *Config
