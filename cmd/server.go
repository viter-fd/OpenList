package cmd

import (
	log "github.com/sirupsen/logrus"
	"github.com/spf13/cobra"
)

// ServerCmd represents the server command
var ServerCmd = &cobra.Command{
	Use:   "server",
	Short: "Start the server at the specified address",
	Long: `Start the server at the specified address
the address is defined in config file`,
	Run: func(cmd *cobra.Command, args []string) {
		Init()
		log.Println("Server exit")
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
