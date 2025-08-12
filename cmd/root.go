package cmd

import (
	"fmt"
	"os"

	"github.com/OpenListTeam/OpenList/v5/cmd/flags"
	"github.com/spf13/cobra"
)

var RootCmd = &cobra.Command{
	Use:   "openlist",
	Short: "A file list program that supports multiple storage.",
	Long: `A file list program that supports multiple storage,
built with love by OpenListTeam.
Complete documentation is available at https://doc.oplist.org/`,
}

func Execute() {
	if err := RootCmd.Execute(); err != nil {
		fmt.Fprintln(os.Stderr, err)
		os.Exit(1)
	}
}

func init() {
	RootCmd.PersistentFlags().StringVarP(&flags.ConfigFile, "config", "c", "data/config.json", "config file")
	RootCmd.PersistentFlags().BoolVar(&flags.Debug, "debug", false, "start with debug mode")
	RootCmd.PersistentFlags().BoolVar(&flags.NoPrefix, "no-prefix", false, "disable env prefix")
	RootCmd.PersistentFlags().BoolVar(&flags.Dev, "dev", false, "start with dev mode")
	RootCmd.PersistentFlags().BoolVarP(&flags.ForceBinDir, "force-bin-dir", "f", false, "force to use the directory where the binary file is located as data directory")
	RootCmd.PersistentFlags().BoolVar(&flags.LogStd, "log-std", false, "force to log to std")
}
