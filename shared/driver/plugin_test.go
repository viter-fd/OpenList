package driver

import (
	"context"
	"fmt"
	"os"
	"os/exec"
	"testing"
	"time"

	"github.com/hashicorp/go-plugin"
)

func TestGrpcPlugin(t *testing.T) {
	ctx, cancel := context.WithCancel(context.Background())
	defer cancel()
	c := plugin.NewClient(&plugin.ClientConfig{
		Cmd:             helperProcess("grpc"),
		HandshakeConfig: GetHandshakeConfig(),
		Plugins: plugin.PluginSet{
			"grpc": &Plugin{},
		},
		AllowedProtocols: []plugin.Protocol{plugin.ProtocolGRPC},
		SyncStdout:       os.Stdout,
		SyncStderr:       os.Stderr,
	})
	client, err := c.Client()
	if err != nil {
		t.Fatalf("err: %s", err)
	}

	// Pinging should work
	if err := client.Ping(); err != nil {
		t.Fatalf("should not err: %s", err)
	}
	// Grab the impl
	raw, err := client.Dispense("grpc")
	if err != nil {
		t.Fatalf("err should be nil, got %s", err)
	}

	storage, ok := raw.(PluginIF)
	if !ok {
		t.Fatalf("bad: %#v", raw)
	}

	conf, err := storage.Config(ctx)
	if err != nil {
		t.Fatalf("should not err: %s", err)
	}
	fmt.Printf("config: %+v\n", conf)

	<-time.After(100 * time.Millisecond)
	c.Kill()
	cancel()
	if err := client.Ping(); err == nil {
		t.Fatal("should error")
	}

	// Try logging, this should show out in tests. We have to manually verify.
	t.Logf("HELLO")
}

// This is not a real test. This is just a helper process kicked off by
// tests.
func TestHelperProcess(*testing.T) {
	if os.Getenv("GO_WANT_HELPER_PROCESS") != "1" {
		return
	}

	defer os.Exit(0)

	args := os.Args
	for len(args) > 0 {
		if args[0] == "--" {
			args = args[1:]
			break
		}

		args = args[1:]
	}

	if len(args) == 0 {
		fmt.Fprintf(os.Stderr, "No command\n")
		os.Exit(2)
	}
	cmd := args[0]
	// args = args[1:]
	switch cmd {
	case "grpc":
		plugin.Serve(&plugin.ServeConfig{
			HandshakeConfig: GetHandshakeConfig(),
			Plugins: plugin.PluginSet{
				"grpc": &Plugin{
					Impl: &TestServer{},
				},
			},
			GRPCServer: plugin.DefaultGRPCServer,
		})
	default:
		fmt.Fprintf(os.Stderr, "Unknown command %q\n", cmd)
		os.Exit(2)
	}
}

func helperProcess(s ...string) *exec.Cmd {
	cs := []string{"-test.run=TestHelperProcess", "--"}
	cs = append(cs, s...)
	env := []string{
		"GO_WANT_HELPER_PROCESS=1",
	}

	cmd := exec.Command(os.Args[0], cs...)
	cmd.Env = append(env, os.Environ()...)
	return cmd
}

type TestServer struct{}

func (s *TestServer) Config(ctx context.Context) (Config, error) {
	fmt.Println("TestServer Config called")
	return Config{
		Name:        "TestPlugin",
		Version:     "v0.0.1",
		DefaultRoot: "/",
	}, nil
}

var _ PluginIF = (*TestServer)(nil)
