package file

import "context"

// HostFileServer 驱动文件接口 #################################################################
type HostFileServer interface {
	// CopyFile 复制文件 =======================================================================
	CopyFile(ctx context.Context, sources []string, targets []string) ([]*BackFileAction, error)
	// MoveFile 移动文件 =======================================================================
	MoveFile(ctx context.Context, sources []string, targets []string) ([]*BackFileAction, error)
	// NameFile 移动文件 =======================================================================
	NameFile(ctx context.Context, sources []string, targets []string) ([]*BackFileAction, error)
	// ListFile 列举文件 =======================================================================
	ListFile(ctx context.Context, path []string, opt *ListFileOption) ([]*HostFileObject, error)
	// FindFile 搜索文件 =======================================================================
	FindFile(ctx context.Context, path []string, opt *FindFileOption) ([]*HostFileObject, error)
	// Download 获取文件 =======================================================================
	Download(ctx context.Context, path []string, opt *DownloadOption) ([]*LinkFileObject, error)
	// Uploader 上传文件 =======================================================================
	Uploader(ctx context.Context, path []string, opt *UploaderOption) ([]*BackFileAction, error)
	// KillFile 删除文件 =======================================================================
	KillFile(ctx context.Context, path []string, opt *KillFileOption) ([]*BackFileAction, error)
	// MakeFile 搜索文件 =======================================================================
	MakeFile(ctx context.Context, path []string, opt *MakeFileOption) ([]*BackFileAction, error)
	// MakePath 搜索文件 =======================================================================
	MakePath(ctx context.Context, path []string, opt *MakeFileOption) ([]*BackFileAction, error)
}
